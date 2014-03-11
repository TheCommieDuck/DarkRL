using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    struct LightingCell
    {
        public bool IsExplored;

        public byte LightLevel;

        public bool IsObscuring;
    }

    class LightSource : Entity
    {
        public byte Intensity { get; set; }

        public byte LightRadius { get; set; }

        public LightSource(Level level)
            : base(level) { }
    }

    class LightingMap
    {

        public static float LightScaleDropoff = 16f;
        private static int[,] multipliers = 
        {
            {1, 0, 0, -1, -1, 0, 0, 1},
            {0, 1, -1, 0, 0, -1, 1, 0},
            {0, 1, 1, 0, 0, -1, -1, 0},
            {1, 0, 0, 1, -1, 0, 0, -1}
        };

        private LightingCell[,] cells;

        private List<LightSource> lightSources;

        private static double IntensityScaleFactor = 5f;

        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        public LightingCell this[int x, int y]
        {
            get
            {
                return cells[x, y];
            }
        }
        public LightingMap(ushort width, ushort height)
        {
            // TODO: Complete member initialization
            this.Width = width;
            this.Height = height;
            cells = new LightingCell[Width, Height];
            lightSources = new List<LightSource>();
        }

        public void SetCellProperty(ushort x, ushort y, bool isExplored, bool isObscuring)
        {
            cells[x, y].IsObscuring = isObscuring;
            cells[x, y].IsExplored = isExplored;
        }

        public void SetCellPropertyExplored(ushort x, ushort y, bool isExplored)
        {
            cells[x, y].IsExplored = isExplored;
        }

        public void SetCellPropertyObscuring(ushort x, ushort y, bool isObscuring)
        {
            cells[x, y].IsObscuring = isObscuring;
        }

        public int GetLightLevel(int x, int y)
        {
            if (x < 0 || y < 0 || y >= Height || x >= Width)
                return 0;
            return cells[x, y].LightLevel;
        }

        public bool GetExplored(int x, int y)
        {
            if (x < 0 || y < 0 || y >= Height || x >= Width)
                return false;
            return cells[x, y].IsExplored;
        }

        public void AddLightSource(LightSource l)
        {
            l.Intensity = 80;
            lightSources.Add(l);
        }

        public void CalculateFOV()
        {
            //first we have to reset the lighting
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (cells[x, y].LightLevel > 0)
                        cells[x, y].IsExplored = true;
                    cells[x, y].LightLevel = 0;
                }
            }

            foreach(LightSource light in lightSources)
            {
                for (uint i = 0; i < 8; i++) 
                {
                    LightCell(light.Position.X, light.Position.Y, light.Intensity);
                    CastLight(light, light.Position.X, light.Position.Y, light.LightRadius, 1, 1.0f, 0.0f, multipliers[0, i],
                    multipliers[1, i], multipliers[2, i], multipliers[3, i]);
                }
            }
        }

        private void CastLight(LightSource light, int x, int y, int radius, int row, float startSlope, float endSlope, int xx, int xy, int yx, int yy)
        {
            if (startSlope < endSlope)
                return;

            float nextStartSlope = startSlope;
            for (int i = row; i < radius; ++i)
            {
                bool blocked = false;
                for (int dx = -i, dy = -i; dx <= 0; ++dx)
                {
                    float lSlope = (dx - 0.5f) / (dy + 0.5f);
                    float rSlope = (dx + 0.5f) / (dy - 0.5f);

                    if (startSlope < rSlope)
                        continue;
                    else if (endSlope > lSlope)
                        break;

                    int sax = dx * xx + dy * xy;
                    int say = dx * yx + dy * yy;

                    if ((sax < 0 && Math.Abs(sax) > x) || (say < 0 && Math.Abs(say) > y))
                        continue;

                    int ax = x + sax;
                    int ay = y + say;
                    if(ax >= Width || ay >= Height)
                        continue;

                    int rad2 = radius * radius;
                    //we lit the cell
                    if ((dx * dx + dy * dy) < rad2 && (Math.Abs(ax-x) + Math.Abs(ay-y) <= radius/1.1))
                    {
                        int squaredDist = (x - ax) * (x - ax) + (y - ay) * (y - ay);
                        double intensityCoef1 = 1.0 / (1.0 + squaredDist/LightingMap.LightScaleDropoff);
                        double intensityCoef2 = intensityCoef1 - 1.0 / (1.0 + radius * radius);
                        double intensityCoef3 = intensityCoef2 / (1.0 - 1.0 / (1.0 + radius * radius));
                        LightCell(ax, ay, (byte)Math.Max((intensityCoef3 * light.Intensity/LightingMap.IntensityScaleFactor), GetLightLevel(ax, ay)));
                    }

                    if (blocked)
                    {
                        if (cells[ax, ay].IsObscuring)
                        {
                            nextStartSlope = rSlope;
                            continue;
                        }
                        else
                        {
                            blocked = false;
                            startSlope = nextStartSlope;
                        }
                    }
                    else if (cells[ax, ay].IsObscuring)
                    {
                        blocked = true;
                        nextStartSlope = rSlope;
                        CastLight(light, x, y, radius, i + 1, startSlope, lSlope, xx, xy, yx, yy);
                    }
                }
                if (blocked)
                    break;
            }
        }

        private void LightCell(int x, int y, byte p)
        {
            cells[x, y].LightLevel = p;
            cells[x, y].IsExplored = true;
        }
    }
}
