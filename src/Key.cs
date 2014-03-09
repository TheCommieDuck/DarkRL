using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class Key
    {
        public char Character { get; private set; }

        public TCODKeyCode KeyCode { get; private set; }

        public Key(TCODKey key)
        {
            //just set it to some unused value if it's a special key
            Character = key.KeyCode == TCODKeyCode.Char ? key.Character : '\0';
            KeyCode = key.KeyCode;
        }

        public Key(char c)
        {
            Character = c;
            KeyCode = TCODKeyCode.Char;
        }

        public Key(TCODKeyCode code)
        {
            Character = '\0';
            KeyCode = code;
        }

        public override bool Equals(object obj)
        {
            Key other = obj as Key;
            if (other == null)
                return false;
            //we really don't care about alt and stuff
            if (other.Character != this.Character || other.KeyCode != this.KeyCode)
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return this.Character.GetHashCode() ^ this.KeyCode.GetHashCode();
        }
    }
}
