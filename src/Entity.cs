using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcod;

namespace DarkRL
{
    class Mob : Entity
    {
        public Backpack Backpack;

        public Stat HP { get; protected set;}

        public void DropItem(Item i)
        {
            Backpack.RemoveItem(i);
            i.Owner = null;
            l.AddEntityAtPos(Tile.PositionToID(this.Position), i);
        }

        public virtual void PickupItem(Item i)
        {
            l.RemoveEntityAtPos(i);
            Backpack.AddItem(i);
        }

        public Mob(Level l, String name = "something evil looking", int HPval = 20, bool isPlayer=false)
            :base(l, name, isPlayer)
        {
            Backpack = new Backpack(this);
            if (isPlayer)
                HP = new Stat("HP", HPval, true, Player.HPDesc);
            else
                HP = new Stat("HP", HPval);
        }
    }

    class Entity
    {
        public static int CurrGUID = 1;

        public static TCODColor DefaultColor = TCODColor.green;
        public static char DefaultCharacter = 'A';
        public static int DefaultViewPriority = 0;

        public int ViewPriority { get; protected set; }

        public int ID { get; private set; }

        public TCODColor Color { get; set; }

        public char Character { get; set; }

        public String Name { get; protected set; }

        public virtual Point Position
        {
            get
            {
                return l.GetEntityPosition(ID);
            }
        }

        public void Move(int x, int y)
        {
            Point pos = Position;
            Point newPos = new Point(pos.X + x, pos.Y + y);
            if (!l[newPos].IsWalkable)
            {
                if(l[newPos].Type != TileType.Wall)
                    DarkRL.WriteMessage("There is " + l[newPos].Name + " in the way.");
                return;
            }
            SetPosition(newPos);
        }

        public void SetPosition(Point newPos)
        {
            SetPosition(newPos.X, newPos.Y);
        }

        public virtual void SetPosition(int x, int y)
        {
            l.RemoveEntityAtPos(this);
            l.AddEntityAtPos(x, y, this);
        }

        protected Level l;

        public Entity(Level level, String name = "something", bool isPlayer=false)
        {
            ID = isPlayer ? 0 : Entity.CurrGUID++;
            Color = DefaultColor;
            Character = DefaultCharacter;
            ViewPriority = DefaultViewPriority;
            l = level;
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
