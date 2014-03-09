using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class Tile
    {
        public static TCODColor DefaultBackgroundColor = TCODColor.darkerCyan;

        public static Tile BlankTile = new Tile(null, 0);

        public TCODColor BackgroundColor { get; set; }

        public int ID { get; private set; }

        public Entity VisibleEntity
        {
            get
            {
                if (level != null)
                    return level.GetVisibleEntity(ID);
                else
                    return null;
            }
        }

        private Level level;

        public Tile(Level l, int id)
        {
            ID = id;
            level = l;
            BackgroundColor = Tile.DefaultBackgroundColor;
        }
    }
}
