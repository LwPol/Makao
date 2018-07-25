using System;
using System.ComponentModel;

namespace Makao
{
    public struct Card
    {
        private CardSuit suit;
        private CardRank rank;
        
        public Card(CardRank rank, CardSuit suit)
        {
            if (rank < CardRank.Two || rank > CardRank.Ace)
                throw new InvalidEnumArgumentException("rank", (int)rank, typeof(CardRank));
            this.rank = rank;

            if (suit < CardSuit.Pike || suit > CardSuit.Heart)
                throw new InvalidEnumArgumentException("suit", (int)suit, typeof(CardSuit));
            this.suit = suit;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", RankString, SuitString);
        }

        public static string SuitAsString(CardSuit suit)
        {
            string[] suits = new string[] { "pik", "trefl", "karo", "kier" };
            return suits[(int)suit];
        }

        public static string RankAsString(CardRank rank)
        {
            string[] ranks = new string[]
            {
                "dwójka",
                "trójka",
                "czwórka",
                "piątka",
                "szóstka",
                "siódemka",
                "ósemka",
                "dziewiątka",
                "dziesiątka",
                "jopek",
                "dama",
                "król",
                "as"
            };
            return ranks[(int)rank];
        }

        public CardSuit Suit
        {
            get
            {
                return suit;
            }
            set
            {
                if (value < CardSuit.Pike || value > CardSuit.Heart)
                    throw new InvalidEnumArgumentException("Suit:value", (int)value, typeof(CardSuit));
                suit = value;
            }
        }

        public CardRank Rank
        {
            get
            {
                return rank;
            }
            set
            {
                if (value < CardRank.Two || value > CardRank.Ace)
                    throw new InvalidEnumArgumentException("Rank:value", (int)value, typeof(CardRank));
                rank = value;
            }
        }

        public string SuitString
        {
            get
            {
                return SuitAsString(suit);
            }
        }

        public string RankString
        {
            get
            {
                return RankAsString(rank);
            }
        }

        public static bool operator==(Card lhs, Card rhs)
        {
            return Equals(lhs, rhs);
        }
        public static bool operator!=(Card lhs, Card rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return (int)suit * 13 + (int)rank;
        }
    }

    public enum CardSuit
    {
        Pike,
        Clover,
        Tile,
        Heart
    }

    public enum CardRank
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
}
