﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;
using DarkRL.Entities;


namespace DarkRL
{

    class Level
    {
        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        private Tile[,] tileData;

        private LightingMap lightingMap;

        private Dictionary<int, List<int>> entitiesOnTile = new Dictionary<int, List<int>>();

        private Dictionary<int, Entity> entityDict = new Dictionary<int, Entity>();

        private static List<int> EmptyEntityList = new List<int>();

        private bool needsLightingRecalc = true;

        public Entity Player
        {
            get
            {
                return GetEntity(0);
            }
        }

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

        public Tile this[Point point]
        {
            get
            {
                return this[point.X, point.Y];
            }
        }

        public Entity GetVisibleEntity(int tileID)
        {
            List<int> entitiesOnTile = GetEntities(tileID);
            Entity visEntity = null;
            foreach (int e in entitiesOnTile)
            {
                Entity curr = GetEntity(e);
                if (visEntity == null || curr.ViewPriority > visEntity.ViewPriority)
                    visEntity = curr;
            }
            return visEntity;
        }

        public Point GetEntityPosition(int ID)
        {
            return Tile.IDToPosition(entitiesOnTile.Where(t => t.Value.Contains(ID)).FirstOrDefault().Key);
        }

        public List<int> GetEntities(int tileID)
        {
            List<int> ents;
            entitiesOnTile.TryGetValue(tileID, out ents);
            return (ents == null) ? Level.EmptyEntityList : ents;
        }

        public Entity GetEntity(int id)
        {
            return entityDict[id];
        }

        public void AddEntity(int x, int y, Entity ent)
        {
            AddEntity(Tile.PositionToID(x, y), ent);
        }

        public void AddEntity(int tileID, Entity ent)
        {
            entityDict.Add(ent.ID, ent);
            AddEntityAtPos(tileID, ent);
        }

        public void AddEntity(Entity ent)
        {
            entityDict.Add(ent.ID, ent);
        }

        public void AddEntityAtPos(int tileID, Entity ent)
        {
            List<int> currentEntities;
            entitiesOnTile.TryGetValue(tileID, out currentEntities);
            if (currentEntities == null) //we have no existing entities
                entitiesOnTile[tileID] = new List<int>();
            entitiesOnTile[tileID].Add(ent.ID);
        }

        public void AddEntityAtPos(int x, int y, Entity ent)
        {
            AddEntityAtPos(Tile.PositionToID(x, y), ent);
        }

        public void RemoveEntityAtPos(Entity entity)
        {
            List<int> entitiesHere = GetEntities(Tile.PositionToID(entity.Position));
            entitiesHere.Remove(entity.ID);
        }

        public Level(ushort width, ushort height)
        {
            Width = width;
            Height = height;
            tileData = new Tile[width, height];
            lightingMap = new LightingMap(Width, Height);
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

            AddPlayer();

            AddFoobariumEverywhere();

            //setup our lighting et al
            for (ushort x = 0; x < Width; ++x)
            {
                for (ushort y = 0; y < Height; ++y)
                {
                    lightingMap.SetCellPropertyObscuring(x, y, this[x, y].IsObscuring);
                }
            }
        }

        private void AddFoobariumEverywhere()
        {
            for (int i = 0; i < 60; ++i)
            {
                Point fooPoint;
                do
                {
                    fooPoint = new Point(DarkRL.Random.getInt(0, Width), DarkRL.Random.getInt(0, Height));
                } while (!this[fooPoint].IsWalkable);

                this.AddEntity(this[fooPoint].ID, Item.NewBarOfFoobium(this));
            }
        }

        private void AddPlayer()
        {
            Point playerPoint;
            do
            {
                playerPoint = new Point(DarkRL.Random.getInt(0, Width), DarkRL.Random.getInt(0, Height));
            } while (!this[playerPoint].IsWalkable);

            Player player = new Player(this);
            Lantern lantern = new Lantern(this);
            //player.AddItem(lantern);
            player.Equip(lantern);
            this.AddEntity(this[playerPoint].ID, player);
            this.AddEntity(lantern);
            for (int i = 0; i < 5; ++i)
            {
                Torch t = new Torch(this);
                player.AddItem(t);
                this.AddEntity(t);
            }
        }

        public void Update()
        {
            LightingUpdate();
        }

        public void Draw(Window window, Camera camera)
        {
            for (int x = camera.Left; x < camera.Right; ++x)
            {
                for (int y = camera.Top; y < camera.Bottom; ++y)
                {
                    int windowX = x - camera.Left;
                    int windowY = y - camera.Top;
                    int light = lightingMap.GetLightLevel(x, y);
                    if (light != 0 || lightingMap.GetExplored(x, y))
                    {
                        float lightMod;
                        if (lightingMap[x, y].IsExplored && light == 0)
                            lightMod = LightingMap.ExploredLightScale;
                        else
                            lightMod = Math.Min(1f, ((light * (1- LightingMap.ExploredLightScale) / LightingMap.IntensityScaleFactor) 
                                +  LightingMap.ExploredLightScale));

                        window.Draw(lightMod,
                            this[x, y].VisibleEntity == null ? this[x, y].Color : this[x, y].VisibleEntity.Color,
                            this[x, y].VisibleEntity == null ? this[x, y].Character : this[x, y].VisibleEntity.Character, windowX, windowY);
                    }
                    else
                        window.Draw(TCODColor.black, windowX, windowY);
                }
            }
        }

        public void AddLightSource(LightSource Lantern)
        {
            lightingMap.AddLightSource(Lantern);
        }

        internal void NeedsLightingUpdate()
        {
            needsLightingRecalc = true;
        }

        public void LightingUpdate()
        {
            if (needsLightingRecalc)
                lightingMap.CalculateFOV();
            needsLightingRecalc = false;
        }

        public int GetLightLevel(Point p)
        {
            return lightingMap[p.X, p.Y].LightLevel;
        }

        public LightingCell GetLightingInformation(Point p)
        {
            return lightingMap[p.X, p.Y];
        }

        public void SetLightingCellObscured(Point pos, bool obscuring)
        {
            lightingMap.SetCellPropertyObscuring((ushort)pos.X, (ushort)pos.Y, obscuring);
        }
    }
}
