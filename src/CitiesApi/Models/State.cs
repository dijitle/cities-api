using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public StateRegion Region { get; set; }
        public StateDivision Division { get; set; }
        [JsonIgnore]
        public List<County> Counties { get; set; } = new List<County>();
        [JsonIgnore]
        public List<Place> Places { get; set; } = new List<Place>();

        public override string ToString()
        {
            return Name;
        }

        public enum StateRegion
        {
            Northeast = 1,
            Midwest = 2,
            South = 3,
            West = 4,
            Puerto_Rico = 9
        }

        public enum StateDivision
        {
            Puerto_Rico = 0,
            New_England = 1,
            Middle_Atlantic = 2,
            East_North_Central = 3,
            West_North_Central = 4,
            South_Atlantic = 5,
            East_South_Central = 6,
            West_South_Central = 7,
            Mountain = 8,
            Pacific = 9
        }
    }
}
