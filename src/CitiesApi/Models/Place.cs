using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CitiesApi.Models
{
    public class Place
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AltName { 
            get
            {
                if (Name.Contains("("))
                {
                    return Name.Substring(Name.IndexOf("("), Name.IndexOf(")") - Name.IndexOf("("));
                }

                return Name.Replace("St.", "Saint").Replace("Urban ", "");
            } 
        }
        public bool IsCapital
        {
            get
            {
                return State.Capital == this;
            }
        }

        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        public string Classification { get; set; }
        public string County { 
            get
            {
                return string.Join(',', Counties.Select(c => c.Name));
            } 
        }

        public List<County> Counties { get; set; } = new List<County>();
        public State State { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}, {State.Name}";
        }
    }
}
