using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace Makao
{
    class PlayersMenu : AppControl
    {
        private ComboBox cbNumOfPlayers;
        private MButton btnStartGame, btnReturn;
        private TextBox[] tbPlayersName;
        private Label lbComboLabel;
        private Label[] lbPlayersOrder;
        private Label[] lbCpuMarks;
        private Label lbMissingNames;

        private const int textboxWidth = 400, textboxHeight = 25, textboxDistance = 30;
        private const int comboWidth = 100, comboHeight = 200;
        private const int firstTextboxVertPos = 150;
        private const int buttonsWidth = 200, buttonsHeight = 70, buttonsDist = 50;

        private IntPtr keyboardHook;
        private HookProc keepMeAlive;

        public override void InitializeWindow()
        {
            mainWindow.BackColor = Color.FromArgb(0, 128, 0);
            mainWindow.Paint += new PaintEventHandler(MainWnd_Paint);
            mainWindow.Resize += new EventHandler(MainWnd_Resize);

            cbNumOfPlayers = new ComboBox();
            cbNumOfPlayers.Name = "cbNumOfPlayers";
            cbNumOfPlayers.DropDownStyle = ComboBoxStyle.DropDownList;
            cbNumOfPlayers.Items.AddRange(new object[]
            {
                "2", "3", "4", "5", "6"
            });
            cbNumOfPlayers.Size = new Size(comboWidth, comboHeight);
            cbNumOfPlayers.SelectedIndexChanged += new EventHandler(NumOfPlayers_SelectedIndexChanged);

            lbComboLabel = new Label();
            lbComboLabel.Name = "lbComboLabel";
            lbComboLabel.Text = "Określ liczbę graczy:";
            lbComboLabel.Font = AppFont;
            lbComboLabel.AutoSize = true;

            btnStartGame = new MButton();
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Text = "Rozpocznij grę";
            btnStartGame.Size = new Size(buttonsWidth, buttonsHeight);
            btnStartGame.Click += new EventHandler(StartGame_Click);
            btnReturn = new MButton();
            btnReturn.Name = "btnReturn";
            btnReturn.Text = "Wróć";
            btnReturn.Size = new Size(buttonsWidth, buttonsHeight);
            btnReturn.Click += new EventHandler(Return_Click);

            tbPlayersName = new TextBox[6];
            for (int i = 0; i < tbPlayersName.Length; ++i)
            {
                tbPlayersName[i] = new TextBox();
                tbPlayersName[i].Name = string.Format("tbPlayersName[{0}]", i);
                tbPlayersName[i].Size = new Size(textboxWidth, textboxHeight);
                tbPlayersName[i].Font = AppFont;
                tbPlayersName[i].MaxLength = 30;
            }

            lbPlayersOrder = new Label[6];
            for (int i = 0; i < lbPlayersOrder.Length; ++i)
            {
                lbPlayersOrder[i] = new Label();
                lbPlayersOrder[i].Name = string.Format("lbPlayersOrder[{0}]", i);
                lbPlayersOrder[i].Text = string.Format("{0}.", i + 1);
                lbPlayersOrder[i].AutoSize = true;
                lbPlayersOrder[i].Font = AppFont;
            }
            lbCpuMarks = new Label[5];
            for (int i = 0; i < lbCpuMarks.Length; ++i)
            {
                lbCpuMarks[i] = new Label();
                lbCpuMarks[i].Name = string.Format("lbCpuMarks[{0}]", i);
                lbCpuMarks[i].Text = "CPU";
                lbCpuMarks[i].AutoSize = true;
                lbCpuMarks[i].Font = AppFont;
            }

            lbMissingNames = new Label();
            lbMissingNames.Name = "lbMissingNames";
            lbMissingNames.Text = "Nie wpisano imion wszystkich graczy";
            lbMissingNames.ForeColor = Color.Red;
            lbMissingNames.AutoSize = true;
            lbMissingNames.Font = AppFont;
            lbMissingNames.Visible = false;

            Control[] ctrls = new Control[22];
            ctrls[0] = cbNumOfPlayers;
            ctrls[1] = btnStartGame;
            ctrls[2] = btnReturn;
            tbPlayersName.CopyTo(ctrls, 3);
            ctrls[9] = lbComboLabel;
            lbPlayersOrder.CopyTo(ctrls, 10);
            lbCpuMarks.CopyTo(ctrls, 16);
            ctrls[21] = lbMissingNames;
            foreach (Control c in ctrls)
                c.TabStop = false;
            mainWindow.Controls.AddRange(ctrls);

            Configure();

            mainWindow.RaiseResizeEvent();

            keepMeAlive = new HookProc(KeyboardHook);
            keyboardHook = SetWindowsHookEx(2, keepMeAlive, IntPtr.Zero, GetCurrentThreadId());  // 2 == WH_KEYBOARD
        }

        public override void Dispose()
        {
            UnhookWindowsHookEx(keyboardHook);

            mainWindow.Paint -= new PaintEventHandler(MainWnd_Paint);
            mainWindow.Resize -= new EventHandler(MainWnd_Resize);

            Control[] ctrl = new Control[mainWindow.Controls.Count];
            mainWindow.Controls.CopyTo(ctrl, 0);
            foreach (Control c in ctrl)
                c.Dispose();
        }

        public int GetNumberOfPlayers()
        {
            return cbNumOfPlayers.SelectedIndex + 2;
        }

        public void Configure()
        {
            try
            {
                string[] lines = File.ReadAllLines("cfg.ini");
                int numOfPlayers = int.Parse(lines[0]);
                if (numOfPlayers < 2 || numOfPlayers > 6)
                    numOfPlayers = 2;
                cbNumOfPlayers.SelectedIndex = numOfPlayers - 2;

                for (int i = 0; i < numOfPlayers && i + 1 < lines.Length; ++i)
                {
                    if (lines[i + 1].Length > 30)
                        lines[i + 1] = lines[i + 1].Substring(0, 30);

                    tbPlayersName[i].Text = lines[i + 1];
                }
            }
            catch (Exception)
            {
                ConfigureWithDefaultValues();
            }
        }

        public void ConfigureWithDefaultValues()
        {
            cbNumOfPlayers.SelectedIndex = 0;
            foreach (TextBox t in tbPlayersName)
                t.Text = string.Empty;
        }

        private void MainWnd_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(mainWindow.BackColor), mainWindow.DisplayRectangle);
        }

        private void MainWnd_Resize(object sender, EventArgs e)
        {
            cbNumOfPlayers.Location = new Point((mainWindow.Width + textboxWidth) / 2 - comboWidth, 100);
            lbComboLabel.Location = new Point((mainWindow.Width + textboxWidth) / 2 - comboWidth - 20 - lbComboLabel.Width,
                100 - (cbNumOfPlayers.ItemHeight - lbComboLabel.Height) / 2);

            int sel = GetNumberOfPlayers();
            int charWidth;
            using (Graphics g = mainWindow.CreateGraphics())
            {
                g.PageUnit = GraphicsUnit.Pixel;
                SizeF aSize = g.MeasureString("A", AppFont);
                charWidth = (int)aSize.Width;
            }
            for (int i = 0; i < sel; ++i)
            {
                tbPlayersName[i].Location = new Point((mainWindow.Width - textboxWidth) / 2, firstTextboxVertPos + i * textboxDistance);
                tbPlayersName[i].Visible = true;

                lbPlayersOrder[i].Location = new Point((mainWindow.Width - textboxWidth) / 2 - 2 * charWidth - 5,
                    firstTextboxVertPos + i * textboxDistance + (textboxHeight - lbPlayersOrder[i].Height) / 2);
                lbPlayersOrder[i].Visible = true;

                if (i > 0)
                {
                    lbCpuMarks[i - 1].Location = new Point((mainWindow.Width + textboxWidth) / 2 + charWidth,
                        firstTextboxVertPos + i * textboxDistance + (textboxHeight - lbPlayersOrder[i].Height) / 2);
                    lbCpuMarks[i - 1].Visible = true;
                }
            }
            for (int i = sel; i < tbPlayersName.Length; ++i)
            {
                tbPlayersName[i].Text = string.Empty;
                tbPlayersName[i].Visible = false;

                lbPlayersOrder[i].Visible = false;
                lbCpuMarks[i - 1].Visible = false;
            }

            int buttonHorzPos = (mainWindow.Width - 2 * buttonsWidth - buttonsDist) / 2;
            int buttonVertPos = firstTextboxVertPos + 6 * textboxDistance + 50;
            btnStartGame.Location = new Point(buttonHorzPos, buttonVertPos);
            buttonHorzPos += buttonsWidth + buttonsDist;
            btnReturn.Location = new Point(buttonHorzPos, buttonVertPos);

            const int yMissingNamesMsg = firstTextboxVertPos + 6 * textboxDistance + 150;
            int xMissingNamesMsg = (mainWindow.Width - lbMissingNames.Width) / 2;
            lbMissingNames.Location = new Point(xMissingNamesMsg, yMissingNamesMsg);
        }

        private void NumOfPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainWindow.RaiseResizeEvent();
        }

        private void RemoveRepeatingNames(string[] namesTable)
        {
            Dictionary<string, int> uniqueNames = new Dictionary<string, int>();
            foreach (string s in namesTable)
            {
                if (!uniqueNames.ContainsKey(s))
                {
                    uniqueNames.Add(s, 1);
                }
                else
                {
                    ++uniqueNames[s];
                }
            }

            if (uniqueNames.Count != namesTable.Length)
            {
                for (int i = 0; i < namesTable.Length; ++i)
                {
                    if (uniqueNames[namesTable[i]] > 1)
                    {
                        string renamed;
                        for (int j = 1; uniqueNames.ContainsKey((renamed = namesTable[i] + string.Format(" ({0})", j))); ++j)
                        {
                        }

                        uniqueNames.Add(renamed, 1);
                        namesTable[i] = renamed;
                    }
                }
            }
        }

        private void StartGame_Click(object sender, EventArgs e)
        {
            int playersCount = GetNumberOfPlayers();
            int i;
            for (i = 0; i < playersCount; ++i)
            {
                if (tbPlayersName[i].Text == string.Empty)
                {
                    lbMissingNames.Visible = true;
                    tbPlayersName[i].Focus();
                    break;
                }
            }
            if (i == playersCount)
            {
                string[] names = new string[playersCount];
                for (int j = 0; j < names.Length; ++j)
                {
                    names[j] = tbPlayersName[j].Text;
                }

                // rename players with same names
                RemoveRepeatingNames(names);

                // save configuration to .ini file
                WriteConfigToIni(names);

                CtrlInstance = new Game(names);
            }
        }

        private void Return_Click(object sender, EventArgs e)
        {
            CtrlInstance = new StartingMenu();
        }

        private void WriteConfigToIni(string[] names)
        {
            string[] iniFileContent = new string[names.Length + 1];
            iniFileContent[0] = names.Length.ToString();

            names.CopyTo(iniFileContent, 1);

            try
            {
                File.WriteAllLines("cfg.ini", iniFileContent);
            }
            catch (Exception)
            {
            }
        }

        private IntPtr KeyboardHook(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            if (code == 0) // HC_ACTION
            {
                Keys pressed = (Keys)wParam;
                if (pressed == Keys.Tab || pressed == Keys.Down || pressed == Keys.Up)
                {
                    if (((int)lParam & (1 << 31)) != 0)
                    {
                        int focusedTextbox = -1;
                        int sel = GetNumberOfPlayers();
                        for (int i = 0; i < sel; ++i)
                        {
                            if (tbPlayersName[i].Focused)
                            {
                                focusedTextbox = i;
                                break;
                            }
                        }

                        if (focusedTextbox == -1)
                        {
                            tbPlayersName[0].Select();
                            focusedTextbox = 0;
                        }
                        else
                        {
                            if (pressed == Keys.Up)
                            {
                                if (--focusedTextbox < 0)
                                    focusedTextbox = sel - 1;
                            }
                            else
                                focusedTextbox = (focusedTextbox + 1) % sel;
                            tbPlayersName[focusedTextbox].Select();
                        }

                        if (pressed == Keys.Tab)
                            tbPlayersName[focusedTextbox].SelectAll();
                        else
                            tbPlayersName[focusedTextbox].Select(tbPlayersName[focusedTextbox].Text.Length, 0);
                    }
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }
    }
}
