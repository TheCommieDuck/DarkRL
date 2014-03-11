using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Camera
    {
        private Entity focus;

        public Rectangle Viewport { get; private set; }

        public int Left
        {
            get
            {
                return Viewport.Left;
            }
        }

        public int Top
        {
            get
            {
                return Viewport.Top;
            }
        }

        public int Right
        {
            get
            {
                return Viewport.Right;
            }
        }

        public int Bottom
        {
            get
            {
                return Viewport.Bottom;
            }
        }

        public Camera(int topX, int topY, int width, int height)
        {
            Viewport = new Rectangle(topX, topY, width, height);
            focus = null;
        }

        public Camera(Point point, int width, int height)
            : this(point.X, point.Y, width, height) { }

        public void Update()
        {
            if(focus != null)
                this.CentreOn(focus.Position);
        }

        public void SetFocus(Entity focusPoint)
        {
            focus = focusPoint;
        }

        public void CentreOn(Point point)
        {
            this.SetPosition(point.X - Viewport.Width / 2, point.Y - Viewport.Height / 2);
        }

        public void Move(int xOffset, int yOffset)
        {
            SetPosition(Viewport.Left + xOffset, Viewport.Top + yOffset);
        }

        public void SetPosition(int x, int y)
        {
            Viewport = new Rectangle(x, y, Viewport.Width, Viewport.Height);
        }
    }
}
