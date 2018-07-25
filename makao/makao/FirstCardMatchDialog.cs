using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Makao
{
    public class FirstCardMatchDialog : Form
    {
        private MButton btnYes;
        private MButton btnNo;

        private Label lblInfo;

        private CardDisplayer firstCard;

        public FirstCardMatchDialog()
        {
            FormBorderStyle = FormBorderStyle.None;

            CreateHandle();

            Paint += new PaintEventHandler(FirstCardMatchDialog_Paint);
        }

        public void InitializeComponents(Card displayedCard)
        {
            btnYes = new MButton();
            btnNo = new MButton();
            lblInfo = new Label();
            firstCard = new CardDisplayer();

            SuspendLayout();

            Controls.Add(btnYes);
            Controls.Add(btnNo);
            Controls.Add(lblInfo);

            // firstCard

            firstCard.Size = new Size((int)(0.6f * Width), (int)(0.54f * Height));
            firstCard.Location = new Point((Width - firstCard.Size.Width) / 2, (int)(0.05f * Height));
            firstCard.Displayed = displayedCard;

            // lblInfo

            lblInfo.AutoSize = true;
            lblInfo.BackColor = Color.FromArgb(0, Color.Black);
            lblInfo.Font = SystemFonts.DefaultFont;
            lblInfo.Name = "lblInfo";
            lblInfo.Text = string.Format("Wyciągnąłeś kartę: {0} {1}\nCzy chcesz wrzucić tą kartę na stos?",
                displayedCard.RankString, displayedCard.SuitString);
            lblInfo.Location = new Point((Width - lblInfo.Width) / 2, firstCard.Location.Y + firstCard.Size.Height + 10);
            lblInfo.TextAlign = ContentAlignment.TopCenter;

            // buttons
            const int buttonWidth = 50, buttonHeight = 25;
            const int buttonDist = 10;

            // btnYes

            btnYes.Name = "btnYes";
            btnYes.Text = "Tak";
            btnYes.Size = new Size(buttonWidth, buttonHeight);
            btnYes.Location = new Point((Width - 2 * buttonWidth - buttonDist) / 2, lblInfo.Location.Y + lblInfo.Height + 5);
            btnYes.Click += new EventHandler(Buttons_Click);

            // btnNo

            btnNo.Name = "btnNo";
            btnNo.Text = "Nie";
            btnNo.Size = btnYes.Size;
            btnNo.Location = new Point(btnYes.Location.X + buttonWidth + buttonDist, btnYes.Location.Y);
            btnNo.Click += new EventHandler(Buttons_Click);

            ResumeLayout();
        }

        private void Buttons_Click(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, btnYes))
                DialogResult = DialogResult.Yes;
            else
                DialogResult = DialogResult.No;

            Close();
        }

        private void FirstCardMatchDialog_Paint(object sender, PaintEventArgs e)
        {
            using (Brush brush = new SolidBrush(AppControl.MainWindow.BackColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);

            e.Graphics.DrawRectangle(Pens.Blue,
                new Rectangle(new Point(0, 0), new Size(DisplayRectangle.Width - 1, DisplayRectangle.Height - 1)));

            firstCard.Render(e.Graphics);
        }
    }
}
