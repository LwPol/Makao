using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Makao
{
    public abstract class Drawable
    {
        private Rectangle displayRect;

        public abstract void Render(Graphics g);

        public Rectangle DisplayRect
        {
            get
            {
                return displayRect;
            }
            set
            {
                displayRect = value;
            }
        }

        public Size Size
        {
            get
            {
                return displayRect.Size;
            }
            set
            {
                displayRect.Size = value;
            }
        }

        public Point Location
        {
            get
            {
                return displayRect.Location;
            }
            set
            {
                displayRect.Location = value;
            }
        }
    }
}
