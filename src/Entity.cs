using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcod;

namespace DarkRL
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

        public Point Position
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
                return;
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

        public Entity(Level level, bool isPlayer=false)
        {
            ID = isPlayer ? 0 : Entity.CurrGUID++;
            Color = DefaultColor;
            Character = DefaultCharacter;
            ViewPriority = DefaultViewPriority;
            l = level;
        }
    }
}
