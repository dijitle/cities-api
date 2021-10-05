using CitiesApi.Models;
using System.Collections.Generic;

namespace CitiesApi.Services
{
    public interface ICensusService
    {
        List<County> Counties { get; set; }
        List<Place> Places { get; set; }
        List<State> States { get; set; }
    }
}