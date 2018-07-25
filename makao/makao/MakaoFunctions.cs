using System;
using System.Collections.Generic;

namespace Makao
{
    public static class MakaoFunctions
    {
        public static bool AreEqualInRankOrSuit(Card lhs, Card rhs)
        {
            return lhs.Rank == rhs.Rank || lhs.Suit == rhs.Suit;
        }

        public static bool CanBeJackDemanded(Card card)
        {
            return card.Rank >= CardRank.Five && card.Rank <= CardRank.Ten;
        }

        public static bool IsAggressiveCard(Card card)
        {
            if (card.Rank < CardRank.Four)
                return true;
            if (card.Rank == CardRank.King)
            {
                if (card.Suit == CardSuit.Pike || card.Suit == CardSuit.Heart)
                    return true;
            }

            return false;
        }

        public static bool IsAggressiveKing(Card card)
        {
            return card.Rank == CardRank.King && (card.Suit == CardSuit.Pike || card.Suit == CardSuit.Heart);
        }

        public static bool IsFunctionalCard(Card card)
        {
            if (card.Rank > CardRank.Four && card.Rank < CardRank.Jack)
                return false;
            if (card.Rank == CardRank.King)
            {
                if (card.Suit == CardSuit.Clover || card.Suit == CardSuit.Tile)
                    return false;
            }

            return true;
        }

        public static uint GetCardAggressiveness(Card card)
        {
            switch (card.Rank)
            {
                case CardRank.Two:
                    return 2;
                case CardRank.Three:
                    return 3;
                case CardRank.King:
                    if (card.Suit == CardSuit.Pike || card.Suit == CardSuit.Heart)
                        return 5;
                    else
                        throw new ArgumentException("Card is not aggressive", "card");
                default:
                    throw new ArgumentException("Card is not aggressive", "card");
            }
        }
    }
}
