using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

namespace Makao
{
    public class CardDisplayer : Drawable, IClickable
    {
        static Dictionary<Card, Image> cardsImages;
        private Card displayed;
        private bool selected = false;
        private bool visible = true;

        public event EventHandler Click;

        public static void LoadCardsImages()
        {
            cardsImages = new Dictionary<Card, Image>();

            string directory = "cards_graphics\\";
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 13; ++j)
                {
                    CardRank curRank = (CardRank)j;
                    CardSuit curSuit = (CardSuit)i;
                    string fileName = directory + curSuit.ToString().ToLower() + "\\" + curRank.ToString().ToLower() + ".bmp";

                    try
                    {
                        cardsImages.Add(new Card(curRank, curSuit), new Bitmap(fileName));
                    }
                    catch (ArgumentException)
                    {
                        throw new FileNotFoundException("File not found", fileName);
                    }
                }
            }
        }

        public override void Render(Graphics g)
        {
            if (visible)
            {
                g.DrawImage(cardsImages[displayed], DisplayRect);

                if (selected)
                {
                    using (Pen pen = new Pen(Color.FromArgb(0, 0, 240), 3.0f))
                    {
                        g.DrawRectangle(pen, DisplayRect);
                    }
                }
            }
        }

        bool IClickable.Contains(Point pt)
        {
            return DisplayRect.Contains(pt);
        }

        void IClickable.Clicked()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        public static Dictionary<Card, Image> CardsImages
        {
            get
            {
                return cardsImages;
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        public Card Displayed
        {
            get
            {
                return displayed;
            }
            set
            {
                displayed = value;
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
}
