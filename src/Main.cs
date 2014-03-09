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
        private Window window;
        private World world;
        private Camera camera;

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
                window.Update();
                Draw();
                InputSystem.WaitForInput();
                
            }
        }

        public void Draw()
        {
            window.Clear();
            for (int x = camera.Left; x < camera.Right; ++x)
            {
                for (int y = camera.Top; y < camera.Bottom; ++y)
                {
                    int windowX = x - camera.Left;
                    int windowY = y - camera.Top;
                    if(world[x,y].VisibleEntity == null)
                        window.Draw(world[x, y].BackgroundColor, windowX, windowY);
                    else
                        window.Draw(world[x, y].BackgroundColor, world[x, y].VisibleEntity.Color, world[x, y].VisibleEntity.Character, windowX, windowY);
                }
            }
            window.Update();
        }

        public void Init()
        {
            window = new Window();
            window.Init();
            world = new World();
            camera = new Camera(40, 40, window.Width, window.Height);
            InputSystem.RegisterInputEvent(new Key('h'), () => camera.Move(-1, 0));
            InputSystem.RegisterInputEvent(new Key('a'), () => camera.Move(-1, 0));

            InputSystem.RegisterInputEvent(new Key('j'), () => camera.Move(0, 1));
            InputSystem.RegisterInputEvent(new Key('s'), () => camera.Move(0, 1));

            InputSystem.RegisterInputEvent(new Key('w'), () => camera.Move(0, -1));
            InputSystem.RegisterInputEvent(new Key('k'), () => camera.Move(0, -1));

            InputSystem.RegisterInputEvent(new Key('l'), () => camera.Move(1, 0));
            InputSystem.RegisterInputEvent(new Key('d'), () => camera.Move(1, 0));
        }
    }
}
