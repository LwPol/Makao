using System;
using System.Collections.Generic;
using System.Linq;

namespace Makao
{
    public class Deck
    {
        private List<Card> cards;

        public event EventHandler Shuffled;

        #region CTORS
        public Deck()
        {
            cards = new List<Card>();
        }

        public Deck(bool fullDeck) :
            this()
        {
            if (fullDeck)
            {
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 13; ++j)
                    {
                        cards.Add(new Card((CardRank)j, (CardSuit)i));
                    }
                }
                Shuffle();
            }
        }

        public Deck(IEnumerable<Card> cardsForDeck) :
            this()
        {
            cards.AddRange(cardsForDeck);
            Shuffle();
        }
        #endregion

        #region CARDS_MANIPULATION
        public void Shuffle()
        {
            Random rng = new Random();
            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < cards.Count; ++j)
                {
                    int swapIndex = rng.Next(cards.Count);
                    Card temp = cards[j];
                    cards[j] = cards[swapIndex];
                    cards[swapIndex] = temp;
                }
            }

            Shuffled?.Invoke(this, EventArgs.Empty);
        }
        
        public void PopTopCard()
        {
            cards.RemoveAt(cards.Count - 1);
        }

        public void PopCardsFromTop(int numOfCards)
        {
            cards.RemoveRange(cards.Count - numOfCards, numOfCards);
        }

        public Card TakeTopCard()
        {
            Card ret = TopCard;
            PopTopCard();
            return ret;
        }

        public Card[] TakeCardsFromTop(int numOfCards)
        {
            Card[] cardsTaken = new Card[numOfCards];
            cards.CopyTo(cards.Count - numOfCards, cardsTaken, 0, numOfCards);
            PopCardsFromTop(numOfCards);
            return cardsTaken;
        }

        public void PopBottomCard()
        {
            cards.RemoveAt(0);
        }

        public void PopCardsFromBottom(int numOfCards)
        {
            cards.RemoveRange(0, numOfCards);
        }

        public Card TakeBottomCard()
        {
            Card ret = BottomCard;
            PopBottomCard();
            return ret;
        }

        public Card[] TakeCardsFromBottom(int numOfCards)
        {
            Card[] cardsTaken = new Card[numOfCards];
            cards.CopyTo(0, cardsTaken, 0, numOfCards);
            PopCardsFromBottom(numOfCards);
            return cardsTaken;
        }

        public void MoveTopToBottom()
        {
            Card top = TakeTopCard();
            cards.Insert(0, top);
        }

        public void MoveBottomToTop()
        {
            Card bottom = TakeBottomCard();
            cards.Add(bottom);
        }

        public void PushCardsOnTop(Card card)
        {
            cards.Add(card);
        }

        public void PushCardsOnTop(Card[] cards)
        {
            this.cards.AddRange(cards);
        }

        public void PushCardsToBottom(Card card)
        {
            cards.Insert(0, card);
        }

        public void PushCardsToBottom(Card[] cards)
        {
            this.cards.InsertRange(0, cards);
        }
        #endregion

        #region PROPERTIES
        public bool Empty
        {
            get
            {
                return cards.Count == 0;
            }
        }

        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }

        public Card TopCard
        {
            get
            {
                return cards.Last();
            }
        }

        public Card BottomCard
        {
            get
            {
                return cards.First();
            }
        }
        #endregion
    }
}
