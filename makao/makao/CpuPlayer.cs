using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Makao
{
    public delegate void PushCardsDelegate(Deck deck, int[] indices);
    public delegate void TransferCardsDelegate(Player player);
    public delegate void WaitTurnsDelegate(uint turnsToWait);

    public abstract class CpuPlayer : Player
    {
        private GameState state;

        public CpuPlayer(string name, GameState state) :
            base(name)
        {
            this.state = state;
        }

        protected void Delay(int milliseconds)
        {
            if (!state.Skip)
                Thread.Sleep(milliseconds);
        }

        protected GameState State
        {
            get
            {
                return state;
            }
        }
    }
}
