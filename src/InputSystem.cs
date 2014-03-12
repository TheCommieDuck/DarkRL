using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    static class InputSystem
    {
        private static Dictionary<Key, List<Action>> events = new Dictionary<Key, List<Action>>();

        public static void WaitForInput()
        {
            TCODKey rawKey = TCODConsole.waitForKeypress(true);
            Key key = new Key(rawKey);
            if (events.Keys.Contains(key))
            {
                foreach (Action a in events[key])
                    a();
            }
        }

        public static Key WaitForAndReturnInput()
        {
            TCODKey rawKey = TCODConsole.waitForKeypress(true);
            Key key = new Key(rawKey);
            return key;
        }

        public static void RegisterInputEvent(Key key, Action keyEvent)
        {
            List<Action> actions;
            if (!events.TryGetValue(key, out actions))
            {
                actions = new List<Action>();
                events[key] = actions;
            }

            events[key].Add(keyEvent);
        }

        public static void RegisterInputEvent(Key[] keys, Action action)
        {
            foreach (Key k in keys)
                RegisterInputEvent(k, action);
        }
    }
}
