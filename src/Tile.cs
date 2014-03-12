using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    public enum TileType
    {
        Floor,
        Wall,
        ClosedDoor,
        OpenDoor
    }

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

        public TileType Type { get; set; }

        public TileData(TileData data)
        {
            Color = data.Color;
            IsWalkable = data.IsWalkable;
            IsObscuring = data.IsObscuring;
            Character = data.Character;
            Type = data.Type;
        }

        public TileData()
        {
            Color = Tile.DefaultColor;
            IsWall = false;
            Type = TileType.Floor;
        }
    }

    class Tile
    {
        public static TCODColor DefaultColor = TCODColor.black;

        public static TileData Blank = new TileData(){ Color = DefaultColor };

        public static TileData Floor = new TileData() { Type = TileType.Floor, Color = TCODColor.lightGrey, Character = '.', IsWall = false };

        public static TileData ClosedDoor = new TileData() { Type = TileType.ClosedDoor, Color = TCODColor.gold, Character = '+', IsWall = true };

        public static TileData Wall = new TileData() { Type = TileType.Wall, Color = TCODColor.darkGrey, Character = '#', IsWall = true };

        public static TileData OpenLeftRightDoor = new TileData() { Type = TileType.OpenDoor, Color = TCODColor.gold, Character = '|', IsObscuring = false, IsWalkable = true };

        public static TileData OpenUpDownDoor = new TileData() { Type = TileType.OpenDoor, Color = TCODColor.gold, Character = '-', IsObscuring = false, IsWalkable = true };

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

        private TileData data;

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

        public TileType Type 
        {
            get
            {
                return Data.Type;
            }
            set
            {
                Data = new TileData(Data);
                Data.Type = value;
                level.SetLightingCellObscured(IDToPosition(ID), Data.IsObscuring);
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
                level.SetLightingCellObscured(IDToPosition(ID), value);
            }
        }

        public TileData Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public int ID { get; private set; }

        public void UpdateThisInLightingMap()
        {
            level.SetLightingCellObscured(IDToPosition(ID), this.IsObscuring);
        }
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
