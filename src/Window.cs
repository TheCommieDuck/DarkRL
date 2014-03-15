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

        public static int StatusPanelWidth = 15;

        public static int MessagePanelHeight = 5;

        ~Window()
        {
            console.Dispose();
        }

        //Initialisation logic
        public void Init()
        {
            TCODConsole.setCustomFont("oryx_tiles.png", ((int)TCODFontFlags.Greyscale | (int)TCODFontFlags.LayoutTCOD), 32, 12);
            TCODConsole.initRoot(80, 50, "DarkRL", false, TCODRendererType.GLSL);
            console = TCODConsole.root;
            TCODConsole.checkForKeypress();
            console.setBackgroundColor(TCODColor.black);
        }

        public void Draw(TCODColor backgroundColor, TCODColor foregroundColor, char character, int x, int y)
        {
                console.putCharEx(x, y, character, foregroundColor, backgroundColor);
        }

        public void Draw(float lightMod, TCODColor foregroundColor, char character, int x, int y)
        {
            console.putCharEx(x, y, character, foregroundColor.Multiply(lightMod), TCODColor.black);
        }

        public void Update()
        {
            TCODConsole.flush();
        }

        public void Clear()
        {
            console.clear();
        }


        public void Draw(TCODColor backgroundColor, int x, int y)
        {
            console.setCharBackground(x, y, backgroundColor);
        }

        

        public void WriteString(int x, int y, string msg, TCODColor foreCol, TCODColor backCol)
        {
            console.setForegroundColor(foreCol);
            console.print(x, y, msg);
        }
    }
}
