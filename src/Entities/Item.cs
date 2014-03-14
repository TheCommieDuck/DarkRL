using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;
using BackpackSlot = System.Collections.Generic.KeyValuePair<System.String, DarkRL.Entities.Item>;

namespace DarkRL.Entities
{
    class Item : Entity
    {
        public static Point InInventory = new Point(-69, -69);

        public static Item NewBarOfFoobium(Level l)
        {
            Item i = new Item(l);
            i.Name = "bar of foobium";
            i.Character = '-';
            i.Color = TCODColor.darkGrey;
            return i;
        }

        public Entity Owner { get; set; }

        public override Point Position
        {
            get
            {
                if (Owner == null)
                    return base.Position;
                else //then just use this
                    return Item.InInventory;
            }
        }

        public Item(Level l)
            :base(l)
        {
            ViewPriority = 0;
        }
    }

   
}
