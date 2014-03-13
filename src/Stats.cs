using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Stat
    {
        public String Name { get; private set; }

        public int MaxValue { get; set; }

        public int Value { get; set; }

        public Dictionary<int, String> Descriptions { get; set; }

        public Stat(String name, int max, bool usesDescription=false, Dictionary<int, String> descriptions=null)
        {
            Name = name;
            MaxValue = max;
            Value = max;
            Descriptions = descriptions;
        }

        public override string ToString()
        {
            if (Descriptions == null)
                return Value.ToString();
            else
            {
                return Descriptions[Descriptions.Keys.Where(t => t <= Value).First()];
            }
        }
    }
}
