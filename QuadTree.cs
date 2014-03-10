using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class QuadTree
    {
        QuadTree nw, ne, sw, se;

        public QuadTree()
        {
            nw = ne = sw = se = null;
        }
    }
}
