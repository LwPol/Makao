using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Makao
{
    class EasyPlayer : CpuPlayer
    {
        private Random rng;

        public EasyPlayer(string name, GameState gameState) :
            base(name, gameState)
        {
            rng = new Random(DateTime.Now.Millisecond ^ GetHashCode() ^ name.GetHashCode());
        }

        public override bool DecideIfPushFirstMatch(Card firstMatch)
        {
            return true;
        }

        public override CardSuit? GetAceDemand()
        {
            HashSet<CardSuit> mySuits = new HashSet<CardSuit>();
            foreach (Card c in Cards)
            {
                mySuits.Add(c.Suit);
            }

            int randomNum = rng.Next(mySuits.Count);
            int i = 0;
            foreach (CardSuit cs in mySuits)
            {
                if (i++ == randomNum)
                {
                    return cs;
                }
            }

            return null;
        }

        public override CardRank? GetJackDemand()
        {
            HashSet<CardRank> myRanks = new HashSet<CardRank>();
            foreach (Card c in Cards)
            {
                if (MakaoFunctions.CanBeJackDemanded(c))
                {
                    myRanks.Add(c.Rank);
                }
            }

            int randomNum = rng.Next(myRanks.Count);
            int i = 0;
            foreach (CardRank cr in myRanks)
            {
                if (i++ == randomNum)
                {
                    return cr;
                }
            }

            return null;
        }

        public override void MakeMove()
        {
            Delay(3000);

            List<int> possibleCards = new List<int>();
            for (int i = 0; i < Cards.Count; ++i)
            {
                if (State.ValidateCardForPush(Cards[i]))
                {
                    possibleCards.Add(i);
                }
            }

            if (possibleCards.Count > 0)
            {
                int[] cardToPush = { possibleCards[rng.Next(possibleCards.Count)] };
                AppControl.MainWindow.Invoke(new PushCardsDelegate(PushCardsToDeck), State.MakaoStack, cardToPush);
            }
            else if (State.MakaoStack.TopCard.Rank == CardRank.Four && State.TopCardActive)
            {
                AppControl.MainWindow.Invoke(new WaitTurnsDelegate(WaitTurns), State.TurnsToWait);
            }
            else
            {
                AppControl.MainWindow.Invoke(new TransferCardsDelegate(State.TransferCardsToPlayer), this);
            }
        }
    }
}
