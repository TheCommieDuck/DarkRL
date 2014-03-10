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
                if (LeftWall)
                    count++;
                if (RightWall)
                    count++;
                if (TopWall)
                    count++;
                if (BottomWall)
                    count++;
                return count;
            }
        }
        public bool IsDeadEnd
        {
            get
            {
                return Count == 3;
            }
        }

        public Direction DeadEndDirection
        {
            get
            {
                if (!IsDeadEnd)
                    throw new InvalidOperationException();

                if (!TopWall)
                    return Direction.North;
                if (!BottomWall)
                    return Direction.South;
                if (!LeftWall)
                    return Direction.West;
                if (!RightWall)
                    return Direction.East;

                throw new InvalidOperationException();
            }
        }

        public bool IsCorridor
        {
            get
            {
                return Count < 4 && IsVisited;
            }
        }

        public bool IsRoom
        {
            get
            {
                return Count < 4 && !IsVisited;
            }
        }
    }

    public class DirectionPicker
    {
        private readonly List<Direction> directionsPicked = new List<Direction>();
        private Direction previous;
        private int changeMod;

        public bool HasNextDirection
        {
            get { return directionsPicked.Count < 4; }
        }

        public DirectionPicker(Direction prev, int change)
        {
            previous = prev;
            changeMod = change;
        }

        public Direction GetNextDirection()
        {
            if(!HasNextDirection) 
                throw new InvalidOperationException("No directions available");

            Direction directionPicked;

            do
            {
                directionPicked = MustChangeDirection ? (Direction)TCODRandom.getInstance().getInt(0, 3) : previous;
            } while (directionsPicked.Contains(directionPicked));

            directionsPicked.Add(directionPicked);

            return directionPicked;
        }

        public bool MustChangeDirection
        {
            get
            {
                return (directionsPicked.Count > 0) || changeMod > TCODRandom.getInstance().getInt(0, 99);
            }
        }
    }

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
            Point currentPoint = PickRandomPointToVisit(visited);
            MarkVisited(currentPoint, visited, visitedIndices);
            Direction previous = Direction.North;
            //visit all the others
            while (visitedIndices.Count != Width * Height)
            {
                DirectionPicker directionPicker = new DirectionPicker(previous, 30);
                Direction dir = directionPicker.GetNextDirection();

                while (!HasAdjacentCellInDirection(currentPoint, dir) || IsAdjacentCellVisited(currentPoint, dir, visited))
                {
                    if (directionPicker.HasNextDirection)

                        dir = directionPicker.GetNextDirection();
                    else
                    {
                        currentPoint = DarkRL.SelectRandomFromList(visitedIndices);
                        directionPicker = new DirectionPicker(previous, 30);
                        dir = directionPicker.GetNextDirection();
                    }
                }

                currentPoint = CreateCorridor(currentPoint, dir, visited);
                MarkVisited(currentPoint, visited, visitedIndices);
                previous = dir;
            }

            Sparsify(visited, 70);
            RemoveDeadEnds(visited, 100);

            PlaceRooms(10, 10, 40, 10, 40, visited);
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (visited[x, y].Count == 4)
                        this[x, y].Data = Tile.Floor;
                    if (visited[x, y].IsRoom)
                        this[x, y].BackgroundColor = TCODColor.grey;
                }
            }
            int i = 5;
            int j = 6;
            #region old
            /*Point currentPoint = new Point(random.getInt(0, Width - 1), random.getInt(0, Height - 1));
            visited[currentPoint.X, currentPoint.Y].IsVisited = true;
            visitedIndices.Add(currentPoint);

            while (visitedIndices.Count != Width * Height)
            {
                //go in a random direction
                //while it returns false - i.e. we cannot move from here, then select a random visited point
                int dir = RandomDirection(ref currentPoint, visited, -1);
                while (dir == -1)
                {
                    currentPoint = DarkRL.SelectRandomFromList(visitedIndices);
                    dir = RandomDirection(ref currentPoint, visited, dir);
                }

                visited[currentPoint.X, currentPoint.Y].IsVisited = true;
                visitedIndices.Add(currentPoint);
            }

            int sparseness = 20;
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
                    if (visited[x, y].IsVisited || visited[x, y].Count == 1 )
                    {
                        this[x, y].Data = Tile.Floor;
                        this[x - 1, y].Data = Tile.Floor;
                        this[x + 1, y].Data = Tile.Floor;
                        this[x, y - 1].Data = Tile.Floor;
                        this[x, y + 1].Data = Tile.Floor;
                    }
                    else
                    {
                        if(this[x,y].Data != Tile.Floor)
                            this[x, y].Data = Tile.Wall;
                    }
                }
            }
            //and now we can see about placing rooms. Just define them as a point (size)
            List<Point> rooms = new List<Point>();
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
            #endregion
        }

        private int CalculateRoomScore(Point location, Point room, Cell[,] visited)
        {
            int score = 0;
            for (int x = 0; x < room.X; ++x)
            {
                for (int y = 0; y < room.Y; ++y)
                {
                    Point dungeonLocation = new Point(location.X + x, location.Y + y);
                    if(dungeonLocation.X >= Width-1 || dungeonLocation.Y >= Height-1 || dungeonLocation.X == 0 || dungeonLocation.Y == 0)
                        return Int32.MaxValue;
                            // Add 1 point for each adjacent corridor to the cell
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.North, visited))
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.South, visited))
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.West, visited)) 
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.East, visited))
                        score++; 
                    // Add 3 points if the cell overlaps an existing corridor
                    if (visited[dungeonLocation.X, dungeonLocation.Y].IsCorridor) 
                        score += 3;
                    if(visited[dungeonLocation.X, dungeonLocation.Y].IsRoom)
                        score += 100;
                }
            }
            return score;
        }

        public bool AdjacentCellInDirectionIsCorridor(Point point, Direction direction, Cell[,] visited)
        {
            Point adj = GetTargetLocation(point, direction);
            return visited[adj.X, adj.Y].IsCorridor;
        }

        public void PlaceRooms(int count, int minWidth, int maxWidth, int minHeight, int maxHeight, Cell[,] visited)
        {
            for (int i = 0; i < count; ++i)
            {
                int bestScore = Int32.MaxValue;
                int oldScore = Int32.MaxValue;
                Point bestPoint = new Point(-1, -1);
                Point room = CreateRoom(minWidth, maxWidth, minHeight, maxHeight);
                List<Point> attempts = new List<Point>();
                //try some points
                for(int tries = 0; tries < Math.Min(Width*Height, 1000); ++tries)
                    attempts.Add(new Point(TCODRandom.getInstance().getInt(0, Width - room.X), TCODRandom.getInstance().getInt(0, Height - room.Y)));
                foreach (Point tryPoint in attempts)
                {
                    bestScore = Math.Min(CalculateRoomScore(tryPoint, room, visited), bestScore);
                    if (bestScore != oldScore)
                    {
                        oldScore = bestScore;
                        bestPoint = tryPoint;
                    }
                }
                PlaceRoom(bestPoint, room, visited);
            }
        }

        public void PlaceRoom(Point location, Point room, Cell[,] visited)
        {
            for (int x = 0; x < room.X; ++x)
            {
                for (int y = 0; y < room.Y; ++y)
                {
                    if (x != 0 && y != 0 && y != room.Y - 1 && x != room.X - 1)
                    {
                        visited[location.X + x, location.Y + y].IsVisited = false;
                        visited[location.X + x, location.Y + y].TopWall = false;
                        visited[location.X + x, location.Y + y].LeftWall = false;
                        visited[location.X + x, location.Y + y].RightWall = false;
                        visited[location.X + x, location.Y + y].BottomWall = false;
                    }
                    else
                    {
                        visited[location.X + x, location.Y + y].TopWall = true;
                        visited[location.X + x, location.Y + y].LeftWall = true;
                        visited[location.X + x, location.Y + y].RightWall = true;
                        visited[location.X + x, location.Y + y].BottomWall = true;
                    }
                }
            }
        }

        public Point CreateRoom(int minRoomWidth, int maxRoomWidth, int minRoomHeight, int maxRoomHeight)
        {
            return new Point(TCODRandom.getInstance().getInt(minRoomWidth, maxRoomWidth), TCODRandom.getInstance().getInt(minRoomHeight, maxRoomHeight));
        }

        private bool ShouldRemoveDeadEnd(int removeFactor)
        {
            return removeFactor > TCODRandom.getInstance().getInt(0, 99);
        }

        private void RemoveDeadEnds(Cell[,] visited, int removalModifier)
        {
            foreach (Point p in DeadEndLocations(visited))
            {
                if (ShouldRemoveDeadEnd(removalModifier))
                {
                    Point current = p;
                    do
                    {
                        DirectionPicker dirPicker = new DirectionPicker(visited[current.X, current.Y].DeadEndDirection, 100);
                        Direction direction = dirPicker.GetNextDirection();
                        while(!HasAdjacentCellInDirection(current, direction))
                        {
                            if(dirPicker.HasNextDirection)
                            {
                                direction = dirPicker.GetNextDirection();
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        current = CreateCorridor(current, direction, visited);
                    }while(visited[current.X,current.Y].IsDeadEnd);
                }
            }
        }

        private void Sparsify(Cell[,] visited, int sparseness)
        {
            int cellsToRemove = (int)Math.Ceiling(((float)sparseness / 100) * Width * Height);
            IEnumerator<Point> enumerator = DeadEndLocations(visited).GetEnumerator();

            for(int i = 0; i < cellsToRemove; ++i)
            {
                if (!enumerator.MoveNext()) //check for more
                {
                    enumerator = DeadEndLocations(visited).GetEnumerator();
                    if (!enumerator.MoveNext())
                        break; //nomore

                }
                Point p = enumerator.Current;
                CreateSide(p, visited[p.X, p.Y].DeadEndDirection, visited, true);
            }
        }

        private void MarkVisited(Point p, Cell[,] visited, List<Point> indices)
        {
            visited[p.X, p.Y].IsVisited = true;
            indices.Add(p);
        }

        public IEnumerable<Point> DeadEndLocations(Cell[,] visited)
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (visited[x, y].IsDeadEnd)
                        yield return new Point(x, y);
                }
            }
        }

        public Point PickRandomPointToVisit(Cell[,] visited)
        {
            Point p = new Point(TCODRandom.getInstance().getInt(0, Width - 1), TCODRandom.getInstance().getInt(0, Height - 1));
            visited[p.X, p.Y].IsVisited = true;
            return p;
        }

        public bool HasAdjacentCellInDirection(Point location, Direction direction)
        {
            // Check that the location falls within the bounds of the map
            if ((location.X < 0) || (location.X >= Width) || (location.Y < 0) || (location.Y >= Height)) return false;

            // Check if there is an adjacent cell in the direction
            switch(direction)
            {
                case Direction.North:
                    return location.Y > 0;
                case Direction.South:
                    return location.Y < (Height-1);
                case Direction.West:
                    return location.X > 0;
                case Direction.East:
                    return location.X < (Width-1);
                default:
                    return false;
            }
        }

        public bool IsAdjacentCellVisited(Point location, Direction direction, Cell[,] visited)
        {
            if (!HasAdjacentCellInDirection(location, direction)) 
                throw new InvalidOperationException("No adjacent cell exists for the location and direction provided.");

            switch(direction)
            {
                case Direction.North:
                    return visited[location.X, location.Y - 1].IsVisited;
                case Direction.West:
                    return visited[location.X - 1, location.Y].IsVisited;
                case Direction.South:
                    return visited[location.X, location.Y + 1].IsVisited;
                case Direction.East:
                    return visited[location.X + 1, location.Y].IsVisited;
                default:
                    throw new InvalidOperationException();

            }
        }

        public Point CreateCorridor(Point location, Direction direction, Cell[,] visited)
        {
            return CreateSide(location, direction, visited);
        }

        public Point CreateSide(Point location, Direction direction, Cell[,] visited, bool isBecomingWall=false)
        {

            if (!HasAdjacentCellInDirection(location, direction)) 
                throw new InvalidOperationException("No adjacent cell exists for the location and direction provided.");

            Point target = GetTargetLocation(location, direction);

            switch(direction)
            {
                case Direction.North:
                    visited[location.X, location.Y].TopWall = isBecomingWall;
                    visited[location.X, location.Y - 1].BottomWall = isBecomingWall;
                    break;

                case Direction.South:
                    visited[location.X, location.Y].BottomWall = isBecomingWall;
                    visited[location.X, location.Y + 1].TopWall = isBecomingWall;
                    break;

                case Direction.West:
                    visited[location.X, location.Y].LeftWall = isBecomingWall;
                    visited[location.X - 1, location.Y].RightWall = isBecomingWall;
                    break;

                case Direction.East:
                    visited[location.X, location.Y].RightWall = isBecomingWall;
                    visited[location.X + 1, location.Y].LeftWall = isBecomingWall;
                    break;
            }

            return target;
        }

        private Point GetTargetLocation(Point location, Direction direction)
        {

            if (!HasAdjacentCellInDirection(location, direction)) 
                throw new InvalidOperationException("No adjacent cell exists for the location and direction provided.");

            switch (direction)
            {
                case Direction.North:
                    return new Point(location.X, location.Y - 1);
                case Direction.West:
                    return new Point(location.X - 1, location.Y);
                case Direction.South:
                    return new Point(location.X, location.Y + 1);
                case Direction.East:
                    return new Point(location.X + 1, location.Y);
                default:
                    throw new InvalidOperationException();
            }
        }

        public int RandomDirection(ref Point curr, Cell[,] visited, int oldDir)
        {
            Point newCurr;
            HashSet<int> triedDirs = new HashSet<int>();
            int dir;
            do
            {
                newCurr = new Point(curr.X, curr.Y);
                if (oldDir != -1 && TCODRandom.getInstance().getInt(0, 100) > 70)
                    dir = oldDir;
                else
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
