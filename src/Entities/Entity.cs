using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcod;

namespace DarkRL.Entities
{
    
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

        protected Level level;

        public virtual Point Position
        {
            get
            {
                return level.GetEntityPosition(ID);
            }
        }

        public void Move(int x, int y)
        {
            Point pos = Position;
            Point newPos = new Point(pos.X + x, pos.Y + y);
            if (!level[newPos].IsWalkable)
            {
                if(this.ID == Player.PlayerID && level[newPos].Type != TileType.Wall)
                    DarkRL.WriteMessage("There is " + level[newPos].Name + " in the way.");
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
            level.RemoveEntityAtPos(this);
            level.AddEntityAtPos(x, y, this);
        }


        public Entity(Level level, String name = "something", bool isPlayer=false)
        {
            ID = isPlayer ? Player.PlayerID : Entity.CurrGUID++;
            Color = DefaultColor;
            Character = DefaultCharacter;
            ViewPriority = DefaultViewPriority;
            this.level = level;
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
