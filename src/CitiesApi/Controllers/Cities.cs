using CitiesApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : Controller
    {
        public List<State> States { get; set; } = new List<State>();
        public List<County> Counties { get; set; } = new List<County>();
        public List<Place> Places { get; set; } = new List<Place>();

        [HttpGet()]
        [Route("hello")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetCity()
        {

            foreach (var stateFile in Directory.GetFiles("../../data"))
            {
                using (ZipArchive za = ZipFile.OpenRead(stateFile))
                {
                    foreach (ZipArchiveEntry entry in za.Entries)
                    {
                        if (entry.FullName.EndsWith("geo2020.pl"))
                        {
                            using (StreamReader rdr = new StreamReader(entry.Open()))
                            {
                                ParseFiles(rdr);
                            }
                        }

                    }
                }
            }
            return Ok("hello");
        }

        private void ParseFiles(StreamReader rdr)
        {
            var stateLine = rdr.ReadLine().Split("|");

            var s = new State
            {
                Id = stateLine[9],
                Abbreviation = stateLine[1],
                Name = stateLine[86],
                Population = Convert.ToInt32(stateLine[90]),
                Lat = Convert.ToDouble(stateLine[92]),
                Lon = Convert.ToDouble(stateLine[93]),
                Region = (State.StateRegion)Enum.Parse(typeof(State.StateRegion), stateLine[10]),
                Division = (State.StateDivision)Enum.Parse(typeof(State.StateDivision), stateLine[11])
            };

            States.Add(s);

            var countyPlaceJoin = new List<KeyValuePair<string, string>>();

            while (!rdr.EndOfStream)
            {
                var line = rdr.ReadLine().Split("|");
                if (line[2] == "050")
                {
                    var c = new County
                    {
                        Id = line[14],
                        Name = line[86],
                        Population = Convert.ToInt32(line[90]),
                        Lat = Convert.ToDouble(line[92]),
                        Lon = Convert.ToDouble(line[93]),
                        State = s
                    };
                    Counties.Add(c);
                    s.Counties.Add(c);
                }
                else if (line[2] == "155")
                {
                    countyPlaceJoin.Add(new KeyValuePair<string, string>(line[14], line[31]));
                }
                else if (line[2] == "160")
                {
                    var p = new Place
                    {
                        Id = line[31],
                        Name = line[86],
                        Classification = line[87].Split(" ").Last(),
                        Population = Convert.ToInt32(line[90]),
                        Lat = Convert.ToDouble(line[92]),
                        Lon = Convert.ToDouble(line[93]),
                        State = s
                    };

                    s.Places.Add(p);
                    Places.Add(p);
                }
            }

            foreach(var cpj in countyPlaceJoin)
            {
                var c = s.Counties.Single(c => c.Id == cpj.Key);
                var p = s.Places.Single(p => p.Id == cpj.Value);

                c.Places.Add(p);
                p.Counties.Add(c);
            }
        }
    }
}
