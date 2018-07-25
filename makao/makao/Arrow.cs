using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Makao
{
    public class Arrow : Drawable, IClickable
    {
        private ArrowOrientation orientation = ArrowOrientation.Right;
        private bool visible = true;

        public event EventHandler Click;

        public Arrow()
        {
        }

        public Arrow(ArrowOrientation orientation)
        {
            Orientation = orientation;
        }

        public override void Render(Graphics g)
        {
            if (visible)
            {
                Point[] triangle;
                if (orientation == ArrowOrientation.Right)
                {
                    triangle = new Point[]
                    {
                        new Point(DisplayRect.Left, DisplayRect.Top),
                        new Point(DisplayRect.Left, DisplayRect.Bottom),
                        new Point(DisplayRect.Right, DisplayRect.Top + DisplayRect.Height / 2)
                    };
                }
                else
                {
                    triangle = new Point[]
                    {
                        new Point(DisplayRect.Right, DisplayRect.Top),
                        new Point(DisplayRect.Right, DisplayRect.Bottom),
                        new Point(DisplayRect.Left, DisplayRect.Top + DisplayRect.Height / 2)
                    };
                }

                g.FillPolygon(Brushes.White, triangle);
            }
        }

        void IClickable.Clicked()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        bool IClickable.Contains(Point pt)
        {
            if (DisplayRect.Contains(pt))
            {
                float direction;
                if (orientation == ArrowOrientation.Right)
                {
                    Point origin = new Point(DisplayRect.Right, DisplayRect.Top + DisplayRect.Height / 2);
                    Point bottomCorner = new Point(DisplayRect.Left - origin.X, DisplayRect.Bottom - origin.Y);
                    pt.X -= origin.X;
                    pt.Y -= origin.Y;

                    direction = (float)bottomCorner.Y / bottomCorner.X;
                }
                else
                {
                    Point origin = new Point(DisplayRect.Left, DisplayRect.Top + DisplayRect.Height / 2);
                    Point bottomCorner = new Point(DisplayRect.Right - origin.X, DisplayRect.Bottom - origin.Y);
                    pt.X -= origin.X;
                    pt.Y -= origin.Y;

                    direction = (float)bottomCorner.Y / bottomCorner.X;
                }

                return pt.Y <= (int)(pt.X * direction) && pt.Y >= -(int)(pt.X * direction);
            }
            return false;
        }

        public ArrowOrientation Orientation
        {
            get
            {
                return orientation;
            }
            
            set
            {
                if (value < ArrowOrientation.Right || value > ArrowOrientation.Left)
                    throw new InvalidEnumArgumentException("Orientation", (int)value, typeof(ArrowOrientation));

                orientation = value;
            }
        }

        public bool Visible
        {
            get
            {
                return visible;
            }

            set
            {
                visible = value;
            }
        }
    }

    public enum ArrowOrientation
    {
        Right,
        Left
    }
}
