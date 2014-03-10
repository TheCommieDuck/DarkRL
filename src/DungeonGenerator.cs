using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libtcod;

namespace DarkRL
{
    //A direction
    public enum Direction
    {
        North,
        South,
        West,
        East
    };

    //a cell for generating with
    class Cell
    {
        public bool IsDoor = false;
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

        public bool IsNotWall
        {
            get
            {
                if (!LeftWall)
                    return true;
                if (!RightWall)
                    return true;
                if (!TopWall)
                    return true;
                if (!BottomWall)
                    return true;
                return false;
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
                return IsNotWall && IsVisited;
            }
        }

        public bool IsRoom
        {
            get
            {
                return IsNotWall && !IsVisited;
            }
        }
    }

    //a way to choose directions
    class DirectionPicker
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
            if (!HasNextDirection)
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

    class DungeonGenerator
    {
        //we store the height and width of the level, then we have a 'generating' height and width which does not include a 1 block thick outside wall
        private int levelWidth, levelHeight;

        private Cell[,] visited;

        private List<Point> visitedIndices;

        private Level level;

        public int Width
        {
            get
            {
                return levelWidth - 2;
            }
        }

        public int Height
        {
            get
            {
                return levelHeight - 2;
            }
        }

        public DungeonGenerator(Level level)
        {
            this.level = level;
            this.levelHeight = level.Height;
            this.levelWidth = level.Width;
            this.visitedIndices = new List<Point>();
        }

        public void Generate()
        {
            visited = new Cell[Width, Height];

            TCODRandom random = TCODRandom.getInstance();

            //initialise the array
            for (ushort x = 0; x < Width; ++x)
            {
                for (ushort y = 0; y < Height; ++y)
                    visited[x, y] = new Cell();
            }

            //pick a random point, visit it
            Point currentPoint = PickRandomPointToVisit();
            Direction previous = Direction.North;

            //visit all the others
            while (visitedIndices.Count != Width * Height)
            {
                //pick a direction
                DirectionPicker directionPicker = new DirectionPicker(previous, 30);
                Direction dir = directionPicker.GetNextDirection();

                //whilst the direction is invalid, generate a new one
                while (!HasAdjacentCellInDirection(currentPoint, dir) || IsAdjacentCellVisited(currentPoint, dir))
                {
                    if (directionPicker.HasNextDirection)

                        dir = directionPicker.GetNextDirection();
                    else
                    {
                        //if we can't generate a new one, then pick a new point
                        currentPoint = DarkRL.SelectRandomFromList(visitedIndices);
                        directionPicker = new DirectionPicker(previous, 30);
                        dir = directionPicker.GetNextDirection();
                    }
                }

                //create a corridor, mark it up
                currentPoint = CreateCorridor(currentPoint, dir);
                MarkVisited(currentPoint);
                previous = dir;
            }

            //remove some of the corridors
            Sparsify(65);
            RemoveDeadEnds(100);

            PlaceRooms(Width * Height / 700, 7, 20, 7, 20);

            //and cleanup
            Cleanup(1, 60, 7);
            Cleanup(1, 50, 0);
        }

        private void Cleanup(int times, int threeSpace, int twoSpace)
        {
            for (int i = 0; i < times; ++i)
            {
                //and a final try and sort out those disgusting dead ends within our finishing function - if it's got 3 wall neighbours, then just make it a wall
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {

                        if (visited[x, y].Count == 4)
                        {
                            if (NumAdjacentSpaces(x, y) == 4 ||
                                (NumAdjacentSpaces(x, y) == 3 && !HasAdjacentDoor(x, y) && (TCODRandom.getInstance().getInt(0, 100) < threeSpace))
                                || (NumAdjacentSpaces(x, y) == 2 && !HasAdjacentDoor(x, y) && (TCODRandom.getInstance().getInt(0, 100) < twoSpace))
                                )
                            {
                                level[x + 1, y + 1].Data = Tile.Floor;
                                visited[x, y].LeftWall = false;
                                visited[x, y].TopWall = false;
                                visited[x, y].RightWall = false;
                                visited[x, y].BottomWall = false;
                            }
                            else
                                level[x + 1, y + 1].Data = Tile.Wall;

                        }

                        else if (Has3AdjacentWalls(x, y) && !HasAdjacentDoor(x, y))
                        {
                            level[x + 1, y + 1].Data = Tile.Wall;
                            visited[x, y].LeftWall = true;
                            visited[x, y].TopWall = true;
                            visited[x, y].RightWall = true;
                            visited[x, y].BottomWall = true;
                        }
                        else
                            level[x + 1, y + 1].Data = Tile.Floor;

                        //special
                        if (visited[x, y].IsRoom)
                            level[x + 1, y + 1].BackgroundColor = TCODColor.darkGrey;
                        if (visited[x, y].IsDoor)
                            level[x + 1, y + 1].Data = Tile.Door;
                    }
                }
            }
        }

        private bool HasAdjacentDoor(int x, int y)
        {
            if ((x == 0) || (x == Width - 1) || (y == 0) || (y == Height - 1))
                return false;
            if (visited[x - 1, y].IsDoor)
                return true;
            if (visited[x + 1, y].IsDoor)
                return true;
            if (visited[x, y - 1].IsDoor)
                return true;
            if (visited[x, y + 1].IsDoor)
                return true;
            return false;
        }

        private bool Has3AdjacentWalls(int x, int y)
        {
            if ((x == 0) || (x == Width-1) || (y == 0) || (y == Height-1)) 
                return false;
            int cnt = 0;
            if (!visited[x - 1, y].IsDoor && visited[x - 1, y].Count == 4)
                cnt++;
            if (!visited[x + 1, y].IsDoor && visited[x + 1, y].Count == 4)
                cnt++;
            if (!visited[x, y - 1].IsDoor && visited[x, y - 1].Count == 4)
                cnt++;
            if (!visited[x, y + 1].IsDoor && visited[x, y + 1].Count == 4)
                cnt++;
            return cnt >= 3;
        }

        private int NumAdjacentSpaces(int x, int y)
        {
            if ((x == 0) || (x == Width - 1) || (y == 0) || (y == Height - 1))
                return 0;
            int cnt = 0;
            if (!visited[x - 1, y].IsDoor && visited[x - 1, y].Count != 4)
                cnt++;
            if (!visited[x + 1, y].IsDoor && visited[x + 1, y].Count != 4)
                cnt++;
            if (!visited[x, y - 1].IsDoor && visited[x, y - 1].Count != 4)
                cnt++;
            if (!visited[x, y + 1].IsDoor && visited[x, y + 1].Count != 4)
                cnt++;
            return cnt;
        }

        //pick a random point, mark it visited
        //used at the start
        private Point PickRandomPointToVisit()
        {
            Point p = new Point(TCODRandom.getInstance().getInt(0, Width - 1), TCODRandom.getInstance().getInt(0, Height - 1));
            MarkVisited(p);
            return p;
        }

        //mark a point visited
        private void MarkVisited(Point p)
        {
            visited[p.X, p.Y].IsVisited = true;
            visitedIndices.Add(p);
        }

        //make sure we're not out of the map
        private bool HasAdjacentCellInDirection(Point location, Direction direction)
        {
            // Check that the location falls within the bounds of the map
            if ((location.X < 0) || (location.X >= Width) || (location.Y < 0) || (location.Y >= Height)) return false;

            // Check if there is an adjacent cell in the direction
            switch (direction)
            {
                case Direction.North:
                    return location.Y > 0;
                case Direction.South:
                    return location.Y < (Height - 1);
                case Direction.West:
                    return location.X > 0;
                case Direction.East:
                    return location.X < (Width - 1);
                default:
                    return false;
            }
        }

        //see if the cell next to us in a direction is visited
        private bool IsAdjacentCellVisited(Point location, Direction direction)
        {
            if (!HasAdjacentCellInDirection(location, direction))
                throw new InvalidOperationException("No adjacent cell exists for the location and direction provided.");
            switch (direction)
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

        //create a corridor between this point and the one next to it
        private Point CreateCorridor(Point location, Direction direction)
        {
            return CreateSide(location, direction);
        }

        //create a specific type of side between 2 points
        private Point CreateSide(Point location, Direction direction,  bool isBecomingWall = false)
        {

            if (!HasAdjacentCellInDirection(location, direction))
                throw new InvalidOperationException("No adjacent cell exists for the location and direction provided.");

            Point target = GetTargetLocation(location, direction);

            switch (direction)
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

        //remove a % of the corridors
        private void Sparsify(int sparseness)
        {
            int cellsToRemove = (int)Math.Ceiling(((float)sparseness / 100) * Width * Height);
            IEnumerator<Point> enumerator = DeadEndLocations().GetEnumerator();

            for (int i = 0; i < cellsToRemove; ++i)
            {
                if (!enumerator.MoveNext()) //check for more
                {
                    enumerator = DeadEndLocations().GetEnumerator();
                    if (!enumerator.MoveNext())
                        break; //nomore

                }
                Point p = enumerator.Current;
                CreateSide(p, visited[p.X, p.Y].DeadEndDirection, true);
            }
        }

        //attempt to remove some dead ends
        private void RemoveDeadEnds(int removalModifier)
        {
            foreach (Point p in DeadEndLocations())
            {
                if (ShouldRemoveDeadEnd(removalModifier))
                {
                    Point current = p;
                    do
                    {
                        DirectionPicker dirPicker = new DirectionPicker(visited[current.X, current.Y].DeadEndDirection, 100);
                        Direction direction = dirPicker.GetNextDirection();
                        while (!HasAdjacentCellInDirection(current, direction))
                        {
                            if (dirPicker.HasNextDirection)
                            {
                                direction = dirPicker.GetNextDirection();
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        current = CreateCorridor(current, direction);
                    } while (visited[current.X, current.Y].IsDeadEnd);
                }
            }
        }

        //place a bunch of rooms
        public void PlaceRooms(int count, int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            for (int i = 0; i < count; ++i)
            {
                int bestScore = Int32.MaxValue;
                int oldScore = Int32.MaxValue;
                Point bestPoint = new Point(-1, -1);
                Point room = CreateRoom(minWidth, maxWidth, minHeight, maxHeight);
                List<Point> attempts = new List<Point>();
                //try some points
                for (int tries = 0; tries < Math.Min(Width * Height, 500); ++tries)
                    attempts.Add(new Point(TCODRandom.getInstance().getInt(0, Width - room.X), TCODRandom.getInstance().getInt(0, Height - room.Y)));
                foreach (Point tryPoint in attempts)
                {
                    bestScore = Math.Min(CalculateRoomScore(tryPoint, room), bestScore);
                    if (bestScore != oldScore)
                    {
                        oldScore = bestScore;
                        bestPoint = tryPoint;
                    }
                }
                PlaceRoom(bestPoint, room);
                AddDoorsToRoom(bestPoint, room);
            }
        }

        //find a new point
        private Point GetTargetLocation(Point location, Direction direction)
        {
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

        //find all the dead ends
        private IEnumerable<Point> DeadEndLocations()
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

        //just an easy way to generate a random chance
        private bool ShouldRemoveDeadEnd(int removeFactor)
        {
            return removeFactor > TCODRandom.getInstance().getInt(0, 99);
        }

        //create a room of a size
        private Point CreateRoom(int minRoomWidth, int maxRoomWidth, int minRoomHeight, int maxRoomHeight)
        {
            return new Point(TCODRandom.getInstance().getInt(minRoomWidth, maxRoomWidth), TCODRandom.getInstance().getInt(minRoomHeight, maxRoomHeight));
        }

        //calculate the score for a room
        private int CalculateRoomScore(Point location, Point room)
        {
            int score = 0;
            for (int x = 0; x < room.X; ++x)
            {
                for (int y = 0; y < room.Y; ++y)
                {
                    Point dungeonLocation = new Point(location.X + x, location.Y + y);
                    if (dungeonLocation.X >= Width - 1 || dungeonLocation.Y >= Height - 1 || dungeonLocation.X == 0 || dungeonLocation.Y == 0)
                        return Int32.MaxValue;
                    // Add 1 point for each adjacent corridor to the cell
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.North))
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.South))
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.West))
                        score++;
                    if (AdjacentCellInDirectionIsCorridor(dungeonLocation, Direction.East))
                        score++;
                    // Add 3 points if the cell overlaps an existing corridor
                    if (visited[dungeonLocation.X, dungeonLocation.Y].IsCorridor)
                        score += 3;
                    if (visited[dungeonLocation.X, dungeonLocation.Y].IsRoom)
                        score += 100;
                }
            }
            return score;
        }

        private bool AdjacentCellInDirectionIsCorridor(Point point, Direction direction)
        {
            Point adj = GetTargetLocation(point, direction);
            return visited[adj.X, adj.Y].IsCorridor;
        }

        private void PlaceRoom(Point location, Point room)
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

        private void AddDoorsToRoom(Point location, Point room)
        {
            List<Point> potentialDoors = new List<Point>();
            List<KeyValuePair<Point, Point>> potentialDoubleDoors = new List<KeyValuePair<Point, Point>>();
            bool isConnectingCorridor = false;
            bool isDoubleDoor = false;
            //left side
            for (int y = 1; y < room.Y - 1; ++y)
            {
                //if there is a corridor next to us 
                if (AdjacentCellInDirectionIsCorridor(new Point(location.X, location.Y + y), Direction.West))
                {
                    if (!isConnectingCorridor || (TCODRandom.getInstance().getInt(0, 3) < 1 && !isDoubleDoor))
                    {
                        potentialDoors.Add(new Point(location.X, location.Y + y));
                        if (isConnectingCorridor)
                            isDoubleDoor = true;
                        isConnectingCorridor = true;
                    }
                }
                else
                {
                    isConnectingCorridor = false;
                    isDoubleDoor = false;
                }
            }
            //right side
            for (int y = 1; y < room.Y - 1; ++y)
            {
                //if there is a corridor next to us 
                if (AdjacentCellInDirectionIsCorridor(new Point(location.X + room.X - 1, location.Y + y), Direction.East))
                {
                    if (!isConnectingCorridor || (TCODRandom.getInstance().getInt(0, 3) < 1 && !isDoubleDoor))
                    {
                        potentialDoors.Add(new Point(location.X + room.X - 1, location.Y + y));
                        if (isConnectingCorridor)
                            isDoubleDoor = true;
                        isConnectingCorridor = true;
                    }
                }
                else
                {
                    isConnectingCorridor = false;
                    isDoubleDoor = false;
                }
            }
            //top side
            for (int x = 1; x < room.X - 1; ++x)
            {
                //if there is a corridor next to us 
                if (AdjacentCellInDirectionIsCorridor(new Point(location.X + x, location.Y), Direction.North))
                {
                    if (!isConnectingCorridor || (TCODRandom.getInstance().getInt(0, 3) < 1 && !isDoubleDoor))
                    {
                        potentialDoors.Add(new Point(location.X + x, location.Y));
                        if (isConnectingCorridor)
                            isDoubleDoor = true;
                        isConnectingCorridor = true;
                    }
                }
                else
                {
                    isConnectingCorridor = false;
                    isDoubleDoor = false;
                }
            }
            //bottom side
            for (int x = 1; x < room.X - 1; ++x)
            {
                //if there is a corridor next to us 
                if (AdjacentCellInDirectionIsCorridor(new Point(location.X + x, location.Y + room.Y - 1), Direction.South))
                {
                    if (!isConnectingCorridor || (TCODRandom.getInstance().getInt(0, 3) < 1 && !isDoubleDoor))
                    {
                        potentialDoors.Add(new Point(location.X + x, location.Y + room.Y - 1));
                        if (isConnectingCorridor)
                            isDoubleDoor = true;
                        isConnectingCorridor = true;
                    }
                }
                else
                {
                    isConnectingCorridor = false;
                    isDoubleDoor = false;
                }
            }

            int actualDoors = 0;
            while (actualDoors == 0 && potentialDoors.Count != 0)
            {
                foreach (Point p in potentialDoors)
                {
                    if (TCODRandom.getInstance().getInt(0, 100) < 70)
                    {
                        visited[p.X, p.Y].IsDoor = true;
                        actualDoors++;
                    }
                }
            }
        }


    }
}
