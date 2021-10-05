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
        public async Task<ActionResult<IEnumerable<string>>> GetCity(string name)
        {
            var places = _census.Places.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
            var townships = _census.Townships.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim() + " township");
            return Ok(places.Union(townships).Select(p => $"{p.Name}, {p.State.Abbreviation} - {p.Lat}, {p.Lon} - {p.Population}"));
        }

        
    }
}
