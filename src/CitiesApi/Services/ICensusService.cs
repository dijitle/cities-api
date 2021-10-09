using CitiesApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CitiesApi.Services
{
    public interface ICensusService
    {
        IEnumerable<County> Counties { get; set; }
        IEnumerable<Place> Places { get; set; }
        IEnumerable<Place> Townships { get; set; }
        IEnumerable<State> States { get; set; }
        Task GetData();
    }
}