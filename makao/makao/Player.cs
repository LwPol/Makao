using System;
using System.Collections.Generic;
using System.Linq;

namespace Makao
{
    public delegate void CardsTakenEventHandler(object sender, CardsTakenEventArgs e);
    public delegate void CardsPushedEventHandler(object sender, CardsPushedEventArgs e);
    public delegate void MakaoEventHandler(object sender, MakaoEventArgs e);

    public abstract class Player
    {
        private string name;
        private List<Card> cards;

        private uint turnsToWait = 0;
        //private bool activeInCurrentTurn = true;

        public event EventHandler NameChanged;
        public event CardsTakenEventHandler CardsTaken;
        public event CardsPushedEventHandler CardsPushed;
        public event MakaoEventHandler Makao;
        public event EventHandler TurnsToWaitChanged;
        public event EventHandler WaitingTurns;

        public Player(string name)
        {
            cards = new List<Card>();
            this.name = name;
        }

        public abstract bool DecideIfPushFirstMatch(Card firstMatch);

        public abstract CardRank? GetJackDemand();

        public abstract CardSuit? GetAceDemand();

        public abstract void MakeMove();

        public void MakeMoveIfPossible()
        {
            if (TurnsToWait > 0)
            {
                --TurnsToWait;
            }
            if (TurnsToWait == 0)
            {
                MakeMove();
            }
        }

        public virtual void PushCardsToDeck(Deck deck, int[] cardsIndices)
        {
            Card[] cardsToPush = new Card[cardsIndices.Length];
            for (int i = 0; i < cardsIndices.Length; ++i)
                cardsToPush[i] = cards[cardsIndices[i]];
            
            foreach (Card c in cardsToPush)
                cards.Remove(c);

            deck.PushCardsOnTop(cardsToPush);

            //ActiveInCurrentTurn = true;

            CardsPushed?.Invoke(this, new CardsPushedEventArgs(cardsIndices.Length));

            if (cards.Count == 0)
            {
                Makao?.Invoke(this, new MakaoEventArgs(MakaoEvent.PastMakao));
            }
            else if (cards.Count == 1)
            {
                Makao?.Invoke(this, new MakaoEventArgs(MakaoEvent.Makao));
            }
        }

        public virtual void TakeCardsFromDeck(Deck deck, int numOfCards)
        {
            try
            {
                Card[] newCards = deck.TakeCardsFromTop(numOfCards);
                cards.AddRange(newCards.Reverse());
            }
            catch (ArgumentException)
            {
                throw new PlayerTakingCardsException(deck, this, numOfCards);
            }

            //ActiveInCurrentTurn = true;

            CardsTaken?.Invoke(this, new CardsTakenEventArgs(numOfCards));
        }

        public virtual void WaitTurns(uint turnsToWait)
        {
            TurnsToWait = turnsToWait;
            WaitingTurns?.Invoke(this, EventArgs.Empty);
        }

        public void TakeSingleCardFromDeck(Deck deck)
        {
            try
            {
                cards.Add(deck.TakeTopCard());
            }
            catch (InvalidOperationException)
            {
                throw new PlayerTakingCardsException(deck, this, 1);
            }

           // ActiveInCurrentTurn = true;

            CardsTaken?.Invoke(this, new CardsTakenEventArgs(1));
        }

        /*public bool ActiveInCurrentTurn
        {
            get
            {
                return activeInCurrentTurn;
            }

            set
            {
                activeInCurrentTurn = value;
            }
        }*/

        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
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
                /*if (turnsToWait > 0)
                    ActiveInCurrentTurn = false;*/
                TurnsToWaitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class PlayerTakingCardsException : Exception
    {
        Deck theDeck;
        Player thePlayer;
        int numOfCardsToTake;

        public PlayerTakingCardsException(Deck deck, Player player, int numOfCardsToTake)
        {
            theDeck = deck;
            thePlayer = player;
            this.numOfCardsToTake = numOfCardsToTake;
        }

        public override string Message
        {
            get
            {
                return "Deck doesn't have enought cards for player's request";
            }
        }

        public Deck TheDeck
        {
            get
            {
                return theDeck;
            }
        }

        public Player ThePlayer
        {
            get
            {
                return thePlayer;
            }
        }

        public int NumOfCardsToTake
        {
            get
            {
                return numOfCardsToTake;
            }
        }
    }

    public class CardsTakenEventArgs : EventArgs
    {
        private int takenCount;

        public CardsTakenEventArgs(int taken)
        {
            takenCount = taken;
        }

        public int TakenCount
        {
            get
            {
                return takenCount;
            }
        }
    }

    public class CardsPushedEventArgs : EventArgs
    {
        private int pushedCount;

        public CardsPushedEventArgs(int pushed)
        {
            pushedCount = pushed;
        }

        public int PushedCount
        {
            get
            {
                return pushedCount;
            }
        }
    }

    public enum MakaoEvent
    {
        Makao, PastMakao
    }

    public class MakaoEventArgs : EventArgs
    {
        private MakaoEvent eventReason;

        public MakaoEventArgs(MakaoEvent reason)
        {
            eventReason = reason;
        }

        public MakaoEvent EventReason
        {
            get
            {
                return eventReason;
            }
        }
    }
}
