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
                window.Update();
            }
        }

        public void Draw()
        {
            window.Clear();
            world.CurrentLevel.Draw(window, camera);
            for (int y = camera.Bottom; y < guiHeight + camera.Bottom; ++y)
            {

            }
            window.Update();
        }

        public void Init()
        {
            window = new Window();
            window.Init();
            world = new World();

            camera = new Camera(0, 0, window.Width, window.Height-guiHeight);
            camera.CentreOn(world.CurrentLevel.Player.Position);
            Entity player = world.CurrentLevel.Player;
            camera.SetFocus(player);
            InputSystem.RegisterInputEvent(new Key('h'), () => player.Move(-1, 0));
            InputSystem.RegisterInputEvent(new Key('a'), () => player.Move(-1, 0));

            InputSystem.RegisterInputEvent(new Key('j'), () => player.Move(0, 1));
            InputSystem.RegisterInputEvent(new Key('s'), () => player.Move(0, 1));

            InputSystem.RegisterInputEvent(new Key('w'), () => player.Move(0, -1));
            InputSystem.RegisterInputEvent(new Key('k'), () => player.Move(0, -1));

            InputSystem.RegisterInputEvent(new Key('l'), () => player.Move(1, 0));
            InputSystem.RegisterInputEvent(new Key('d'), () => player.Move(1, 0));

            window.Update();
            Draw();
        }
    }
}
