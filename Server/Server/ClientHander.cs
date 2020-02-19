using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Server
{
    public class ClientHandler
    {
        public static List<ClientHandler> clntList = new List<ClientHandler>();
        public static List<ClientHandler> gamers = new List<ClientHandler>();
        private TcpClient clnt;
        private NetworkStream stream;
        private byte[] data;
        private int ClientSpecificNumber;
        private bool findgame = false;
        private static List<User> users = new List<User>();
        public static int Userslogged = 0;
        private static List<string> StopPreparing = new List<string>();
        private static List<Game> Games = new List<Game>();
        private bool intransmission = true;
        public ClientHandler(TcpClient clnt)
        {
            this.Clnt = clnt;
            this.stream = clnt.GetStream();
            this.Data = new byte[1024];
            this.ClientSpecificNumber = Userslogged;
            stream.BeginRead(data, 0, data.Length, HandleRead, null);
        }

        private void HandleRead(IAsyncResult ar)
        {
            try
            {
                if (intransmission)
                {
                    string recived = Encoding.UTF8.GetString(data, 0, data.Length);
                    recived = recived.Replace("\0", "");
                    string msg = recived;
                    recived = String.Format("{0}: {1}", ((IPEndPoint)clnt.Client.RemoteEndPoint).Address.ToString(), recived);

                    Console.WriteLine(recived);
                    recived = msg;
                    Array.Clear(data, 0, data.Length);
                    if (recived.Length >= 3) // avoids exp of "string PROTOCOL = recived.Substring(0, 3);" out of range
                    {
                        string PROTOCOL = recived.Substring(0, 3);
                        if (PROTOCOL == "LOG")//Log in, signs in users by checking their username and password in the log file
                        {
                            if (LOGUSER(recived))//check if username and passowrd are right
                            {
                                Send("Logged in, ID: " + Userslogged);
                                Userslogged++;
                            }
                            else
                                Send("Failed");

                        }
                        if (PROTOCOL == "REG")//registering new user if possible.
                        {
                            if (REGUSER(recived))
                            {
                                Send("Registered");
                                Userslogged++;
                            }
                            else
                                Send("Can't Register");
                        }
                        if (PROTOCOL == "PLY")//attempts to find another client to play with 
                        {
                            bool add = true;
                            for (int i = 0; i < gamers.Count; i++)
                            {
                                if (clntList[ClientSpecificNumber] == gamers[i])
                                    add = false;
                            }
                            if (add)
                                gamers.Add(clntList[ClientSpecificNumber]);
                            {
                                if (gamers.Count == 2)
                                {
                                    // Game(ClientSpecificNumber, clntList[i].ClientSpecificNumber);
                                    Console.WriteLine("ID: " + ClientSpecificNumber + "and ID: " + gamers[0].ClientSpecificNumber + " starts a game!");
                                    Send("GSN2" + gamers[0].ClientSpecificNumber);
                                    gamers[0].Send("GSN1" + ClientSpecificNumber);
                                    gamers.Clear();
                                }
                            }
                        }
                        if (PROTOCOL == "STS")//Protocol Stats! Sends stats to the server.
                        {
                            Send(SetUpStats());
                            Console.WriteLine("Sending stats to ID: " + ClientSpecificNumber + ", " + SetUpStats());
                        }
                        if (PROTOCOL == "LGF")// LGF = Leave Game Finder, Play screen. REQUIRES FIX
                        {
                            this.findgame = false;
                            Console.WriteLine("ID: " + ClientSpecificNumber + " Leaving Play Screen");
                        }
                        if (PROTOCOL == "GEN") //GAME PROTOCOL - Get Enemy Name!
                        {
                            int enemyidGEN = int.Parse(recived.Substring(3, 1));
                            Send(users[enemyidGEN].GetUN());
                        }
                        if (PROTOCOL == "SPP")// STOP PREPARING PROTOCOL = Checks if both of the praticipators ready and adds a game to the Games list if both stopped preparing.
                        {
                            if (SPPFunction(recived))
                            {
                                Console.WriteLine("ID: " + ClientSpecificNumber + " And ID: " + recived.Substring(3, 1) + " Stopped preparing and now starting to play");
                                Games.Add(new Game(ClientSpecificNumber, int.Parse(recived.Substring(3, 1))));
                            }
                            else
                                Console.WriteLine("ID: " + ClientSpecificNumber + "Stopped preparing and waiting for ID: " + recived.Substring(3, 1) + " to finish preparing");
                        }
                        if (PROTOCOL == "FLD") // Get Field Protocol, sets up the game.
                        {
                            for (int i = 0; i < Games.Count; i++)
                            {
                                if (Games[i].Player1 == ClientSpecificNumber || Games[i].Player2 == ClientSpecificNumber)
                                {
                                    Games[i].SetUpMap(recived.Substring(4));
                                    if (Games[i].CountMapSets1 > 1)
                                    {
                                        string toshow = "";
                                        int[,] field = Games[i].GetMap();
                                        for (int m = 0; m < field.GetLength(1); m++)
                                        {
                                            for (int j = 0; j < field.GetLength(0); j++)
                                            {
                                                toshow += field[m, j];
                                            }
                                            toshow += "\n";
                                        }
                                        Console.WriteLine(toshow);
                                    }

                                }
                            }
                            
                        } // FLD end
                        if( PROTOCOL == "HIT")// Game Protocol, gets a point and check if it hits the enemy's battle ship, uses field of both players in game.
                     
                    }
                    if (intransmission)
                    {
                        stream.BeginRead(data, 0, data.Length, HandleRead, null);
                    }
                }
            }
            catch (Exception Exp)
            {
                Console.WriteLine(Exp);
                ClientDisconnected();
            }
        }
        public string SetUpStats()//Sets up the stats to send
        {
            string tosend = "";
            tosend += users[ClientSpecificNumber].GetR() + "/"+users[ClientSpecificNumber].GetW() + "/"+ users[ClientSpecificNumber].GetL();
            return tosend;
        }
      
        private void Send(string msg) // Send function, the function that sends the data to the client.
        {
            try
            {
                byte[] toSend = Encoding.UTF8.GetBytes(msg);
                stream.Write(toSend, 0, toSend.Length);
                stream.Flush();
                Console.WriteLine(String.Format("{0}: {1}", ((IPEndPoint)clnt.Client.RemoteEndPoint).Address.ToString(), msg));
            }
            catch(Exception)
            {
                ClientDisconnected();
            }
        }
        private bool LOGUSER(string msg) // log user, the function that signing in the users.
        {
            bool tosend = false;
            string[] CodeLines = File.ReadAllLines("log.txt");
            msg =msg.Remove(0, 6);
            
            string givenusername = msg.Substring(0, msg.IndexOf("\n"));
            string givenpassword = msg.Substring(givenusername.Length + 1, msg.Length- givenusername.Length-2);
           
            
            bool searching = true;
            for (int i = 0; i < CodeLines.Length - 1 && searching; i = i + 3)
            {
                if (givenusername == CodeLines[i])
                {
                    if (CodeLines[i + 1] == givenpassword)
                    {
                        
                            tosend = true;
                            searching = false;


                            users.Add(new User(givenusername, i, Userslogged));
                            
                            Console.WriteLine(givenusername + "'s stats: \n ID: "+Userslogged + "\n Rank: " + users[Userslogged].GetR()+ "\n Wins: "+users[Userslogged].GetW() +"\n Loses: " + users[Userslogged].GetL());
                        
                    }
                }
            }
            return tosend;
        }
        public bool REGUSER(string msg)// Register user, the function that signing up the new users.
        {
            string[] CodeLines = File.ReadAllLines("log.txt");

            msg = msg.Remove(0, 6);

            string givenusername = msg.Substring(0, msg.IndexOf("\n"));
            string givenpassword = msg.Substring(givenusername.Length + 1, msg.Length - givenusername.Length - 2);


            bool searching = true;
            for (int i = 0; i < CodeLines.Length - 1 && searching; i = i + 3)
            {
                if (givenusername == CodeLines[i])
                    searching = false; // user exists
            }
            if (searching)
            {

                using (StreamWriter sw = new StreamWriter("log.txt", true)) // making new user
                {
                    sw.WriteLine(givenusername);
                    sw.WriteLine(givenpassword);
                    sw.WriteLine("0/0/0");
                }
                users.Add(new User(givenusername, CodeLines.Length, Userslogged));
                Console.WriteLine(givenusername + "'s stats: \n ID: " + Userslogged + "\n Rank: " + users[Userslogged].GetR() + "\n Wins: " + users[Userslogged].GetW() + "\n Loses: " + users[Userslogged].GetL());
            }
            return searching;
        }
        public bool SPPFunction(string recived) // Stop Preparing function, this function checks if both of the players are ready to play, if they are, it sends okay and starting a game on Games list
        {
            string firstplayer = recived.Substring(3, 1);
            string msg = firstplayer;
            msg = msg + "-" + ClientSpecificNumber;
            if (StopPreparing.Contains(ClientSpecificNumber + "-" + firstplayer))
            {
                clntList[int.Parse(firstplayer)].Send("Start Playing");
                Send("Start Playing");
                StopPreparing.Remove(ClientSpecificNumber + "-" + firstplayer);
                return true;

            }
            else if (StopPreparing.Contains(msg))
                Console.WriteLine("ID: " + ClientSpecificNumber + " Already started the SPP protocol.");
            else
                StopPreparing.Add(msg);
            return false;
        }

        public void ClientDisconnected()//Client Disconnected, disconnecting from the client and it's stream
        {
            Console.WriteLine("ID: " + ClientSpecificNumber + " has disconnected!");
            clnt.Close();
            stream.Close();
        }
        
        public TcpClient Clnt
        {
            get
            {
                return clnt;
            }

            set
            {
                clnt = value;
            }
        }
        

        public byte[] Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        public bool Findgame
        {
            get
            {
                return findgame;
            }

            set
            {
                findgame = value;
            }
        }

        public int ClientSpecificNumber1
        {
            get
            {
                return ClientSpecificNumber;
            }

            set
            {
                ClientSpecificNumber = value;
            }
        }

    }
}