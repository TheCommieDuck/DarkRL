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
            statusWindow.setBackgroundColor(TCODColor.darkestRed.Multiply(0.1f));
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
            int currentY = 1;

            currentY += WriteToStatusWindow("Player Name:", currentY, TCODColor.white);
            currentY += WriteToStatusWindow(Player.MainPlayer.Name, currentY++, TCODColor.white);
            currentY += WriteToStatusWindow("Life:", ++currentY, TCODColor.white);
            currentY += WriteToStatusWindow(Player.MainPlayer.HP.ToString(), ++currentY, TCODColor.white) + 1;
            currentY += WriteToStatusWindow("Sanity:", ++currentY, TCODColor.white);
            currentY += WriteToStatusWindow("gone mad lol", ++currentY, TCODColor.white);

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

        public int WriteString(TCODConsole console, int x, int y, string msg, TCODColor col)
        {
            console.setForegroundColor(col);
            return console.printRect(x, y, console.getWidth(), 0, msg);
        }

        public int WriteToStatusWindow(string msg, int y, TCODColor col)
        {
            return WriteString(statusWindow, statusWindow.getWidth() / 2, y, msg, col);
        }
    }
}
