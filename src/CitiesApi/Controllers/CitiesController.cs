using CitiesApi.Models;
using CitiesApi.Services;
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
        private readonly ICensusService _census;

        public CitiesController(ICensusService census)
        {
            _census = census;
        }

        [HttpGet()]
        [Route("city")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCity(string name, bool includeTownships = false)
        {
            var cities = _census.Places.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim() || p.AltName.ToLower().Trim() == name.ToLower().Trim());
            var townships = _census.Townships.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim() || p.AltName.ToLower().Trim() == name.ToLower().Trim());

            var places = includeTownships ? cities : cities.Union(townships);
            return Ok(places.Select(p => $"{p.Name} ({p.Classification}), {p.State.Abbreviation} - {p.County} - {p.Lat}, {p.Lon} - {p.Population}"));
        }

        [HttpGet()]
        [Route("cityPop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCityPopulation(int minPop = 100000)
        {
            var places = _census.Places.Where(p => p.Population > minPop);
            var townships = _census.Townships.Where(p => p.Population > minPop);
            return Ok(places.OrderByDescending(c => c.Population).Select(p => $"{p.Name} [{p.Classification}], {p.State.Abbreviation} - {p.County} - {p.Lat}, {p.Lon} - {p.Population}"));
        }


        [HttpGet()]
        [Route("countyStatePop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCountyStatePopulation()
        {
            var states = new List<string>();
            var total = 0;

            foreach (var s in _census.States)
            {
                total += s.Population;
                states.Add($"{s.Name} - {s.Population}");
                states.Add($"           {s.Counties.Select(c => c.Population).Sum()}");
                states.Add($"           {s.Population - s.Counties.Select(c => c.Population).Sum()}");
            }

            states.Add($"total = {total}");

            return Ok(states);
        }


        [HttpGet()]
        [Route("countyPop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCountyPopulation()
        {
            var states = new List<string>();
            var total = 0;

            foreach (var c in _census.Counties)
            {
                total += c.Population;
                if (c.Population - c.Townships.Select(t => t.Population).Sum() != 0)
                {
                    states.Add($"{c.Name} - {c.State.Abbreviation}");
                    states.Add($"           {c.Population - c.Townships.Select(t => t.Population).Sum()}");
                }
            }

            states.Add($"total = {total}");

            return Ok(states);
        }

        [HttpGet()]
        [Route("townshipClass")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetTownshipClassification()
        {

            return Ok(_census.Townships.Select(t => t.Classification).Distinct());
        }

    }
}



