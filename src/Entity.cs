using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcod;

namespace DarkRL
{
    class Entity
    {
        public static TCODColor DefaultColor = TCODColor.green;
        public static char DefaultCharacter = 'A';
        public static int DefaultViewPriority = 0;

        public int ViewPriority { get; private set; }

        public TCODColor Color { get; private set; }

        public char Character { get; private set; }

        public Entity()
        {
            Color = DefaultColor;
            Character = DefaultCharacter;
            ViewPriority = DefaultViewPriority;
        }
    }
}
