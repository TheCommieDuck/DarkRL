using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class GUI
    {
        private TCODConsole statusWindow, eventLog, main;
        private Window window;

        public GUI(Window win, TCODConsole main)
        {
            statusWindow = new TCODConsole(win.StatusPanelWidth, win.Height);
            eventLog = new TCODConsole(win.Width - win.StatusPanelWidth, win.MessagePanelHeight);
            statusWindow.setAlignment(TCODAlignment.CenterAlignment);
            statusWindow.setBackgroundColor(TCODColor.darkestRed.Multiply(0.5f));
            this.main = main;
            this.window = win;
        }

        ~GUI()
        {
            statusWindow.Dispose();
            eventLog.Dispose();
        }

        public void Draw(List<String> messages)
        {
            eventLog.clear();
            statusWindow.clear();
            //draw status window

            WriteToStatusWindow("Player Name:", 1, TCODColor.white);
            WriteToStatusWindow(Player.MainPlayer.Name, 2, TCODColor.white);
            WriteToStatusWindow("Life:", 4, TCODColor.white);
            WriteToStatusWindow("Feeling fine.", 5, TCODColor.white);
            WriteToStatusWindow("Sanity:", 7, TCODColor.white);
            WriteToStatusWindow("gone mad lol", 8, TCODColor.white);

            //draw event log
            int msgCount = 0;
            for (int y = 0; y < eventLog.getHeight(); ++y)
            {
                int msgIndex = (messages.Count) - (y + 1);
                if (msgIndex >= 0)
                {
                    float darken = 1f - (((float)msgCount / (float)eventLog.getHeight()) * 1.1f);
                    WriteString(eventLog, 0, y, messages[msgIndex], TCODColor.white.Multiply(darken));
                    msgCount++;
                }
            }
            TCODConsole.blit(eventLog, 0, 0, eventLog.getWidth(), eventLog.getHeight(), main, window.StatusPanelWidth, 0);
            TCODConsole.blit(statusWindow, 0, 0, statusWindow.getWidth(), statusWindow.getHeight(), main, 0, 0);
        }

        public void WriteString(TCODConsole console, int x, int y, string msg, TCODColor col)
        {
            console.setForegroundColor(col);
            console.print(x, y, msg);
        }

        public void WriteToStatusWindow(string msg, int y, TCODColor col)
        {
            WriteString(statusWindow, statusWindow.getWidth() / 2, y, msg, col);
        }
    }
}
