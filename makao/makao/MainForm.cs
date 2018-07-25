using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Makao
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            MouseDown += new MouseEventHandler(MainForm_MouseDown);
            FormClosed += new FormClosedEventHandler(MainForm_Closed);

            AppControl.MainWindow = this;
            AppControl.CtrlInstance = new StartingMenu();
        }

        public void RaiseResizeEvent()
        {
            OnResize(EventArgs.Empty);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
        }

        private void MainForm_Closed(object sender, FormClosedEventArgs e)
        {
            AppControl.CtrlInstance = null;
        }

        [DllImport("kernel32.dll")]
        public static extern bool Beep(uint freq, uint duration);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
