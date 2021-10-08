using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CitiesApi.Models
{

    public class County
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        [JsonIgnore]
        public List<Place> Places { get; set; } = new List<Place>();
        [JsonIgnore]
        public List<Place> Townships { get; set; } = new List<Place>();
        public State State { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}, {State.Name}";
        }
    }
}
