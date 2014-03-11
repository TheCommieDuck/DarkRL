using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Player : Entity
    {
        public LightSource Lantern;
        public Player(Level l)
            :base(l, true)
        {
            this.Character = '@';
            this.ViewPriority = Int32.MaxValue;
            Lantern = new LightSource(l);
            Lantern.LightRadius = 10;
            l.AddLightSource(Lantern);
        }

        public override void SetPosition(int x, int y)
        {
            l.NeedsLightingUpdate();
            base.SetPosition(x, y);
            Lantern.SetPosition(x, y);
        }
    }
}
