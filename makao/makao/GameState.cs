using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Makao
{
    public class GameState
    {
        private Deck mainDeck = new Deck(true);
        private Deck makaoStack = new Deck();

        private User lordAndSaviour;
        private CpuPlayer[] cpuPlayers;
        private List<Player> playersInGame;
        private int currentPlayerIndex = 0;

        private bool stepBack = false;

        private bool skip = false;

        private uint cardsToTake = 1;
        private uint turnsToWait = 0;
        private bool topCardActive = false;
        private JackDemandInfo jackDemand;
        private CardSuit? aceSuit;

        public event EventHandler CardsToTakeChanged;
        public event EventHandler TurnsToWaitChanged;
        public event EventHandler TopCardActiveChanged;
        public event EventHandler JackDemandChanged;
        public event EventHandler AceSuitChanged;
        public event EventHandler GameEnded;
        public event EventHandler CurrentPlayerChanged;

        private delegate int SearchningPlayerDelegate(int cur);

        public GameState(string[] playerNames)
        {
            lordAndSaviour = new User(playerNames[0]);

            cpuPlayers = new CpuPlayer[playerNames.Length - 1];
            for (int i = 0; i < cpuPlayers.Length; ++i)
            {
                cpuPlayers[i] = new EasyPlayer(playerNames[i + 1], this);
            }

            PushStartingCardToStack();
            mainDeck.Shuffle();

            lordAndSaviour.TakeCardsFromDeck(mainDeck, 5);

            lordAndSaviour.CardsPushed += new CardsPushedEventHandler(Players_CardsPushed);
            lordAndSaviour.CardsTaken += new CardsTakenEventHandler(Players_CardsTaken);
            lordAndSaviour.WaitingTurns += new EventHandler(Players_WaitingTurns);
            lordAndSaviour.MoveMade = false;

            foreach (var player in cpuPlayers)
            {
                player.TakeCardsFromDeck(mainDeck, 5);

                player.CardsPushed += new CardsPushedEventHandler(Players_CardsPushed);
                player.CardsTaken += new CardsTakenEventHandler(Players_CardsTaken);
                player.WaitingTurns += new EventHandler(Players_WaitingTurns);
            }

            playersInGame = new List<Player>();
            playersInGame.Add(lordAndSaviour);
            playersInGame.AddRange(cpuPlayers);
        }

        public void DeactivateJackDemand()
        {
            jackDemand.Deactivate();
        }
        
        public Player MoveToNextPlayer()
        {
            if (stepBack || (makaoStack.TopCard == new Card(CardRank.King, CardSuit.Pike) && TopCardActive))
            {
                return HandleStepBack();
            }

            if (playersInGame[currentPlayerIndex].Cards.Count == 0)
            {
                playersInGame.RemoveAt(currentPlayerIndex);

                if (playersInGame.Count < 2)
                {
                    GameEnded?.Invoke(this, EventArgs.Empty);
                    return null;
                }

                currentPlayerIndex %= playersInGame.Count;
            }
            else
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % playersInGame.Count;
            }

            CurrentPlayerChanged?.Invoke(this, EventArgs.Empty);

            return CurrentPlayer;
        }

        public void PassPlayerWithJackDemand()
        {
            jackDemand.PassPlayer();
        }

        public void Players_CardsPushed(object sender, CardsPushedEventArgs e)
        {
            // update game state
            Player whoPushed = (Player)sender;

            if (AceSuit.HasValue)
            {
                AceSuit = null;
            }

            if (MakaoFunctions.IsAggressiveCard(MakaoStack.TopCard))
            {
                uint totalAggressiveness = 0;
                for (int i = 0; i < e.PushedCount; ++i)
                    totalAggressiveness += MakaoFunctions.GetCardAggressiveness(MakaoStack.Cards[MakaoStack.Cards.Count - 1 - i]);

                if (CardsToTake == 1)
                    CardsToTake = totalAggressiveness;
                else
                    CardsToTake += totalAggressiveness;

                TopCardActive = true;
            }
            else if (MakaoStack.TopCard.Rank == CardRank.Four)
            {
                TurnsToWait += (uint)e.PushedCount;

                TopCardActive = true;
            }
            else if (MakaoStack.TopCard.Rank == CardRank.Jack)
            {
                var rankDemanded = whoPushed.GetJackDemand();
                if (rankDemanded.HasValue)
                {
                    JackDemand = new JackDemandInfo(rankDemanded.Value, (uint)PlayersInGame.Count);
                    if (whoPushed.Cards.Count == 0)
                        PassPlayerWithJackDemand();
                }
                else
                    DeactivateJackDemand();

                TopCardActive = JackDemand.Active;
            }
            else if (MakaoStack.TopCard.Rank == CardRank.Queen)
            {
                CardsToTake = 1;
                TurnsToWait = 0;
                DeactivateJackDemand();

                TopCardActive = true;
            }
            else if (makaoStack.TopCard.Rank == CardRank.Ace)
            {
                var suitDemanded = whoPushed.GetAceDemand();
                if (suitDemanded.HasValue && suitDemanded.Value != MakaoStack.TopCard.Suit)
                    AceSuit = suitDemanded;
                else
                    AceSuit = null;

                TopCardActive = true;
            }
            else
            {
                TopCardActive = false;
                if (JackDemand.Active)
                    PassPlayerWithJackDemand();
            }
        }

        public void Players_CardsTaken(object sender, CardsTakenEventArgs e)
        {
            if (CardsToTake > 1)
            {
                CardsToTake = 1;
                TopCardActive = false;
            }
            else if (JackDemand.Active)
            {
                PassPlayerWithJackDemand();
            }
        }

        private void Players_WaitingTurns(object sender, EventArgs e)
        {
            TopCardActive = false;
            TurnsToWait = 0;
        }

        public void TransferCardsToPlayer(Player taking)
        {
            MethodInvoker shuffle = () =>
            {
                Card topCard = makaoStack.TakeTopCard();
                makaoStack.Shuffle();
                mainDeck.Cards.InsertRange(0, makaoStack.Cards);
                makaoStack.Cards.Clear();
                makaoStack.Cards.Add(topCard);
            };

            if (mainDeck.Empty)
            {
                shuffle();
                if (mainDeck.Empty)
                {
                    // TODO: invoke event ending game...
                }
            }

            if (ValidateCardForPush(mainDeck.TopCard))
            {
                if (taking.DecideIfPushFirstMatch(mainDeck.TopCard))
                {
                    taking.Cards.Add(mainDeck.TakeTopCard());
                    taking.PushCardsToDeck(makaoStack, new int[] { taking.Cards.Count - 1 });
                    return;
                }
            }

            try
            {
                taking.TakeCardsFromDeck(mainDeck, (int)CardsToTake);
            }
            catch (PlayerTakingCardsException ex)
            {
                shuffle();

                if (mainDeck.Cards.Count < ex.NumOfCardsToTake)
                {
                    // TODO: invoke event ending game...
                }

                taking.TakeCardsFromDeck(mainDeck, ex.NumOfCardsToTake);
            }
        }

        public bool ValidateCardForPush(Card card)
        {
            if (card == new Card(CardRank.Queen, CardSuit.Pike))
                return true;

            if (JackDemand.Active)
            {
                if (card.Rank == JackDemand.Rank)
                    return true;
                if (makaoStack.TopCard.Rank == CardRank.Jack && card.Rank == CardRank.Jack)
                    return true;

                return false;
            }
            else
            {
                if (TopCardActive)
                {
                    switch (MakaoStack.TopCard.Rank)
                    {
                        case CardRank.Two:
                            if (card.Rank == CardRank.Two)
                                return true;
                            if (card == new Card(CardRank.Three, makaoStack.TopCard.Suit))
                                return true;
                            if (card.Rank == CardRank.King)
                            {
                                if (MakaoFunctions.IsAggressiveCard(card) && card.Suit == makaoStack.TopCard.Suit)
                                    return true;
                            }
                            break;
                        case CardRank.Three:
                            if (card.Rank == CardRank.Three)
                                return true;
                            if (card == new Card(CardRank.Two, makaoStack.TopCard.Suit))
                                return true;
                            if (card.Rank == CardRank.King)
                            {
                                if (MakaoFunctions.IsAggressiveCard(card) && card.Suit == makaoStack.TopCard.Suit)
                                    return true;
                            }
                            break;
                        case CardRank.Four:
                            if (card.Rank == CardRank.Four)
                                return true;
                            break;
                        case CardRank.Queen:
                            return true;
                        case CardRank.King:
                            if (MakaoFunctions.IsAggressiveKing(card))
                                return true;
                            if (card.Rank == CardRank.Queen && card.Suit == makaoStack.TopCard.Suit)
                                return true;
                            if (MakaoFunctions.IsAggressiveCard(card) && card.Suit == makaoStack.TopCard.Suit)
                                return true;
                            break;
                        case CardRank.Ace:
                            if (AceSuit.HasValue)
                                return MakaoFunctions.AreEqualInRankOrSuit(new Card(CardRank.Ace, AceSuit.Value), card);
                            else
                                return MakaoFunctions.AreEqualInRankOrSuit(makaoStack.TopCard, card);
                    }
                    return false;
                }
                else
                {
                    if (card.Rank == CardRank.Queen)
                        return true;

                    return MakaoFunctions.AreEqualInRankOrSuit(card, makaoStack.TopCard);
                }
            }
        }

        private Player HandleStepBack()
        {
            SearchningPlayerDelegate getPreceedingPlayerIndex = (int cur) =>
            {
                return (cur - 1 < 0) ? playersInGame.Count - 1 : cur - 1;
            };
            SearchningPlayerDelegate getProceedingPlayerIndex = (int cur) =>
            {
                return (cur + 1) % playersInGame.Count;
            };
            
            if (playersInGame[currentPlayerIndex].Cards.Count == 0)
            {
                playersInGame.RemoveAt(currentPlayerIndex);

                if (playersInGame.Count < 2)
                {
                    GameEnded?.Invoke(this, EventArgs.Empty);
                    return null;
                }
                
                if (!stepBack)
                {
                    stepBack = true;
                    
                    for (currentPlayerIndex = getPreceedingPlayerIndex(currentPlayerIndex);
                        playersInGame[currentPlayerIndex].TurnsToWait > 0;
                        currentPlayerIndex = getPreceedingPlayerIndex(currentPlayerIndex))
                    {
                    }
                }
                else
                {
                    stepBack = false;

                    for (currentPlayerIndex = currentPlayerIndex % playersInGame.Count;
                        playersInGame[currentPlayerIndex].TurnsToWait > 0;
                        currentPlayerIndex = getProceedingPlayerIndex(currentPlayerIndex))
                    {
                    }
                }
            }
            else
            {
                if (!stepBack)
                {
                    stepBack = true;

                    for (currentPlayerIndex = getPreceedingPlayerIndex(currentPlayerIndex);
                        playersInGame[currentPlayerIndex].TurnsToWait > 0;
                        currentPlayerIndex = getPreceedingPlayerIndex(currentPlayerIndex))
                    {
                    }
                }
                else
                {
                    stepBack = false;

                    for (currentPlayerIndex = getProceedingPlayerIndex(currentPlayerIndex);
                        playersInGame[currentPlayerIndex].TurnsToWait > 0;
                        currentPlayerIndex = getProceedingPlayerIndex(currentPlayerIndex))
                    {
                    }
                }
            }

            CurrentPlayerChanged?.Invoke(this, EventArgs.Empty);

            return playersInGame[currentPlayerIndex];
        }

        private void PushStartingCardToStack()
        {
            while (MakaoFunctions.IsFunctionalCard(mainDeck.TopCard))
                mainDeck.MoveTopToBottom();

            makaoStack.PushCardsOnTop(mainDeck.TakeTopCard());
        }

        public uint CardsToTake
        {
            get
            {
                return cardsToTake;
            }
            set
            {
                cardsToTake = value;
                CardsToTakeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CpuPlayer[] CpuPlayers
        {
            get
            {
                return cpuPlayers;
            }
        }

        public Player CurrentPlayer
        {
            get
            {
                return playersInGame[currentPlayerIndex];
            }
        }

        public int CurrentPlayerIndex
        {
            get
            {
                return currentPlayerIndex;
            }

            set
            {
                if (value > 0 && value < playersInGame.Count)
                {
                    currentPlayerIndex = value;
                }
            }
        }

        public User LordAndSaviour
        {
            get
            {
                return lordAndSaviour;
            }
        }

        public Deck MainDeck
        {
            get
            {
                return mainDeck;
            }
        }

        public Deck MakaoStack
        {
            get
            {
                return makaoStack;
            }
        }

        public List<Player> PlayersInGame
        {
            get
            {
                return playersInGame;
            }
        }

        public bool Skip
        {
            get
            {
                return skip;
            }
            set
            {
                skip = value;
            }
        }

        public uint TurnsToWait
        {
            get
            {
                return turnsToWait;
            }
            set
            {
                turnsToWait = value;
                TurnsToWaitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TopCardActive
        {
            get
            {
                return topCardActive;
            }
            set
            {
                topCardActive = value;
                TopCardActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public JackDemandInfo JackDemand
        {
            get
            {
                return jackDemand;
            }
            set
            {
                jackDemand = value;
                jackDemand.DemandExpired += (sender, e) => { JackDemandChanged?.Invoke(this, EventArgs.Empty); };
                JackDemandChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CardSuit? AceSuit
        {
            get
            {
                return aceSuit;
            }
            set
            {
                aceSuit = value;
                AceSuitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public struct JackDemandInfo
    {
        private CardRank rank;
        private uint playersToGo;

        public event EventHandler DemandExpired;

        public JackDemandInfo(CardRank rank, uint players)
        {
            this.rank = rank;
            playersToGo = players;
            DemandExpired = null;
        }

        public void Deactivate()
        {
            playersToGo = 0;
            DemandExpired?.Invoke(this, EventArgs.Empty);
        }

        public void PassPlayer()
        {
            if (--playersToGo == 0)
                DemandExpired?.Invoke(this, EventArgs.Empty);
        }

        public CardRank Rank
        {
            get
            {
                return rank;
            }
            set
            {
                rank = value;
            }
        }

        public bool Active
        {
            get
            {
                return playersToGo > 0;
            }
        }

        public uint PlayersToGo
        {
            get
            {
                return playersToGo;
            }
            set
            {
                playersToGo = value;
                if (playersToGo == 0)
                    DemandExpired?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
