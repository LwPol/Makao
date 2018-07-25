using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Makao
{
    class StartingMenu : AppControl
    {
        private MButton btnPlay;
        private MButton btnQuit;

        public override void InitializeWindow()
        {
            mainWindow.BackColor = Color.FromArgb(0, 128, 0);
            mainWindow.Paint += new PaintEventHandler(MainWnd_Paint);
            mainWindow.Resize += new EventHandler(MainWnd_Resize);

            btnPlay = new MButton();
            btnPlay.Name = "btnPlay";
            btnPlay.Text = "Graj";
            btnPlay.Click += new EventHandler(Play_Click);
            btnQuit = new MButton();
            btnQuit.Name = "btnQuit";
            btnQuit.Text = "Wyjdź";
            btnQuit.Click += new EventHandler(Quit_Click);

            mainWindow.Controls.Add(btnPlay);
            mainWindow.Controls.Add(btnQuit);
            mainWindow.RaiseResizeEvent();
        }

        public override void Dispose()
        {
            mainWindow.Paint -= new PaintEventHandler(MainWnd_Paint);
            mainWindow.Resize -= new EventHandler(MainWnd_Resize);

            btnPlay.Dispose();
            btnQuit.Dispose();
        }

        private void MainWnd_Paint(object sender, PaintEventArgs e)
        {
            using (Brush br = new SolidBrush(mainWindow.BackColor))
                e.Graphics.FillRectangle(br, mainWindow.DisplayRectangle);
        }

        private void MainWnd_Resize(object sender, EventArgs e)
        {
            const int buttonsWidth = 200;
            const int buttonsHeight = 70;
            const int verticalButtonsDist = 30;

            btnPlay.Location = new Point((mainWindow.Width - buttonsWidth) / 2,
                (mainWindow.Height - 2 * buttonsHeight - verticalButtonsDist) / 2);
            btnPlay.Size = new Size(buttonsWidth, buttonsHeight);

            btnQuit.Location = new Point(btnPlay.Location.X, btnPlay.Location.Y + buttonsHeight + verticalButtonsDist);
            btnQuit.Size = new Size(buttonsWidth, buttonsHeight);
        }

        private void Play_Click(object sender, EventArgs e)
        {
            CtrlInstance = new PlayersMenu();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            mainWindow.Close();
        }
    }
}
