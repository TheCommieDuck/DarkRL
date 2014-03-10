using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class TileData
    {
        public TCODColor BackgroundColor { get; set; }

        public bool IsWall { get; set; }

        public TileData(TileData data)
        {
            BackgroundColor = data.BackgroundColor;
            IsWall = data.IsWall;
        }

        public TileData()
        {
            BackgroundColor = Tile.DefaultBackgroundColor;
            IsWall = false;
        }
    }

    class Tile
    {
        public static TCODColor DefaultBackgroundColor = TCODColor.darkerGrey;

        public static TileData Blank = new TileData(){ BackgroundColor = DefaultBackgroundColor };

        public static TileData Floor = new TileData() { BackgroundColor = TCODColor.white, IsWall = false };

        public static TileData Door = new TileData() { BackgroundColor = TCODColor.gold, IsWall = true };

        public static TileData Wall = new TileData() { BackgroundColor = TCODColor.black, IsWall = true };

        public static Tile BlankTile = new Tile(null, 0, Blank);

        public TCODColor BackgroundColor
        {
            get
            {
                return Data.BackgroundColor;
            }
            set
            {
                Data = new TileData(Data);
                Data.BackgroundColor = value;
            }
        }

        public bool IsWall
        {
            get
            {
                return Data.IsWall;
            }
            private set
            {
                Data = new TileData(Data);
                Data.IsWall = value;
            }
        }

        public TileData Data { get; set; }

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

        public Tile(Level l, int id, TileData data)
        {
            ID = id;
            level = l;
            Data = new TileData(data);
        }
    }
}
