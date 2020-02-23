using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Game
    {
        private int player1;
        private int player2;
        private int[,] Map;
        private int BoardWide;
        private int BoardHigh;
        private int CountMapSets;

        public Game(int player1, int player2)
        {
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
        public void SetUpMap(string Givenmap) // Sets up the map by recieving string of 1 side each time.
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

        public int[,] GetMap()
        {
            return Map;
        }
        public bool HitMap(int col, int row) //Sets the map on the col and row to zero and checks if it hit something.
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
