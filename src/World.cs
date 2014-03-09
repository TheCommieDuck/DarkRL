using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class World
    {
        public List<Level> Levels { get; private set; }

        public Level CurrentLevel
        {
            get
            {
                return Levels[CurrentLevelIndex];
            }
        }

        public int CurrentLevelIndex { get; private set; }

        public Tile this[int x, int y]
        {
            get
            {
                return CurrentLevel[x, y];
            }
        }

        public World()
        {
            Levels = new List<Level>();
            Levels.Add(new Level(200, 200));
            CurrentLevelIndex = 0;
            CurrentLevel.GenerateLevel();
        }
    }
}
