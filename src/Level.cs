using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

//x, y, width, height, whether it was successful
using RoomGenerator = System.Func<int, int, DarkRL.Direction, bool>;

namespace DarkRL
{
    public enum Direction
    {
        North,
        South,
        West,
        East
    };

    class Cell
    {
        public bool IsRoom = false;
        public bool IsVisited = false;
        public bool LeftWall = true;
        public bool RightWall = true;
        public bool TopWall = true;
        public bool BottomWall = true;
        public int Count
        {
            get
            {
                int count = 0;
                if (!LeftWall)
                    count++;
                if (!RightWall)
                    count++;
                if (!TopWall)
                    count++;
                if (!BottomWall)
                    count++;
                return count;
            }
        }
    };

    class Level
    {

        public List<RoomGenerator> RoomGenerators = new List<RoomGenerator>();

        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        private Tile[,] tileData;

        private Dictionary<int, List<Entity>> entities = new Dictionary<int, List<Entity>>();

        private static List<Entity> EmptyEntityList = new List<Entity>();

        private static int MaxRooms = 600;

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
            
            RoomGenerators.Add(this.DigRectangularRoom);
            RoomGenerators.Add(this.DigRectangularRoom);
            RoomGenerators.Add(this.DigRectangularRoom);
            RoomGenerators.Add(this.DigRectangularRoom);
        }

        public void Generate()
        {

            Cell[,] visited = new Cell[Width, Height];
            List<Point> visitedIndices = new List<Point>();

            TCODRandom random = TCODRandom.getInstance();

            for (ushort x = 0; x < Width; ++x)
            {
                for (ushort y = 0; y < Height; ++y)
                {
                    //1. fill with walls.
                    Tile tile = new Tile(this, (x << 16 | y), Tile.Wall);
                    tileData[x, y] = tile;
                    visited[x, y] = new Cell();
                    if (TCODRandom.getInstance().getInt(0, 10) > 7)
                    {
                        //AddTileEntity(tile.ID, new Entity());
                    }
                }
            }

            //pick a random point, visit it
            Point currentPoint = new Point(random.getInt(0, Width - 1), random.getInt(0, Height - 1));
            visited[currentPoint.X, currentPoint.Y].IsVisited = true;
            visitedIndices.Add(currentPoint);

            while (visitedIndices.Count != Width * Height)
            {
                //go in a random direction
                //while it returns false - i.e. we cannot move from here, then select a random visited point
                int dir = RandomDirection(ref currentPoint, visited);
                while (dir == -1)
                {
                    currentPoint = DarkRL.SelectRandomFromList(visitedIndices);
                    dir = RandomDirection(ref currentPoint, visited);
                }

                visited[currentPoint.X, currentPoint.Y].IsVisited = true;
                visitedIndices.Add(currentPoint);
            }

            int sparseness = 12;
            for (int sparse = 0; sparse < sparseness; ++sparse)
            {
                for (ushort x = 0; x < Width; ++x)
                {
                    for (ushort y = 0; y < Height; ++y)
                    {
                        int count = 0;
                        if (!visited[x, y].LeftWall)
                            count++;
                        if (!visited[x, y].RightWall)
                            count++;
                        if (!visited[x, y].TopWall)
                            count++;
                        if (!visited[x, y].BottomWall)
                            count++;

                        if (visited[x,y].Count == 1 )
                        {
                            //if it's the left wall empty
                            if (!visited[x, y].LeftWall)
                            {
                                visited[x, y].LeftWall = true;
                                visited[x - 1, y].RightWall = true;
                            }
                            else if (!visited[x, y].RightWall)
                            {
                                visited[x, y].RightWall = true;
                                visited[x + 1, y].LeftWall = true;
                            }
                            else if (!visited[x, y].TopWall)
                            {
                                visited[x, y].TopWall = true;
                                visited[x, y - 1].BottomWall = true;
                            }
                            else if (!visited[x, y].BottomWall)
                            {
                                visited[x, y].BottomWall = true;
                                visited[x, y + 1].TopWall = true;
                            }
                            visited[x, y].IsVisited = false;
                        }
                    }
                }
            }

            for (int x = 1; x < Width-1; ++x)
            {
                for (int y = 1; y < Height-1; ++y)
                {
                    int count = (visited[x - 1, y - 1].IsVisited ? 1 : 0) + (visited[x, y - 1].IsVisited ? 1 : 0) + (visited[x + 1, y - 1].IsVisited ? 1 : 0) +
                        (visited[x, y - 1].IsVisited ? 1 : 0) + (visited[x, y].IsVisited ? 1 : 0) + (visited[x, y + 1].IsVisited ? 1 : 0) + 
                        (visited[x + 1, y - 1].IsVisited ? 1 : 0) + (visited[x + 1, y].IsVisited ? 1 : 0) + (visited[x + 1, y + 1].IsVisited ? 1 : 0);
                    if (visited[x, y].IsVisited || count > 7)
                    {
                        this[x, y].Data = Tile.Floor;
                    }
                    else
                        this[x, y].Data = Tile.Wall;
                }
            }
            //and now we can see about placing rooms. Just define them as a point (size)
            /*List<Point> rooms = new List<Point>();
            rooms.Add(new Point(20, 15));
            rooms.Add(new Point(15, 25));
            rooms.Add(new Point(15, 12));
            rooms.Add(new Point(20, 17));
            rooms.Add(new Point(10, 20));
            rooms.Add(new Point(15, 21));
            rooms.Add(new Point(15, 21));
            rooms.Add(new Point(15, 21));
            rooms.Add(new Point(15, 21));
            rooms.Add(new Point(15, 21));

            foreach (Point room in rooms)
            {
                int score = 10000000;
                Point bestScorePoint = new Point(-1, -1);
                for (int x = 1; x < Width - room.X - 1; ++x)
                {
                    for (int y = 1; y < Height - room.Y - 1; ++y)
                    {
                        int runningScore = 0;
                        //now iterate every cell of the room, which is (roomX+x, roomY+y)
                        for (int roomX = 0; roomX < room.X; ++roomX)
                        {
                            for (int roomY = 0; roomY < room.Y; ++roomY)
                            {
                                Cell left = visited[roomX + x - 1, roomY + y];
                                Cell right = visited[roomX + x + 1, roomY + y];
                                Cell top = visited[roomX + x, roomY + y - 1];
                                Cell bottom = visited[roomX + x - 1, roomY + y + 1];

                                if (left.IsVisited)
                                    runningScore++;
                                if (right.IsVisited)
                                    runningScore++;
                                if (top.IsVisited)
                                    runningScore++;
                                if (bottom.IsVisited)
                                    runningScore++;

                                if (visited[roomX + x, roomY + y].IsRoom)
                                    runningScore += 100;
                                else if (visited[roomX + x, roomY + y].IsVisited)
                                    runningScore += 3;
                            }
                        }
                        if (runningScore < score)
                        {
                            score = runningScore;
                            bestScorePoint = new Point(x, y);
                        }
                    }
                }
                for (int x = 0; x < room.X; ++x)
                {
                    for (int y = 0; y < room.Y; ++y)
                    {
                        if (x != 0 && y != 0 && x != room.X - 1 && y != room.Y - 1)
                        {
                            visited[x + bestScorePoint.X, y + bestScorePoint.Y].IsRoom = true;
                            visited[x + bestScorePoint.X, y + bestScorePoint.Y].IsVisited = true;
                        }
                    }
                }
            }*/
        }

        public int RandomDirection(ref Point curr, Cell[,] visited)
        {
            Point newCurr;
            HashSet<int> triedDirs = new HashSet<int>();
            int dir;
            do
            {
                newCurr = new Point(curr.X, curr.Y);
                dir = TCODRandom.getInstance().getInt(0, 3);
                triedDirs.Add(dir);
                if (triedDirs.Count == 4) //out of directions
                    return -1;
                switch (dir)
                {
                    case 1:
                        newCurr.Y += 1;
                        break;
                    case 2: //u
                        newCurr.Y -= 1;
                        break;
                    case 3: //r
                        newCurr.X += 1;
                        break;
                    case 0: //l
                        newCurr.X -= 1;
                        break;
                }
            } while (newCurr.X < 0 || newCurr.Y < 0 || newCurr.X >= Width || newCurr.Y >= Height || visited[newCurr.X, newCurr.Y].IsVisited);

            if (dir == 0)
            {
                visited[curr.X, curr.Y].LeftWall = false;
                visited[newCurr.X, newCurr.Y].RightWall = false;
            }
            else if (dir == 1)
            {
                visited[curr.X, curr.Y].BottomWall = false;
                visited[newCurr.X, newCurr.Y].TopWall = false;
            }
            else if (dir == 2)
            {
                visited[curr.X, curr.Y].TopWall = false;
                visited[newCurr.X, newCurr.Y].BottomWall = false;
            }
            else if (dir == 3)
            {
                visited[curr.X, curr.Y].RightWall = false;
                visited[newCurr.X, newCurr.Y].LeftWall = false;
            }
            curr = newCurr;
            return dir;
        }






        public void GenerateLevel()
        {
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
            List<Point> dugTiles = new List<Point>();
            
            //2. dig out a room in the middle of the map.
            int roomWidth = TCODRandom.getInstance().getInt(6, 20);
            int roomHeight = TCODRandom.getInstance().getInt(6, 20);
            int topX = (ushort)((Width - roomWidth) / 2);
            int topY = (ushort)(Height / 2);
            this[topX, topY].BackgroundColor = TCODColor.lightLime;
            DigRectangularRoom(topX, topY, Direction.South);

            int currentRooms = 1;
            int totalTries = 0;
            while (currentRooms < Level.MaxRooms && totalTries < 5000)
            {
                bool successful = false;
                int tries = 0;
                Point wall;
                //so we try digging..pick a point, pick a direction, pick a feature. we try putting a feature in 10 times, then we try up to 3 directions,
                //and if we can't fit it in that point at all we just get a new point.
                do
                {
                    //3. pick a random wall.
                    do
                    {
                        wall = new Point(TCODRandom.getInstance().getInt(0, Width), TCODRandom.getInstance().getInt(0, Height));
                    } while (!this[wall.X, wall.Y].IsWall || !HasAdjacentSpace(wall.X, wall.Y));

                    successful = TryDiggingRoom(wall);
                    tries++;
                } while (!successful && tries < 100); //I can't see tries ever hitting 100.
                currentRooms++;
                //this[wall.X, wall.Y].BackgroundColor = TCODColor.lightFuchsia;
                totalTries++;
            }
        }

        private bool HasAdjacentWall(int x, int y)
        {

            return this[x - 1, y].IsWall || this[x + 1, y].IsWall || this[x, y - 1].IsWall || this[x, y + 1].IsWall;
        }

        private bool HasAdjacentSpace(int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= Width-1 || y >= Height-1) //if it's on the edge of the level
                return false;
            return (!this[x - 1, y].IsWall) || (!this[x + 1, y].IsWall) || (!this[x, y - 1].IsWall) || (!this[x, y + 1].IsWall);
        }

        private bool AreaContainsSpace(int topX, int topY, int width, int height)
        {
 	        for (ushort x = 0; x < width; ++x)
            {
                for (ushort y = 0; y < height; ++y)
                {
                    if (!this[x + topX, y + topY].IsWall)
                        return true;
                }
            }
            return false;
        }

        private List<Direction> FindAdjacentWalls(int x, int y)
        {
            List<Direction> points = new List<Direction>();
            if (this[x - 1, y].IsWall)
                points.Add(Direction.West);
            if (this[x + 1, y].IsWall)
                points.Add(Direction.East);
            if (this[x, y - 1].IsWall)
                points.Add(Direction.North);
            if (this[x, y + 1].IsWall)
                points.Add(Direction.South);
            return points;
        }

        private void CentralToTopLeft(ref int x, ref int y, int width, int height, Direction direction)
        {
            switch (direction)
            {
                //point is bottom middle
                case Direction.North:
                    x -= width / 2;
                    y -= height;
                    break;
                //point is top middle
                case Direction.South:
                    x -= width / 2;
                    break;
                //point is right middle
                case Direction.West:
                    x -= width;
                    y -= height / 2;
                    break;
                //point is left middle
                case Direction.East:
                    y -= height / 2;
                    break;

            }
        }

        private bool TryDiggingRoom(Point wall)
        {
            //find which direction(s) are available and pick one at random.
            //just assume that since there is an adjacent space.
            int directionTries = 0;
            bool successful;
            do
            {
                List<Direction> possibleDirections = FindAdjacentWalls(wall.X, wall.Y);
                Direction dir = DarkRL.SelectRandomFromList(possibleDirections);

                //now select a feature..try up to 10 times, after that we decide it's impossible
                RoomGenerator newFeature;
                int featureTries = 0;
                do
                {
                    newFeature = DarkRL.SelectRandomFromList(RoomGenerators);
                    featureTries++;
                    successful = newFeature(wall.X, wall.Y, dir);
                } while (!successful && featureTries < 5); //whilst it was not successful and we have tries left, try again
                directionTries++;
            } while (!successful && directionTries < 3);
            return successful;
        }

        private bool DigRectangularRoom(int x, int y, Direction direction)
        {
            int width = 30;
            int height = 30; 
            //first convert from our centre point to our topleft/right
            CentralToTopLeft(ref x, ref y, width, height, direction);

            if(AreaContainsSpace(x-1, y-1, width+1, height+1))
                return false;

            for (ushort iterX = 0; iterX < width; ++iterX)
            {
                for (ushort iterY = 0; iterY < height; ++iterY)
                    this[x + iterX, y + iterY].Data = Tile.Floor;
            }

            return true;
        }

        private bool DigCorridor(int x, int y, Direction direction)
        {
            int length = TCODRandom.getInstance().getInt(5, 30); //TODO: random sizes, maybe make it go up and down
            switch (direction)
            {
                case Direction.North:
                    if (y - length <= 0 || AreaContainsSpace(x-1, y - length, 3, length))
                        return false;
                    for (int iterY = 0; iterY < length; ++iterY)
                        this[x, y - iterY].Data = Tile.Floor;
                    break;
                case Direction.South:
                    if (y + length >= Height - 1 || AreaContainsSpace(x-1, y, 3, length)) //out of bounds
                        return false;
                    for (int iterY = 0; iterY < length; ++iterY)
                        this[x, y + iterY].Data = Tile.Floor;
                    break;
                case Direction.West:
                    if (x - length <= 0 || AreaContainsSpace(x - length, y-1, length, 3)) //out of bounds
                        return false;
                    for (int iterX = 0; iterX < length; ++iterX)
                        this[x - iterX, y].Data = Tile.Floor;
                    break;
                case Direction.East:
                    if (x + length >= Width + 1 || AreaContainsSpace(x, y-1, length, 3)) //out of bounds
                        return false;
                    for (int iterX = 0; iterX < length; ++iterX)
                        this[x + iterX, y].Data = Tile.Floor;
                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}
