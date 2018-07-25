using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Makao
{
    public delegate void UserSelectionChangeEventHandler(object sender, UserSelectionChangeEventArgs e);

    public delegate CardSuit? SuitDemandDelegate();
    public delegate CardRank? RankDemandDelegate();
    public delegate bool FirstCardMatchDelegate(Card firstMatch);

    public class User : Player
    {
        private int visibleCardIndex = 0;
        private List<Card> selectedCards = new List<Card>();
        private bool moveMade = false;
        private bool defferMoveMadeDeclaration = false;

        public SuitDemandDelegate SuitDemand;
        public RankDemandDelegate RankDemand;
        public FirstCardMatchDelegate FirstCardMatch;

        public event UserSelectionChangeEventHandler SelectionChanged;
        public event EventHandler VisibleCardIndexChanged;
        public event EventHandler UsersTurn;
        public event EventHandler UsersMoveMade;

        public User(string name) :
            base(name)
        {
        }

        public void DeclareMoveMade()
        {
            lock (this)
            {
                moveMade = true;
                defferMoveMadeDeclaration = false;
                Monitor.Pulse(this);
            }
        }

        public bool DecrementVisibleCardIndex()
        {
            if (visibleCardIndex > 0)
            {
                --visibleCardIndex;
                VisibleCardIndexChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public override bool DecideIfPushFirstMatch(Card firstMatch)
        {
            return FirstCardMatch != null ? FirstCardMatch(firstMatch) : false;
        }

        public override CardSuit? GetAceDemand()
        {
            return SuitDemand != null ? SuitDemand() : null;
        }

        public override CardRank? GetJackDemand()
        {
            return RankDemand != null ? RankDemand() : null;
        }

        public bool IncrementVisibleCardIndex()
        {
            if (Cards.Count - visibleCardIndex > 5)
            {
                ++visibleCardIndex;
                VisibleCardIndexChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public override void MakeMove()
        {
            AppControl.MainWindow.Invoke(UsersTurn);

            lock (this)
            {
                while (!moveMade)
                    Monitor.Wait(this);
                moveMade = false;
            }
            
            AppControl.MainWindow.Invoke(UsersMoveMade);
        }

        public void PushCardsToDeck(Deck deck)
        {
            if (selectedCards.Count > 0)
            {
                int cardsCountAfterPush = Cards.Count - selectedCards.Count;
                if (cardsCountAfterPush <= 5)
                    VisibleCardIndex = 0;
                else if (cardsCountAfterPush - visibleCardIndex < 5)
                    VisibleCardIndex = cardsCountAfterPush - 5;

                int[] indices = new int[selectedCards.Count];
                for (int i = 0; i < indices.Length; ++i)
                    indices[i] = Cards.FindIndex(c => c == selectedCards[i]);
                UnselectAllCards();
                PushCardsToDeck(deck, indices);
            }
        }

        public override void PushCardsToDeck(Deck deck, int[] cardsIndices)
        {
            base.PushCardsToDeck(deck, cardsIndices);

            if (!DefferMoveMadeDeclaration)
            {
                DeclareMoveMade();
            }
        }

        public bool SelectCard(int visibleIndex)
        {
            bool isValidToSelect = false;
            Card card = Cards[visibleCardIndex + visibleIndex];

            if (selectedCards.Count == 0)
                isValidToSelect = true;
            else if (card.Rank == selectedCards.First().Rank)
            {
                if (card.Rank == CardRank.King)
                {
                    if (!MakaoFunctions.IsAggressiveCard(card) && !MakaoFunctions.IsAggressiveCard(selectedCards.First()))
                        isValidToSelect = true;
                }
                else
                    isValidToSelect = true;
            }

            if (isValidToSelect)
            {
                selectedCards.Add(card);
                SelectionChanged?.Invoke(this, new UserSelectionChangeEventArgs(UserSelectionChangeType.Selected, visibleIndex));
            }
            return isValidToSelect;
        }

        public override void TakeCardsFromDeck(Deck deck, int numOfCards)
        {
            base.TakeCardsFromDeck(deck, numOfCards);
            DeclareMoveMade();
        }

        public void UnselectAllCards()
        {
            selectedCards.Clear();
            SelectionChanged?.Invoke(this, new UserSelectionChangeEventArgs(UserSelectionChangeType.SelectionCleared));
        }

        public void UnselectCard(int visibleIndex)
        {
            Card card = Cards[visibleCardIndex + visibleIndex];
            selectedCards.Remove(card);
            SelectionChanged?.Invoke(this, new UserSelectionChangeEventArgs(UserSelectionChangeType.Unselected, visibleIndex));
        }

        public override void WaitTurns(uint turnsToWait)
        {
            base.WaitTurns(turnsToWait);
            DeclareMoveMade();
        }

        public bool DefferMoveMadeDeclaration
        {
            get
            {
                return defferMoveMadeDeclaration;
            }
            
            set
            {
                defferMoveMadeDeclaration = value;
            }
        }

        public bool MoveMade
        {
            get
            {
                return moveMade;
            }

            set
            {
                moveMade = value;
            }
        }

        public int VisibleCardIndex
        {
            get
            {
                return visibleCardIndex;
            }
            set
            {
                if (value == 0 || (value > 0 && Cards.Count - value >= 5))
                {
                    visibleCardIndex = value;
                    VisibleCardIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public List<Card> SelectedCards
        {
            get
            {
                return selectedCards;
            }
        }
    }

    public class UserSelectionChangeEventArgs : EventArgs
    {
        private UserSelectionChangeType changeType;
        private int visibleIndex;

        public UserSelectionChangeEventArgs(UserSelectionChangeType changeType)
        {
            if (changeType != UserSelectionChangeType.SelectionCleared)
                throw new ArgumentException(string.Format("Enum value {0} must specify card index info", changeType), "changeType");

            this.changeType = changeType;
            visibleIndex = -1;
        }

        public UserSelectionChangeEventArgs(UserSelectionChangeType changeType, int cardIndex)
        {
            if (changeType == UserSelectionChangeType.SelectionCleared)
                throw new ArgumentException("UserSelectionChangeType.SelectionCleared cannot contain card index info",
                    "changeType");

            this.changeType = changeType;
            visibleIndex = cardIndex;
        }

        public UserSelectionChangeType ChangeType
        {
            get
            {
                return changeType;
            }
        }

        public int VisibleIndex
        {
            get
            {
                return visibleIndex;
            }
        }
    }

    public enum UserSelectionChangeType
    {
        Selected, Unselected, SelectionCleared
    }
}
