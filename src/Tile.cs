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
        public TCODColor Color { get; set; }

        public char Character { get; set; }

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
            Color = data.Color;
            IsWalkable = data.IsWalkable;
            IsObscuring = data.IsObscuring;
            Character = data.Character;
        }

        public TileData()
        {
            Color = Tile.DefaultColor;
            IsWall = false;
        }
    }

    class Tile
    {
        public static TCODColor DefaultColor = TCODColor.black;

        public static TileData Blank = new TileData(){ Color = DefaultColor };

        public static TileData Floor = new TileData() { Color = TCODColor.lightGrey, Character = '.', IsWall = false };

        public static TileData ClosedDoor = new TileData() { Color = TCODColor.gold, Character = '+', IsWall = true };

        public static TileData Wall = new TileData() { Color = TCODColor.darkGrey, Character = '#', IsWall = true };

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

        public TCODColor Color
        {
            get
            {
                return Data.Color;
            }
            set
            {
                Data = new TileData(Data);
                Data.Color = value;
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

        public char Character
        {
            get
            {
                return Data.Character;
            }
            set
            {
                Data = new TileData(Data);
                Data.Character = value;
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
