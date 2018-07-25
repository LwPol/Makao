using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Makao
{
    class MButton : Button
    {
        enum State
        {
            Default,
            Hover,
            Pushed
        }
        private State myState = State.Default;

        public MButton()
        {
            BackColor = Color.FromArgb(0, 128, 255);
            Font = AppControl.AppFont;
            Cursor = Cursors.Hand;
            Paint += new PaintEventHandler(MyPaint);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            myState = State.Default;
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            myState = State.Hover;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            myState = State.Default;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            myState = State.Pushed;
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            myState = State.Hover;
            base.OnMouseUp(mevent);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if (mevent.Location.X < 0 || mevent.Location.Y < 0 || mevent.Location.X >= Width || mevent.Location.Y >= Height)
            {
                OnMouseLeave(EventArgs.Empty);
                MainForm.ReleaseCapture();
            }
            base.OnMouseMove(mevent);
        }

        private void MyPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), DisplayRectangle);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Brush textBrush;
            if (!Enabled)
                textBrush = Brushes.Gray;
            else if (myState == State.Default)
                textBrush = Brushes.Black;
            else
                textBrush = Brushes.White;
            e.Graphics.DrawString(Text, Font, textBrush, new RectangleF(0, 0, Width, Height), sf);
            if (myState == State.Pushed)
                e.Graphics.DrawRectangle(Pens.White, new Rectangle(0, 0, Width - 1, Height - 1));
        }
    }
}
