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
        private int BoardWidth;
        private int BoardHigth;
        public Game(ClientHandler player1, ClientHandler player2)
        {
            this.player1 = player1;
            this.player2 = player2;
            Map = new int[10, 10];
        }
   
    }
}
