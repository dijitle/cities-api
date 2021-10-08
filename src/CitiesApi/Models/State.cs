using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static CitiesApi.Models.StateData;

namespace CitiesApi.Models
{
    public class State
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        [JsonIgnore]
        public Place Capital { 
            get
            {
                return Places.Single(p => p.Name == StateCaptials[Name]);
            }
        }
        public StateRegion Region { get; set; }
        public StateDivision Division { get; set; }
        [JsonIgnore]
        public List<County> Counties { get; set; } = new List<County>();
        [JsonIgnore]
        public List<Place> Places { get; set; } = new List<Place>();
        [JsonIgnore]
        public List<Place> Townships { get; set; } = new List<Place>();

        public override string ToString()
        {
            return Name;
        }
    }
}
