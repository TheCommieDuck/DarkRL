using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;


namespace DarkRL
{

    class Level
    {
        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        private Tile[,] tileData;

        private Dictionary<int, List<Entity>> entities = new Dictionary<int, List<Entity>>();

        private static List<Entity> EmptyEntityList = new List<Entity>();

        public Tile this[int x, int y]
        {
            get
            {
                if (x >= Width || y >= Height || x < 0 || y < 0)
                    return Tile.BlankTile;
                else
                    return tileData[x, y];
            }
        }

        public Entity GetVisibleEntity(int tileID)
        {
            return GetEntities(tileID).OrderByDescending(t => t.ViewPriority).FirstOrDefault();
        }

        public List<Entity> GetEntities(int tileID)
        {
            List<Entity> ents;
            entities.TryGetValue(tileID, out ents);
            return (ents == null) ? Level.EmptyEntityList : entities[tileID];
        }

        public void AddTileEntity(int tileID, Entity ent)
        {
            List<Entity> currentEntities;
            entities.TryGetValue(tileID, out currentEntities);
            if (currentEntities == null) //we have no existing entities
                entities[tileID] = new List<Entity>();
            entities[tileID].Add(ent);
        }

        public Level(ushort width, ushort height)
        {
            Width = width;
            Height = height;
            tileData = new Tile[width, height];

        }

        public void Generate()
        {
            DungeonGenerator generator = new DungeonGenerator(this);
            for (ushort x = 0; x < Width; ++x)
            {
                for (ushort y = 0; y < Height; ++y)
                {
                    //1. fill with walls.
                    Tile tile = new Tile(this, (x << 16 | y), Tile.Wall);
                    tileData[x, y] = tile;
                    if (TCODRandom.getInstance().getInt(0, 10) > 7)
                    {
                        //AddTileEntity(tile.ID, new Entity());
                    }
                }
            }

            generator.Generate();
        }       
    }
}
