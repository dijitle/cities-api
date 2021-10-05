using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesApi.Models
{
    public class Place
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        public string Classification { get; set; }
        public List<County> Counties { get; set; } = new List<County>();
        public State State { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}, {State.Name}";
        }
    }
}
