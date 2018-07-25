using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace Makao
{
    public class DemandDialog : Form
    {
        private DemandDialogType type;

        private ListBox lbPossibleValues;
        private Label lblInfo;
        private MButton btnOk;

        private const int dlgWidth = 471, dlgHeight = 299;

        private CardSuit? demandedSuit = null;
        private CardRank? demandedRank = null;

        private Point lastRegisteredMousePos;

        public DemandDialog() :
            this(DemandDialogType.JackDemand)
        {
        }

        public DemandDialog(DemandDialogType type)
        {
            if (type < DemandDialogType.JackDemand || type > DemandDialogType.AceDemand)
                throw new InvalidEnumArgumentException("type", (int)type, typeof(DemandDialogType));

            this.type = type;

            FormBorderStyle = FormBorderStyle.None;

            CreateHandle();

            Size = new Size(dlgWidth, dlgHeight);
            Form owner = AppControl.MainWindow;
            Location = owner.PointToScreen(new Point((owner.Width - dlgWidth) / 2, (owner.Height - dlgHeight) / 2));

            InitializeComponents();
            
            MouseDown += new MouseEventHandler(DemandDialog_MouseDown);
            MouseMove += new MouseEventHandler(DemandDialog_MouseMove);
            Paint += new PaintEventHandler(DemandDialog_Paint);
        }

        private void DemandDialog_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
                lastRegisteredMousePos = PointToScreen(e.Location);
        }

        private void DemandDialog_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                Point currentPoint = PointToScreen(e.Location);
                Point translation = new Point(currentPoint.X - lastRegisteredMousePos.X, currentPoint.Y - lastRegisteredMousePos.Y);
                lastRegisteredMousePos = currentPoint;

                Location = new Point(Location.X + translation.X, Location.Y + translation.Y);
            }
        }

        private void DemandDialog_Paint(object sender, PaintEventArgs e)
        {
            using (Brush brush = new SolidBrush(AppControl.MainWindow.BackColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);

            e.Graphics.DrawRectangle(Pens.Blue,
                new Rectangle(new Point(0, 0), new Size(DisplayRectangle.Width - 1, DisplayRectangle.Height - 1)));
        }

        private void InitializeComponents()
        {
            lbPossibleValues = new ListBox();
            btnOk = new MButton();
            lblInfo = new Label();

            SuspendLayout();

            Controls.Add(lbPossibleValues);
            Controls.Add(btnOk);
            Controls.Add(lblInfo);

            // lbPossibleValues
            
            string[] values;
            if (type == DemandDialogType.JackDemand)
                values = new string[] { "Piątka", "Szóstka", "Siódemka", "Ósemka", "Dziewiątka", "Dziesiątka", "Brak żadania" };
            else
                values = new string[] { "Pik", "Trefl", "Karo", "Kier", "Bez zmian" };

            lbPossibleValues.Name = "lbPossibleValues";
            lbPossibleValues.Items.AddRange(values);
            const int listBoxWidth = 150;
            lbPossibleValues.Size = new Size(listBoxWidth, (lbPossibleValues.Items.Count + 1) * lbPossibleValues.ItemHeight);
            lbPossibleValues.Location = new Point((Width - lbPossibleValues.Width) / 2, (Height - lbPossibleValues.Height) / 2);

            // btnOk
            
            btnOk.Name = "btnOk";
            btnOk.Text = "OK";
            const int buttonWidth = 100, buttonHeight = 27;
            int xButton = (Width - buttonWidth) / 2;
            int yButton = lbPossibleValues.Location.Y + lbPossibleValues.Height + 30;
            btnOk.Location = new Point(xButton, yButton);
            btnOk.Size = new Size(buttonWidth, buttonHeight);
            btnOk.Enabled = false;

            // lblInfo
            
            lblInfo.AutoSize = true;
            lblInfo.Name = "lblInfo";
            lblInfo.Font = AppControl.AppFont;
            lblInfo.Text = (type == DemandDialogType.JackDemand ? "Wybierz żądaną kartę:" : "Wybierz żądany kolor:");
            lblInfo.BackColor = Color.FromArgb(0, Color.Black);
            lblInfo.Size = new Size(0, 0);

            const int yInfo = 65;
            int xInfo = (Width - lblInfo.Width) / 2;
            lblInfo.Location = new Point(xInfo, yInfo);

            ResumeLayout();

            lbPossibleValues.SelectedIndexChanged += new EventHandler(PossibleValue_SelectedIndexChanged);
            btnOk.Click += new EventHandler(OKButton_Click);

            MouseEventHandler forLabelMouseDown = (sender, e) =>
            {
                OnMouseDown(new MouseEventArgs(e.Button,
                    e.Clicks,
                    lblInfo.Location.X + e.Location.X,
                    lblInfo.Location.Y + e.Location.Y,
                    e.Delta));
            };
            MouseEventHandler forLabelMouseMove = (sender, e) =>
            {
                OnMouseMove(new MouseEventArgs(e.Button,
                    e.Clicks,
                    lblInfo.Location.X + e.Location.X,
                    lblInfo.Location.Y + e.Location.Y,
                    e.Delta));
            };
            lblInfo.MouseDown += forLabelMouseDown;
            lblInfo.MouseMove += forLabelMouseMove;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (type == DemandDialogType.JackDemand)
            {
                CardRank[] ranks = new CardRank[]
                {
                    CardRank.Five,
                    CardRank.Six,
                    CardRank.Seven,
                    CardRank.Eight,
                    CardRank.Nine,
                    CardRank.Ten
                };

                if (lbPossibleValues.SelectedIndex >= 0 && lbPossibleValues.SelectedIndex < ranks.Length)
                    demandedRank = ranks[lbPossibleValues.SelectedIndex];
            }
            else
            {
                CardSuit[] suits = new CardSuit[]
                {
                    CardSuit.Pike,
                    CardSuit.Clover,
                    CardSuit.Tile,
                    CardSuit.Heart
                };

                if (lbPossibleValues.SelectedIndex >= 0 && lbPossibleValues.SelectedIndex < suits.Length)
                    demandedSuit = suits[lbPossibleValues.SelectedIndex];
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PossibleValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbPossibleValues.SelectedIndex != -1)
                btnOk.Enabled = true;
            else
                btnOk.Enabled = false;
        }


        public CardSuit? DemandedSuit
        {
            get
            {
                return demandedSuit;
            }
        }

        public CardRank? DemandedRank
        {
            get
            {
                return demandedRank;
            }
        }
    }

    public enum DemandDialogType
    {
        JackDemand, AceDemand
    }
}
