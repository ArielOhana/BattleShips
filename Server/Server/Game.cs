using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Game
    {
        private ClientHandler player1;
        private ClientHandler player2;
        private int[,] Map;
        private int BoardWide;
        private int BoardHigh;
        public Game(ClientHandler player1, ClientHandler player2)
        {
            this.BoardHigh = 20;
            this.BoardWide = 20;
            this.player1 = player1;
            this.player2 = player2;
            Map = new int[BoardWide, BoardHigh];
        }
   
    }
}
