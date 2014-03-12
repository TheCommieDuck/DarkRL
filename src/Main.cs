using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;


namespace DarkRL
{
    class DarkRL
    {
        public static TCODRandom Random = TCODRandom.getInstance();
        private Window window;
        private World world;
        private Camera camera;
        private int guiHeight = 4;
        private static DarkRL main;

        public static Key[] LeftMovement = new Key[]{new Key('h'), new Key('a')};
        public static Key[] RightMovement = new Key[]{new Key('l'), new Key('d')};
        public static Key[] UpMovement = new Key[]{new Key('w'), new Key('k')};
        public static Key[] DownMovement = new Key[] { new Key('j'), new Key('s') };

        public static Dictionary<Direction, Point> Directions = new Dictionary<Direction, Point>()
        {
            {Direction.North, new Point(0, -1) },
            {Direction.South, new Point(0, 1) },
            {Direction.West, new Point(-1, 0) },
            {Direction.East, new Point(1, 0) }
        };

        private static List<string> messages;

        public static T SelectRandomFromList<T>(List<T> items)
        {
            return items[TCODRandom.getInstance().getInt(0, items.Count - 1)];
        }

        static void Main(string[] args)
        {
            DarkRL darkRL = new DarkRL();
            darkRL.Init();
            darkRL.Run();

            //make it close in a decent amount of time
            System.Environment.Exit(0);
        }

        public void Run()
        {
            while (!window.IsClosed)
            {
                InputSystem.WaitForInput();
                world.CurrentLevel.Update();
                camera.Update();
                Draw();
            }
        }

        private void DrawLoop()
        {
        }

        public void Draw()
        {
            world.CurrentLevel.LightingUpdate();
            window.Clear();
            world.CurrentLevel.Draw(window, camera);
            int msgCount = 0;
            for (int y = camera.Bottom; y < guiHeight + camera.Bottom; ++y)
            {
                int windowY = y - camera.Top;
                int msgIndex = (messages.Count + camera.Bottom) - (y + 1);
                if (msgIndex >= 0)
                {
                    float darken = 1f - ((float)msgCount / (float)guiHeight);
                    window.WriteString(0, windowY, messages[msgIndex], TCODColor.white.Multiply(darken));
                    msgCount++;
                }
            }
            window.Update();
        }

        public void Init()
        {
            main = this;
            window = new Window();
            window.Init();
            world = new World();
            messages = new List<string>();

            camera = new Camera(0, 0, window.Width, window.Height-guiHeight);
            camera.CentreOn(world.CurrentLevel.Player.Position);
            Player player = (Player)world.CurrentLevel.Player;
            camera.SetFocus(player);
            InputSystem.RegisterInputEvent(LeftMovement, () => player.Move(-1, 0));
            InputSystem.RegisterInputEvent(DownMovement, () => player.Move(0, 1));
            InputSystem.RegisterInputEvent(UpMovement, () => player.Move(0, -1));
            InputSystem.RegisterInputEvent(RightMovement, () => player.Move(1, 0));
            InputSystem.RegisterInputEvent(new Key('o'), () => player.Open());

            window.Update();
            Draw();
        }

        public static void WriteMessage(string p)
        {
            messages.Add(p);
            main.Draw();
        }
    }
}
