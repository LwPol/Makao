using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Makao
{
    public class GameLoop
    {
        private bool running;

        private GameState state;

        public GameLoop(GameState state)
        {
            this.state = state;
        }

        public void BeginGame()
        {
            Thread gameLoop = new Thread(new ThreadStart(RunLoop));
            running = true;
            gameLoop.IsBackground = true;
            gameLoop.Start();
        }

        private void RunLoop()
        {
            Player current = state.CurrentPlayer;

            while (running && current != null)
            {
                current.MakeMoveIfPossible();

                current = state.MoveToNextPlayer();
            }
        }

        public bool Running
        {
            get
            {
                lock (this)
                {
                    return running;
                }
            }
            set
            {
                lock (this)
                {
                    running = value;
                }
            }
        }
    }
}
