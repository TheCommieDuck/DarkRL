using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL.Entities
{
    class Lantern : LightSource
    {
        public Lantern(Level l)
            : base(l)
        {
            this.Name = "lantern";
            this.Character = '&';
            this.Color = TCODColor.brass;
            this.Intensity = 100;
            this.LightRadius = 12;
            this.ViewPriority = 30;
            this.Slot = EquipSlot.Lantern;
        }
    }

    class Torch : LightSource
    {
        public Torch(Level l)
            : base(l)
        {
            this.Name = "flaming torch";
            this.Character = 'i';
            this.Color = TCODColor.gold;
            this.Intensity = 40;
            this.LightRadius = 7;
            this.ViewPriority = 20;
        }
    }
}
