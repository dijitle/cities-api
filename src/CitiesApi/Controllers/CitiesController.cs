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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Place>>> GetCity(string name, string stateName, bool includeTownships = false)
        {
            await _census.GetData();

            State state = null;

            if (! string.IsNullOrEmpty(stateName))
            {
                state = _census.States.SingleOrDefault(s => s.Name.ToLower() == stateName.ToLower().Trim() || s.Abbreviation.ToLower() == stateName.ToLower().Trim());

                if (state == null)
                {
                    return NotFound($"State '{state}' was not found!");
                }
            }

            Func<Place, bool> search = p => p.Name.ToLower().Trim() == name.ToLower().Trim() || p.AltName.ToLower().Trim() == name.ToLower().Trim();

            var cities = state == null ? _census.Places.Where(search) : state.Places.Where(search);

            if (includeTownships)
            {
                var townships = state == null ? _census.Townships.Where(search) : state.Townships.Where(search);
                var places = cities.Union(townships);

                if (places.Any())
                {
                    return Ok(places);
                }

                return NotFound($"Could not find place with name '{name}'");
            }

            if (cities.Any())
            {
                return Ok(cities);
            }

            return NotFound($"Could not find place with name '{name}'");
        }

        [HttpGet()]
        [Route("cities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Place>>> GetCities(string name, bool includeTownships = false)
        {
            await _census.GetData();

            var cities = _census.Places.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim() || p.AltName.ToLower().Trim() == name.ToLower().Trim());
            var townships = _census.Townships.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim() || p.AltName.ToLower().Trim() == name.ToLower().Trim());

            var places = includeTownships ? cities : cities.Union(townships);
            return Ok(places);
        }

        [HttpGet()]
        [Route("state")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<State>>> GetState(string stateName)
        {
            await _census.GetData();

            State state = null;

            if (!string.IsNullOrEmpty(stateName))
            {
                state = _census.States.SingleOrDefault(s => s.Name.ToLower() == stateName.ToLower().Trim() || s.Abbreviation.ToLower() == stateName.ToLower().Trim());

                if (state == null)
                {
                    return NotFound($"State '{state}' was not found!");
                }
            }

            return Ok(state);
        }

        [HttpGet()]
        [Route("states")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<State>>> GetStates()
        {
            await _census.GetData();

            return Ok(_census.States);
        }

        [HttpGet()]
        [Route("statesCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetStatesCount()
        {
            await _census.GetData();

            return Ok(_census.States.Count());
        }

        [HttpGet()]
        [Route("captial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Place>> GetCaptial(string stateName)
        {
            await _census.GetData();

            return Ok(_census.States.Single(s => s.Name.ToLower() == stateName.ToLower().Trim() || s.Abbreviation.ToLower() == stateName.ToLower().Trim()).Capital);
        }

        [HttpGet()]
        [Route("county")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<County>>> GetCounty(string countyName)
        {
            await _census.GetData();

            County county = null;

            if (!string.IsNullOrEmpty(countyName))
            {
                county = _census.Counties.SingleOrDefault(c => c.Name.ToLower() == countyName.ToLower().Trim());

                if (county == null)
                {
                    return NotFound($"County '{county}' was not found!");
                }
            }

            return Ok(county);
        }

        [HttpGet()]
        [Route("counties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<County>>> GetCounties()
        {
            await _census.GetData();

            return Ok(_census.Counties);
        }

        [HttpGet()]
        [Route("countiesCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetCountiesCount()
        {
            await _census.GetData();

            return Ok(_census.Counties.Count());
        }

        [HttpGet()]
        [Route("citiesPop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetCitiesPopulation(int minPop = 100000)
        {
            await _census.GetData();

            var places = _census.Places.Where(p => p.Population > minPop);
            var townships = _census.Townships.Where(p => p.Population > minPop);
            return Ok(places.Count() + townships.Count());
        }


        [HttpGet()]
        [Route("countyStatePop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCountyStatePopulation()
        {
            await _census.GetData();

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
        [Route("placeendincity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> getplacesendingincity()
        {
            await _census.GetData();


            return Ok(_census.Places.Sum(p => p.Population ) + _census.Townships.Sum(t => t.Population));
        }


        [HttpGet()]
        [Route("countyPop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCountyPopulation()
        {
            await _census.GetData();

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
            await _census.GetData();
            return Ok(_census.Townships.Select(t => t.Classification).Distinct());
        }

    }
}



