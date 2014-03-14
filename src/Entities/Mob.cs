using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL.Entities
{
    class Mob : Entity
    {
        public Backpack Backpack;

        public Stat HP { get; protected set; }

        public void DropItem(Item i)
        {
            Backpack.RemoveItem(i);
            i.Owner = null;
            level.AddEntityAtPos(Tile.PositionToID(this.Position), i);
        }

        public virtual void PickupItem(Item i)
        {
            level.RemoveEntityAtPos(i);
            AddItem(i);
        }

        public void AddItem(Item i)
        {
            Backpack.AddItem(i);
        }

        public Mob(Level l, String name = "something evil looking", int HPval = 20, bool isPlayer = false)
            : base(l, name, isPlayer)
        {
            Backpack = new Backpack(this);
            if (isPlayer)
                HP = new Stat("HP", HPval, true, Player.HPDesc);
            else
                HP = new Stat("HP", HPval);
        }
    }
}
