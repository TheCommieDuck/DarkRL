using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;
using DarkRL.Entities;

namespace DarkRL
{
    class DarkRL
    {
        public static TCODRandom Random = TCODRandom.getInstance();
        private Window window;
        private World world;
        private GUI gui;
        private Camera camera;
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
            gui.Draw(messages);
            
            window.Update();
        }

        private void DrawGUI()
        {
            
        }

        public void Init()
        {
            main = this;
            window = new Window();
            window.Init();
            world = new World();
            messages = new List<string>();

            camera = new Camera(0, 0, window.Width, window.Height - Window.MessagePanelHeight);
            camera.CentreOn(world.CurrentLevel.Player.Position);
            gui = new GUI(window, TCODConsole.root, camera);
            Player player = (Player)world.CurrentLevel.Player;
            camera.Focus = player;
            InputSystem.RegisterInputEvent(LeftMovement, () => player.Move(-1, 0));
            InputSystem.RegisterInputEvent(DownMovement, () => player.Move(0, 1));
            InputSystem.RegisterInputEvent(UpMovement, () => player.Move(0, -1));
            InputSystem.RegisterInputEvent(RightMovement, () => player.Move(1, 0));
            InputSystem.RegisterInputEvent(new Key('o'), () => player.Open());
            InputSystem.RegisterInputEvent(new Key('g'), () => player.PickupItem());
            InputSystem.RegisterInputEvent(new Key('r'), () => player.DropItem());
            InputSystem.RegisterInputEvent(new Key('i'), () => player.Inspect());
            InputSystem.RegisterInputEvent(new Key('e'), () => player.Equip());
            InputSystem.RegisterInputEvent(new Key('u'), () => player.Unequip());
            window.Update();
            Draw();
        }

        public static void WriteMessage(string p)
        {
            if (messages == null)
                return;
            messages.Add(p);
            main.Draw();
        }

        public static void AddOverlayConsole(TCODConsole src, int p1, int p2, int p3, int p4, TCODConsole dst, int p5, int p6)
        {
            main.Draw();
            TCODConsole.blit(src, p1, p2, p3, p4, dst, p5, p6);
            TCODConsole.flush();
        }
    }
}
