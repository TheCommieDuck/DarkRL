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
            camera = new Camera(0, 0, window.Width, window.Height);
            InputSystem.RegisterInputEvent(new Key('a'), () => camera.Move(1, 1));
        }
    }
}
