using System;
using System.Drawing;

namespace Makao
{
    public interface IClickable
    {
        bool Contains(Point pt);

        void Clicked();

        bool Visible
        {
            get;
            set;
        }

        event EventHandler Click;
    }
}
