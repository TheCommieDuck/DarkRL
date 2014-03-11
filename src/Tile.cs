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

        public bool IsWalkable { get; set; }

        public bool IsObscuring { get; set; }

        public bool IsWall
        {
            set
            {
                IsWalkable = !value;
                IsObscuring = value;
            }
        }

        //public bool IsWall { }

        public TileData(TileData data)
        {
            BackgroundColor = data.BackgroundColor;
            IsWalkable = data.IsWalkable;
            IsObscuring = data.IsObscuring;
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

        public static TileData Floor = new TileData() { BackgroundColor = TCODColor.grey, IsWall = false };

        public static TileData Door = new TileData() { BackgroundColor = TCODColor.gold, IsWall = true };

        public static TileData Wall = new TileData() { BackgroundColor = TCODColor.darkGrey, IsWall = true };

        public static Tile BlankTile = new Tile(null, 0, Blank);

        public static Point IDToPosition(int id)
        {
            return new Point(id >> 16, (ushort)id);
        }

        public static int PositionToID(int x, int y)
        {
            return (x << 16 | (ushort)y);
        }

        public static int PositionToID(Point point)
        {
            return PositionToID(point.X, point.Y);
        }

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

        public bool IsWalkable 
        {
            get
            {
                return Data.IsWalkable;
            }
            set
            {
                Data = new TileData(Data);
                Data.IsWalkable = value;
            }
        }

        public bool IsObscuring
        {
            get
            {
                return Data.IsObscuring;
            }
            set
            {
                Data = new TileData(Data);
                Data.IsObscuring = value;
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
