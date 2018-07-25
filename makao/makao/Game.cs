using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Makao
{
    class Game : AppControl
    {
        //private Deck mainDeck;
        //private Deck makaoStack;

        //private User benevolentUser;
        //private CpuPlayer[] cpuPlayers;

        private List<Drawable> drawableObjects;
        private List<IClickable> clickableObjects;

        private CardDisplayer topOfStack;
        private CardDisplayer[] usersCardsDisp;
        private Arrow[] arrows;

        private MButton btnPushCards;
        private MButton btnTakeCards;
        private MButton btnFourFold;
        private MButton btnSkip;

        private NamesTable namesTable;

        private Label lbTemporaryMsg;
        private Label lbGameStateMsg;

        private GameState state;
        private GameLoop mainLoop;

        private IntPtr keyboardHook;
        private HookProc keepMeAlive;

        public Game(string[] playerNames)
        {
            state = new GameState(playerNames);
            state.CardsToTakeChanged += new EventHandler(GameState_CardsToTakeChanged);
            state.TurnsToWaitChanged += new EventHandler(GameState_TurnsToWaitChanged);
            state.JackDemandChanged += new EventHandler(GameState_JackDemandChanged);
            state.AceSuitChanged += new EventHandler(GameState_AceSuitChanged);
            state.GameEnded += new EventHandler(GameState_GameEnded);
            state.CurrentPlayerChanged += new EventHandler(GameState_CurrentPlayerChanged);

            namesTable = new NamesTable(playerNames);

            mainLoop = new GameLoop(state);
        }

        public override void InitializeWindow()
        {
            mainWindow.BackColor = Color.FromArgb(0, 128, 0);
            mainWindow.Paint += new PaintEventHandler(MainWnd_Paint);
            mainWindow.MouseUp += new MouseEventHandler(MainWnd_MouseUp);
            mainWindow.Resize += new EventHandler(MainWnd_Resize);
            
            state.MakaoStack.Shuffled += new EventHandler(MakaoStack_Shuffled);
            
            state.LordAndSaviour.CardsPushed += new CardsPushedEventHandler(User_CardsPushed);
            state.LordAndSaviour.CardsPushed += new CardsPushedEventHandler(Players_CardsPushed);
            state.LordAndSaviour.CardsTaken += new CardsTakenEventHandler(User_CardsTaken);
            state.LordAndSaviour.CardsTaken += new CardsTakenEventHandler(Players_CardsTaken);
            state.LordAndSaviour.SelectionChanged += new UserSelectionChangeEventHandler(User_SelectionChange);
            state.LordAndSaviour.VisibleCardIndexChanged += new EventHandler(User_VisibleCardIndexChanged);
            state.LordAndSaviour.UsersTurn += new EventHandler(User_UsersTurn);
            state.LordAndSaviour.UsersMoveMade += new EventHandler(User_UsersMoveMade);
            state.LordAndSaviour.Makao += new MakaoEventHandler(User_Makao);
            state.LordAndSaviour.TurnsToWaitChanged += new EventHandler(Players_TurnsToWaitChanged);

            state.LordAndSaviour.RankDemand = new RankDemandDelegate(User_JackDemandChoice);
            state.LordAndSaviour.SuitDemand = new SuitDemandDelegate(User_AceDemandChoice);
            state.LordAndSaviour.FirstCardMatch = new FirstCardMatchDelegate(User_IfPushFirstMatch);

            foreach (var player in state.CpuPlayers)
            {
                player.CardsPushed += new CardsPushedEventHandler(Players_CardsPushed);
                player.CardsTaken += new CardsTakenEventHandler(Players_CardsTaken);
                player.TurnsToWaitChanged += new EventHandler(Players_TurnsToWaitChanged);
            }

            try
            {
                InitializeUIComponents();
            }
            catch (FileNotFoundException ex)
            {
                string msg = string.Format("{0}: {1}", ex.Message, ex.FileName);
                MessageBox.Show(mainWindow, msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                CtrlInstance = new PlayersMenu();
                return;
            }

            mainWindow.RaiseResizeEvent();

            keepMeAlive = new HookProc(KeyboardHook);
            keyboardHook = SetWindowsHookEx(2, keepMeAlive, IntPtr.Zero, GetCurrentThreadId()); // 2 == WH_KEYBOARD

            mainLoop.BeginGame();
        }

        public override void Dispose()
        {
            UnhookWindowsHookEx(keyboardHook);

            mainWindow.Paint -= new PaintEventHandler(MainWnd_Paint);
            mainWindow.MouseUp -= new MouseEventHandler(MainWnd_MouseUp);
            mainWindow.Resize -= new EventHandler(MainWnd_Resize);

            Control[] ctrls = new Control[mainWindow.Controls.Count];
            mainWindow.Controls.CopyTo(ctrls, 0);
            foreach (var c in ctrls)
                c.Dispose();
        }

        private void ButtonFourFold_Click(object sender, EventArgs e)
        {
            state.LordAndSaviour.WaitTurns(state.TurnsToWait);
        }

        private void ButtonPush_Click(object sender, EventArgs e)
        {
            if (state.LordAndSaviour.SelectedCards.Count > 0)
            {
                if (state.ValidateCardForPush(state.LordAndSaviour.SelectedCards.First()))
                {
                    state.LordAndSaviour.PushCardsToDeck(state.MakaoStack);
                }
                else
                {
                    state.LordAndSaviour.UnselectAllCards();

                    DisplayTemporaryMessage("Aktualnie nie możesz wyłożyć wybranych kart", 3000);
                }
            }
        }

        private void ButtonSkip_Click(object sender, EventArgs e)
        {
            state.Skip = true;

            state.CardsToTakeChanged -= new EventHandler(GameState_CardsToTakeChanged);
            state.TurnsToWaitChanged -= new EventHandler(GameState_TurnsToWaitChanged);
            state.JackDemandChanged -= new EventHandler(GameState_JackDemandChanged);
            state.AceSuitChanged -= new EventHandler(GameState_AceSuitChanged);
            state.CurrentPlayerChanged -= new EventHandler(GameState_CurrentPlayerChanged);

            btnSkip.Visible = false;
        }

        private void ButtonTake_Click(object sender, EventArgs e)
        {
            state.LordAndSaviour.UnselectAllCards();

            state.TransferCardsToPlayer(state.LordAndSaviour);
        }

        private void CardDisplayer_Click(object sender, EventArgs e)
        {
            CardDisplayer displayer = (CardDisplayer)sender;

            int dispIndex = int.MinValue;
            for (int i = 0; i < usersCardsDisp.Length; ++i)
            {
                if (ReferenceEquals(displayer, usersCardsDisp[i]))
                {
                    dispIndex = i;
                    break;
                }
            }

            if (displayer.Selected)
            {
                state.LordAndSaviour.UnselectCard(dispIndex);
            }
            else
            {
                state.LordAndSaviour.SelectCard(dispIndex);
            }
        }

        private void DisplayStateMessage(string msg)
        {
            lbGameStateMsg.Text = msg;
            SetMessagesPosition();
            lbGameStateMsg.Visible = true;
        }

        private void DisplayTemporaryMessage(string msg, int miliseconds)
        {
            if (lbTemporaryMsg.Tag != null)
                ((System.Timers.Timer)lbTemporaryMsg.Tag).Stop();

            lbTemporaryMsg.Text = msg;
            SetMessagesPosition();
            lbTemporaryMsg.Visible = true;

            var timer = new System.Timers.Timer();
            timer.Interval = miliseconds;
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler((sender, e) =>
            {
                if (!lbTemporaryMsg.IsDisposed && ReferenceEquals(lbTemporaryMsg.Tag, sender))
                {
                    lbTemporaryMsg.Visible = false;
                    lbTemporaryMsg.Tag = null;
                }

                ((System.Timers.Timer)sender).Dispose();
            });
            timer.SynchronizingObject = mainWindow;

            lbTemporaryMsg.Tag = timer;

            timer.Start();
        }

        private void GameState_AceSuitChanged(object sender, EventArgs e)
        {
            if (state.AceSuit.HasValue)
                DisplayStateMessage("Żądany kolor: " + Card.SuitAsString(state.AceSuit.Value));
            else
                HideStateMessage();
        }

        private void GameState_CardsToTakeChanged(object sender, EventArgs e)
        {
            if (state.CardsToTake == 1)
                HideStateMessage();
            else
                DisplayStateMessage("Karty do pobrania: " + state.CardsToTake);
        }

        private void GameState_CurrentPlayerChanged(object sender, EventArgs e)
        {
            MethodInvoker handler = () =>
            {
                string currentPlayerName = state.CurrentPlayer.Name;

                for (int i = 0; i < namesTable.Names.Length; ++i)
                {
                    if (namesTable.Names[i] == currentPlayerName)
                    {
                        namesTable.CurrentNameIndex = i;
                        break;
                    }
                }

                mainWindow.Invalidate();
            };

            mainWindow.Invoke(handler);
        }

        private void GameState_GameEnded(object sender, EventArgs e)
        {
            MethodInvoker gameEndedHandler = () =>
            {
                mainLoop.Running = false;

                MessageBox.Show(MainWindow, "Game Ended!", "This is the end...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CtrlInstance = new StartingMenu();
            };

            mainWindow.Invoke(gameEndedHandler);
        }

        private void GameState_JackDemandChanged(object sender, EventArgs e)
        {
            if (state.JackDemand.Active)
                DisplayStateMessage("Żądana karta: " + Card.RankAsString(state.JackDemand.Rank));
            else
            {
                state.TopCardActive = false;
                HideStateMessage();
            }
        }

        private void GameState_TurnsToWaitChanged(object sender, EventArgs e)
        {
            if (state.TurnsToWait == 0)
                HideStateMessage();
            else
                DisplayStateMessage("Ilość kolejek do przeczekania: " + state.TurnsToWait);
        }

        private void HideStateMessage()
        {
            lbGameStateMsg.Visible = false;
        }

        private int HorizontalToPixels(float value)
        {
            return (int)(value * mainWindow.ClientSize.Width);
        }

        private void InitializeUIComponents()
        {
            drawableObjects = new List<Drawable>();
            clickableObjects = new List<IClickable>();

            CardDisplayer.LoadCardsImages();

            topOfStack = new CardDisplayer();
            usersCardsDisp = new CardDisplayer[5];
            for (int i = 0; i < usersCardsDisp.Length; ++i)
            {
                usersCardsDisp[i] = new CardDisplayer();
                usersCardsDisp[i].Click += new EventHandler(CardDisplayer_Click);
            }

            arrows = new Arrow[2];
            arrows[0] = new Arrow(ArrowOrientation.Right);
            arrows[0].Click += new EventHandler(RightArrow_Click);
            arrows[1] = new Arrow(ArrowOrientation.Left);
            arrows[1].Click += new EventHandler(LeftArrow_Click);
            
            namesTable.Location = new Point(0, 0);
            namesTable.Font = AppFont;

            drawableObjects.Add(topOfStack);
            drawableObjects.AddRange(usersCardsDisp);
            drawableObjects.AddRange(arrows);
            drawableObjects.Add(namesTable);
            clickableObjects.AddRange(usersCardsDisp);
            clickableObjects.AddRange(arrows);


            btnPushCards = new MButton();
            btnPushCards.Name = "btnPushCards";
            btnPushCards.Text = "Wyłóż karty";
            btnPushCards.Enabled = false;
            btnPushCards.Click += new EventHandler(ButtonPush_Click);
            btnTakeCards = new MButton();
            btnTakeCards.Name = "btnTakeCards";
            btnTakeCards.Text = "Pobierz karty";
            btnTakeCards.Enabled = false;
            btnTakeCards.Click += new EventHandler(ButtonTake_Click);
            btnFourFold = new MButton();
            btnFourFold.Name = "btnFourFold";
            btnFourFold.Text = "Odpuść";
            btnFourFold.Visible = false;
            btnFourFold.Click += new EventHandler(ButtonFourFold_Click);
            btnSkip = new MButton();
            btnSkip.Name = "btnSkip";
            btnSkip.Text = "Przewiń grę";
            btnSkip.Visible = false;
            btnSkip.Click += new EventHandler(ButtonSkip_Click);

            lbTemporaryMsg = new Label();
            lbTemporaryMsg.Name = "lbTemporaryMsg";
            lbTemporaryMsg.AutoSize = true;
            lbTemporaryMsg.Visible = false;
            lbTemporaryMsg.Font = AppFont;
            lbTemporaryMsg.ForeColor = Color.FromArgb(230, 0, 0);

            lbGameStateMsg = new Label();
            lbGameStateMsg.Name = "lbGameStateMsg";
            lbGameStateMsg.AutoSize = true;
            lbGameStateMsg.Visible = false;
            lbGameStateMsg.Font = AppFont;
            lbGameStateMsg.ForeColor = Color.FromArgb(230, 0, 0);

            Control[] ctrls = new Control[] {
                btnPushCards,
                btnTakeCards,
                btnFourFold,
                btnSkip,
                lbTemporaryMsg,
                lbGameStateMsg
            };
            mainWindow.Controls.AddRange(ctrls);
        }

        private IntPtr KeyboardHook(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            if (code == 0)  // HC_ACTION 
            {
                if ((Keys)wParam == Keys.F4)
                    return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                int val = (int)lParam;
                if ((val & (1 << 31)) == 0)
                {
                    Keys keyCode = (Keys)wParam;
                    switch (keyCode)
                    {
                        case Keys.Left:
                            state.LordAndSaviour.DecrementVisibleCardIndex();
                            break;
                        case Keys.Right:
                            state.LordAndSaviour.IncrementVisibleCardIndex();
                            break;
                    }
                }
                return (IntPtr)1;
            }
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private void LeftArrow_Click(object sender, EventArgs e)
        {
            state.LordAndSaviour.DecrementVisibleCardIndex();
        }

        private void MainWnd_Paint(object sender, PaintEventArgs e)
        {
            using (Brush bk = new SolidBrush(mainWindow.BackColor))
            {
                e.Graphics.FillRectangle(bk, mainWindow.DisplayRectangle);
            }

            topOfStack.Displayed = state.MakaoStack.TopCard;

            foreach (var obj in drawableObjects)
                obj.Render(e.Graphics);
        }

        private void MainWnd_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (var obj in clickableObjects)
            {
                if (obj.Visible && obj.Contains(e.Location))
                    obj.Clicked();
            }
        }

        private void MainWnd_Resize(object sender, EventArgs e)
        {
            const float xTopOfStack = 0.45f, yTopOfStack = 0.37f;
            const float cardsDispWidth = 0.1f, cardsDispHeight = 0.18f;
            topOfStack.Size = new Size(HorizontalToPixels(cardsDispWidth), VerticalToPixels(cardsDispHeight));
            topOfStack.Location = new Point(HorizontalToPixels(xTopOfStack), VerticalToPixels(yTopOfStack));

            foreach (var displayer in usersCardsDisp)
                displayer.Size = topOfStack.Size;

            const float arrowSize = 0.05f;
            foreach (var arrow in arrows)
                arrow.Size = new Size(HorizontalToPixels(arrowSize), topOfStack.Size.Height);

            const float namesTableWidth = 0.17f, namesTableHeight = 0.08f;
            namesTable.CellSize = new Size(HorizontalToPixels(namesTableWidth), VerticalToPixels(namesTableHeight));

            UpdateCardsDisplayControls();

            SetButtonsLayout();

            SetMessagesPosition();

            mainWindow.Invalidate();
        }

        private void MakaoStack_Shuffled(object sender, EventArgs e)
        {
            if (!state.MakaoStack.Empty)
                DisplayTemporaryMessage("Nastąpiło przetasowanie", 3000);
        }

        private void Players_CardsPushed(object sender, CardsPushedEventArgs e)
        {
            // update names table
            UpdatePlayersCardsCountOnNamesTable((Player)sender);
        }

        private void Players_CardsTaken(object sender, CardsTakenEventArgs e)
        {
            // update names table
            UpdatePlayersCardsCountOnNamesTable((Player)sender);
        }

        private void Players_TurnsToWaitChanged(object sender, EventArgs e)
        {
            Player sentBy = (Player)sender;
            if (namesTable.CurrentName == sentBy.Name)
            {
                namesTable.TurnsWaiting[namesTable.CurrentNameIndex] = sentBy.TurnsToWait > 0 ? (int)sentBy.TurnsToWait - 1 : 0;
            }
            else
            {
                int sentByIndex = Array.IndexOf(namesTable.Names, sentBy.Name);
                if (sentByIndex != -1)
                {
                    namesTable.TurnsWaiting[sentByIndex] = sentBy.TurnsToWait > 0 ? (int)sentBy.TurnsToWait - 1 : 0;
                }
            }
        }

        private void RightArrow_Click(object sender, EventArgs e)
        {
            state.LordAndSaviour.IncrementVisibleCardIndex();
        }

        private void SetButtonsLayout()
        {
            const float buttonWidth = 0.2f, buttonHeight = 0.08f, pushAndTakeHorzDist = 0.05f;
            const float pushAndTakeVertPos = 0.9f;
            float pushOrTakeHorzPos = (1.0f - 2 * buttonWidth - pushAndTakeHorzDist) / 2;

            btnPushCards.Size = new Size(HorizontalToPixels(buttonWidth), VerticalToPixels(buttonHeight));
            btnPushCards.Location = new Point(HorizontalToPixels(pushOrTakeHorzPos), VerticalToPixels(pushAndTakeVertPos));
            pushOrTakeHorzPos += buttonWidth + pushAndTakeHorzDist;
            btnTakeCards.Size = btnPushCards.Size;
            btnTakeCards.Location = new Point(HorizontalToPixels(pushOrTakeHorzPos), VerticalToPixels(pushAndTakeVertPos));

            const float xFourFold = 0.28f, yFourFold = 0.4f;
            const float xSkip = 0.01f, ySkip = 0.5f;
            const float fourFoldWidth = 0.11f, skipWidth = 0.15f;

            btnFourFold.Size = new Size(HorizontalToPixels(fourFoldWidth), VerticalToPixels(buttonHeight));
            btnFourFold.Location = new Point(HorizontalToPixels(xFourFold), VerticalToPixels(yFourFold));
            btnSkip.Size = new Size(HorizontalToPixels(skipWidth), VerticalToPixels(buttonHeight));
            btnSkip.Location = new Point(HorizontalToPixels(xSkip), VerticalToPixels(ySkip));
        }

        private void SetCardsDisplayControlsLayout()
        {
            const float picDist = 0.05f;
            int picDistPixels = HorizontalToPixels(picDist);

            int displayersCount = state.LordAndSaviour.Cards.Count > usersCardsDisp.Length ? usersCardsDisp.Length : state.LordAndSaviour.Cards.Count;
            int drawingWidth = displayersCount * topOfStack.Size.Width + (displayersCount - 1) * picDistPixels;

            const float vertPos = 0.65f;
            int vertPosPixels = VerticalToPixels(vertPos);
            int horzPosPixels = (mainWindow.ClientSize.Width - drawingWidth) / 2;
            for (int i = 0; i < displayersCount; ++i)
            {
                usersCardsDisp[i].Location = new Point(horzPosPixels, vertPosPixels);
                horzPosPixels += usersCardsDisp[i].Size.Width + picDistPixels;
                usersCardsDisp[i].Visible = true;
            }
            for (int i = displayersCount; i < usersCardsDisp.Length; ++i)
                usersCardsDisp[i].Visible = false;

            const float arrowsFromCardDisplayersDist = 0.04f;
            int arrowsFromCardDisplayersDistPixels = HorizontalToPixels(arrowsFromCardDisplayersDist);
            arrows[0].Location = new Point((mainWindow.ClientSize.Width + drawingWidth) / 2 + arrowsFromCardDisplayersDistPixels,
                vertPosPixels);
            arrows[1].Location = new Point((mainWindow.ClientSize.Width - drawingWidth) / 2 - arrows[1].Size.Width - arrowsFromCardDisplayersDistPixels,
                vertPosPixels);

            arrows[0].Visible = state.LordAndSaviour.Cards.Count - state.LordAndSaviour.VisibleCardIndex > 5;
            arrows[1].Visible = state.LordAndSaviour.VisibleCardIndex > 0;
        }

        private void SetMessagesPosition()
        {
            lbTemporaryMsg.Location = new Point(
                (mainWindow.ClientSize.Width - lbTemporaryMsg.Width) / 2,
                VerticalToPixels(0.2f));
            lbGameStateMsg.Location = new Point(
                (mainWindow.ClientSize.Width - lbGameStateMsg.Width) / 2,
                VerticalToPixels(0.15f));
        }

        private void UpdateCardsDisplayControls()
        {
            SetCardsDisplayControlsLayout();
            for (int i = 0; i < usersCardsDisp.Length; ++i)
            {
                if (usersCardsDisp[i].Visible)
                    usersCardsDisp[i].Displayed = state.LordAndSaviour.Cards[state.LordAndSaviour.VisibleCardIndex + i];
                else
                    break;
            }

            mainWindow.Invalidate();
        }

        private void UpdatePlayersCardsCountOnNamesTable(Player player)
        {
            int[] namesTableCardsCount = namesTable.CardsCount;
            if (ReferenceEquals(player, state.LordAndSaviour))
            {
                namesTableCardsCount[0] = player.Cards.Count;
            }
            else
            {
                for (int i = 0; i < state.CpuPlayers.Length; ++i)
                {
                    if (ReferenceEquals(state.CpuPlayers[i], player))
                    {
                        namesTableCardsCount[i + 1] = player.Cards.Count;
                        break;
                    }
                }
            }

            mainWindow.Invalidate();
        }

        private CardSuit? User_AceDemandChoice()
        {
            state.LordAndSaviour.DefferMoveMadeDeclaration = true;

            MethodInvoker dialogInvoke = () =>
            {
                DemandDialog dlg = new DemandDialog(DemandDialogType.AceDemand);
                dlg.ShowDialog(mainWindow);

                if (dlg.DialogResult == DialogResult.OK)
                {
                    if (dlg.DemandedSuit.HasValue && dlg.DemandedSuit.Value != state.MakaoStack.TopCard.Suit)
                    {
                        state.AceSuit = dlg.DemandedSuit.Value;
                    }
                }

                dlg.Dispose();

                state.LordAndSaviour.DeclareMoveMade();
            };

            mainWindow.BeginInvoke(dialogInvoke);

            return null;
        }

        private void User_CardsPushed(object sender, CardsPushedEventArgs e)
        {
            UpdateCardsDisplayControls();
        }

        private void User_CardsTaken(object sender, CardsTakenEventArgs e)
        {
            UpdateCardsDisplayControls();
        }

        private bool User_IfPushFirstMatch(Card match)
        {
            using (FirstCardMatchDialog firstMatchDlg = new FirstCardMatchDialog())
            {
                firstMatchDlg.Size = new Size(HorizontalToPixels(0.2f), VerticalToPixels(0.4f));
                firstMatchDlg.Location = mainWindow.PointToScreen(new Point(HorizontalToPixels(0.2f), VerticalToPixels(0.15f)));
                firstMatchDlg.InitializeComponents(match);

                firstMatchDlg.ShowDialog(mainWindow);

                return firstMatchDlg.DialogResult == DialogResult.Yes;
            }
        }

        private CardRank? User_JackDemandChoice()
        {
            state.LordAndSaviour.DefferMoveMadeDeclaration = true;

            MethodInvoker dialogInvoke = () =>
            {
                DemandDialog dlg = new DemandDialog(DemandDialogType.JackDemand);
                dlg.ShowDialog(mainWindow);

                if (dlg.DialogResult == DialogResult.OK)
                {
                    if (dlg.DemandedRank.HasValue)
                    {
                        state.JackDemand = new JackDemandInfo(dlg.DemandedRank.Value, (uint)state.PlayersInGame.Count);
                    }
                }

                dlg.Dispose();

                state.LordAndSaviour.DeclareMoveMade();
            };

            mainWindow.BeginInvoke(dialogInvoke);

            return null;
        }

        private void User_Makao(object sender, MakaoEventArgs e)
        {
            if (e.EventReason == MakaoEvent.PastMakao && state.PlayersInGame.Count > 2)
            {
                btnSkip.Visible = true;
            }
        }

        private void User_SelectionChange(object sender, UserSelectionChangeEventArgs e)
        {
            if (e.ChangeType == UserSelectionChangeType.SelectionCleared)
            {
                foreach (var disp in usersCardsDisp)
                    disp.Selected = false;
            }
            else if (e.ChangeType == UserSelectionChangeType.Selected)
                usersCardsDisp[e.VisibleIndex].Selected = true;
            else
                usersCardsDisp[e.VisibleIndex].Selected = false;

            mainWindow.Invalidate();
        }

        private void User_UsersMoveMade(object sender, EventArgs e)
        {
            btnPushCards.Enabled = false;
            btnTakeCards.Enabled = false;
            btnFourFold.Visible = false;
        }

        private void User_UsersTurn(object sender, EventArgs e)
        {
            btnPushCards.Enabled = true;

            if (state.MakaoStack.TopCard.Rank == CardRank.Four && state.TopCardActive)
            {
                btnFourFold.Visible = true;
            }
            else
            {
                btnTakeCards.Enabled = true;
            }
        }

        private void User_VisibleCardIndexChanged(object sender, EventArgs e)
        {
            UpdateCardsDisplayControls();
            foreach (var disp in usersCardsDisp)
            {
                if (disp.Visible)
                    disp.Selected = state.LordAndSaviour.SelectedCards.Contains(disp.Displayed);
            }

            mainWindow.Invalidate();
        }

        private int VerticalToPixels(float value)
        {
            return (int)(value * mainWindow.ClientSize.Height);
        }
    }

    public delegate void DisplayTempMessageDelegate(string msg, int miliseconds);
}
