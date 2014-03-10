using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class Window
    {
        private TCODConsole console;

        public int Width
        {
            get
            {
                return console.getWidth();
            }
        }

        public int Height
        {
            get
            {
                return console.getHeight();
            }
        }

        public bool IsClosed
        {
            get
            {
                return TCODConsole.isWindowClosed();
            }
        }

        ~Window()
        {
            console.Dispose();
        }

        //Initialisation logic
        public void Init()
        {
            libtcod.TCODConsole.initRoot(40, 20, "DarkRL", false, TCODRendererType.GLSL);
            console = TCODConsole.root;
            TCODConsole.checkForKeypress();
        }

        public void Draw(TCODColor backgroundColor, TCODColor foregroundColor, char character, int x, int y)
        {
            console.putCharEx(x, y, character, foregroundColor, backgroundColor);
        }

        public void Update()
        {
            TCODConsole.flush();
        }

        public void Clear()
        {
            console.clear();
        }


        internal void Draw(TCODColor backgroundColor, int x, int y)
        {
            console.setCharBackground(x, y, backgroundColor);
        }
    }
}
