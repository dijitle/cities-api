using CitiesApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesApi.Services
{
    public class CensusService : ICensusService
    {
        public List<State> States { get; set; } = new List<State>();
        public List<County> Counties { get; set; } = new List<County>();
        public List<Place> Places { get; set; } = new List<Place>();
        public List<Place> Townships { get; set; } = new List<Place>();

        public CensusService()
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
                        Id = line[16],
                        Name = line[86],
                        Population = Convert.ToInt32(line[90]),
                        Lat = Convert.ToDouble(line[92]),
                        Lon = Convert.ToDouble(line[93]),
                        State = s
                    };
                    Counties.Add(c);
                    s.Counties.Add(c);
                }
                else if (line[2] == "060")
                {
                    if(line[90] == "0" ||
                       line[87].Split(" ").First().ToLower() == "township" ||
                       line[87].Split(" ").First().ToLower() == "district" ||
                       line[87].Split(" ").Last().ToLower() == "borough" ||
                       line[87].Split(" ").Last().ToLower() == "ccd" || 
                       line[87].Split(" ").Last().ToLower() == "cdp")
                    {
                        continue;
                    }

                    var p = new Place
                    {
                        Id = line[19],
                        Name = line[86],
                        Classification = line[87].Split(" ").Last(),
                        Population = Convert.ToInt32(line[90]),
                        Lat = Convert.ToDouble(line[92]),
                        Lon = Convert.ToDouble(line[93]),
                        State = s,
                        Counties = new List<County>() { Counties.Single(c => c.Id == line[16]) }
                    };

                    s.Townships.Add(p);
                    Townships.Add(p);
                    Counties.Single(c => c.Id == line[16]).Townships.Add(p);
                }
                else if (line[2] == "155")
                {
                    countyPlaceJoin.Add(new KeyValuePair<string, string>(line[16], line[31]));
                }
                else if (line[2] == "160")
                {
                    if (s.Townships.Any(t => t.Id == line[31]) || 
                       s.Townships.Any(t => t.Name == line[86] && t.Population == Convert.ToInt32(line[90])))
                    {
                        s.Townships.RemoveAll(t => t.Id == line[31]);
                        Townships.RemoveAll(t => t.Id == line[31]);
                        s.Townships.RemoveAll(t => t.Name == line[86] && t.Population == Convert.ToInt32(line[90]));
                        Townships.RemoveAll(t => t.Name == line[86] && t.Population == Convert.ToInt32(line[90]));
                    }

                    var p = new Place
                    {
                        Id = line[31],
                        Name = line[86].Split(" ").Last().ToLower() == "(balance)" || 
                               line[86].Split(" ").Last().ToLower() == "(county)" || 
                               line[86].Split(" ").Last().ToLower() == "county" ? line[86].Split(" ").First().Split("/").First().Split("-").First() : line[86],
                        Classification = line[86].Split(" ").Last().ToLower() == "(balance)" || 
                                         line[86].Split(" ").Last().ToLower() == "(county)" ||
                                         line[86].Split(" ").Last().ToLower() == "county" ? "city*" : line[87].Split(" ").Last(),
                        Population = Convert.ToInt32(line[90]),
                        Lat = Convert.ToDouble(line[92]),
                        Lon = Convert.ToDouble(line[93]),
                        State = s
                    };

                    s.Places.Add(p);
                    Places.Add(p);
                }
                else if (Convert.ToInt32(line[2]) > 160)
                {
                    break;
                }

            }

            foreach (var cpj in countyPlaceJoin)
            {
                var c = s.Counties.Single(c => c.Id == cpj.Key);
                var p = s.Places.Single(p => p.Id == cpj.Value);

                c.Places.Add(p);
                p.Counties.Add(c);
            }

            if(s.Places.GroupBy(p => new { p.Name, p.County }).Any(g => g.Count() > 1))
            {
                var groups = s.Places.GroupBy(p => new { p.Name, p.County }).Where(g => g.Count() > 1);

                foreach(var g in groups)
                {
                    var dupPlaces = g.Select(c => c).ToList();

                    dupPlaces.First().Classification = string.Join('/', dupPlaces.Select(p => p.Classification));
                    dupPlaces.First().Population = dupPlaces.Sum(p => p.Population);

                    for(int i = 1; i < dupPlaces.Count(); i++)
                    {
                        Places.Remove(dupPlaces[i]);
                        s.Places.Remove(dupPlaces[i]);
                    }
                }
                
            }
        }
    }
}