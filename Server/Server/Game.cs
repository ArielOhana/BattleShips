using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{ /// <summary>
/// This class responsible of setting up the game, this one recieves the players, starts the game and sets up the board.
/// after setting up the board the main responsibility turns to check if there are hits on and board and updates them
/// </summary>
    class Game
    {
        private int player1;
        private int player2;
        private int[,] Map;
        private int BoardWide;
        private int BoardHigh;
        private int CountMapSets;
        private int GameFinished; // 0 no, 1 player 1 won, 2 player 2 won.
        /// <summary>
        /// game builder - sets up the game and the board
        /// </summary>
        /// <param name="player1"> the first player ID (ClientSpecificNumber)</param>
        /// <param name="player2">the second player ID (ClientSpecificNumber)</param>
        public Game(int player1, int player2)
        {
            this.GameFinished = 0;
            this.BoardHigh = 20;
            this.BoardWide = 20;
            this.Player1 = player1;
            this.Player2 = player2;
            this.CountMapSets = 0;
            Map = new int[BoardWide, BoardHigh];
            for(int i = 0; i < BoardWide; i++)
            {
                for (int j = 0; j < BoardHigh; j++)
                {
                    Map[i, j] = 0;
                }
            }
        }
        /// <summary>
        /// Sets up the map by recieving string of 1 side each time.
        /// </summary>
        /// <param name="Givenmap"> the given map</param>
        public void SetUpMap(string Givenmap) 
        {
            Givenmap = Givenmap.Replace("\n", "");
            for(int i = 0; i < BoardWide; i++)
            {
                for(int j = 0; j < BoardHigh; j++)
                {
                    if(int.Parse(Givenmap.Substring(i + (20*j),1)) == 1)
                    {
                        Map[j, i] = 1;    
                    }
                    
                }
            }
            CountMapSets++;
        }
        public int Player1
        {
            get
            {
                return player1;
            }

            set
            {
                player1 = value;
            }
        }

        public int Player2
        {
            get
            {
                return player2;
            }

            set
            {
                player2 = value;
            }
        }

        public int CountMapSets1
        {
            get
            {
                return CountMapSets;
            }

            set
            {
                CountMapSets = value;
            }
        }

        public int GameFinished1
        {
            get
            {
                return GameFinished;
            }

            set
            {
                GameFinished = value;
            }
        }

        public int[,] GetMap()
        {
            return Map;
        }
        /// <summary>
        /// Sets the map on the col and row to zero and checks if it hit something.
        /// </summary>
        /// <param name="col"> the column to hit</param>
        /// <param name="row"> the row to hit</param>
        /// <returns></returns>
        public bool HitMap(int col, int row) 
        {
            bool toret = false;
            if(Map[col,row] == 1) // if there is a ship
            {
                toret = true;   
            }
            Map[col, row] = 0;
            return toret;
        }

    }
}
