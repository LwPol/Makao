using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Makao
{
    public class NamesTable : Drawable
    {
        private string[] names;
        private int[] cardsCount;
        private int[] turnsWaiting;

        private int currentNameIndex = 0;

        private Font font = SystemFonts.DefaultFont;

        public NamesTable(string[] names)
        {
            this.names = names;

            cardsCount = new int[names.Length];
            for (int i = 0; i < cardsCount.Length; ++i)
                cardsCount[i] = 5;

            turnsWaiting = new int[names.Length];
        }

        public override void Render(Graphics g)
        {
            Pen borderLinesPen = new Pen(Color.FromArgb(240, 240, 240));

            Size cellSize = CellSize;
            Point cellLocation = Location;

            var pageUnit = g.PageUnit;
            g.PageUnit = GraphicsUnit.Pixel;

            for (int i = 0; i < names.Length; ++i)
            {
                Rectangle currentRect = new Rectangle(cellLocation, cellSize);

                g.SetClip(currentRect);

                g.FillRectangle(Brushes.Black, currentRect);
                g.DrawLine(borderLinesPen,
                    new Point(currentRect.Left, currentRect.Bottom - 1),
                    new Point(currentRect.Right, currentRect.Bottom - 1));

                SizeF textSize = g.MeasureString(names[i], font);
                g.DrawString(names[i], font, Brushes.White,
                    currentRect.X + (currentRect.Width - textSize.Width) / 2,
                    currentRect.Y + currentRect.Height / 2 - textSize.Height);

                string numOfCards = cardsCount[i].ToString() + " kart";
                textSize = g.MeasureString(numOfCards, font);
                g.DrawString(numOfCards, font, Brushes.White,
                    currentRect.X + (currentRect.Width - textSize.Width) / 2,
                    currentRect.Y + currentRect.Height / 2);

                if (i == currentNameIndex)
                {
                    int xCenter = cellLocation.X + (cellSize.Width - (int)textSize.Width) / 2 - 10;
                    int yCenter = cellLocation.Y + (cellSize.Height + (int)textSize.Height) / 2 - 1;
                    const int radius = 3;

                    using (Brush brush = new SolidBrush(Color.FromArgb(0, 128, 0)))
                        g.FillEllipse(brush, new Rectangle(xCenter - radius, yCenter - radius, 2 * radius, 2 * radius));
                }

                if (turnsWaiting[i] > 0)
                {
                    SizeF waitNumSize = g.MeasureString(turnsWaiting[i].ToString(), font);
                    g.DrawString(turnsWaiting[i].ToString(), font,
                        Brushes.Red, new PointF(currentRect.Right - waitNumSize.Width - 5, currentRect.Bottom - waitNumSize.Height - 5));
                }

                cellLocation.Y += cellSize.Height;
            }

            g.ResetClip();
            borderLinesPen.Dispose();
            g.PageUnit = pageUnit;
        }

        public Size CellSize
        {
            get
            {
                return new Size(Size.Width, Size.Height / names.Length);
            }

            set
            {
                DisplayRect = new Rectangle(DisplayRect.Location, new Size(value.Width, value.Height * names.Length));
            }
        }

        public int[] CardsCount
        {
            get
            {
                return cardsCount;
            }
        }

        public int CurrentCardsCount
        {
            get
            {
                return cardsCount[currentNameIndex];
            }

            set
            {
                cardsCount[currentNameIndex] = value;
            }
        }

        public string CurrentName
        {
            get
            {
                return names[currentNameIndex];
            }

            set
            {
                names[currentNameIndex] = value;
            }
        }

        public int CurrentNameIndex
        {
            get
            {
                return currentNameIndex;
            }
            set
            {
                if (value >= 0 && value < names.Length)
                    currentNameIndex = value;
            }
        }

        public Font Font
        {
            get
            {
                return font;
            }

            set
            {
                font = value;
            }
        }

        public string[] Names
        {
            get
            {
                return names;
            }
        }

        public int[] TurnsWaiting
        {
            get
            {
                return turnsWaiting;
            }
        }
    }
}
