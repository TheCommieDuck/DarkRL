using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Player : Entity
    {

        public Player(Level l)
            :base(l, true)
        {
            this.Character = '@';
            this.ViewPriority = Int32.MaxValue;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.l.SetLightingForRecompute();
        }
    }
}
