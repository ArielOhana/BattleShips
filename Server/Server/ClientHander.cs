using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientHandler
    {
        /// <summary>
        /// BIGGEST CLASS IN THE PROJECT!
        /// Client handler handles everything the server recieves from the client and sends everything the server sends to the clients
        /// Client handler handles two scipts - the player script and the admin script
        /// </summary>
        public static List<ClientHandler> clntList = new List<ClientHandler>(); // static list of the clienthandlers
        public static List<ClientHandler> gamers = new List<ClientHandler>(); //static list of the players waiting for game
        private TcpClient clnt;
        private NetworkStream stream;
        private byte[] data;
        private int ClientSpecificNumber; // the id of the player
        private bool findgame = false;
        private static List<User> users = new List<User>(); // static list of users 
        public static int Userslogged = 0;
        private static List<string> StopPreparing = new List<string>(); // static list of stop preparing players
        private static List<Game> Games = new List<Game>(); // static list of games
        private bool intransmission = true;
        private bool confirmedplayer = false; // checks up if the player is a confirmed player to be able to run protocol.
        private bool confirmedadmin = false; // checks up if the player is an admin or not, to avoid him exacute admin's commands.
        public ClientHandler(TcpClient clnt) // BUILDER - creates the stream and starts a loop on the HandleRead function
        {
            this.Clnt = clnt;
            this.stream = clnt.GetStream();
            this.Data = new byte[1024];
            this.ClientSpecificNumber = Userslogged;
            stream.BeginRead(data, 0, data.Length, HandleRead, null);
        }
        /// <summary>
        ///Biggest class in the Project - this class stands for recieving a Code of the Protocol 
        ///checks if the client has access to that code and starts another function / commands in target to send back the response of the server.
        /// </summary>
        private void HandleRead(IAsyncResult ar) 
        {
            try
            {
                if (intransmission) // if the player is still logged.
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
                                confirmedplayer = true; // sets the player confirmed to be able to run game codes (STS, PLY, WIN for example..)
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
                                confirmedplayer = true; // sets the player confirmed to be able to run game codes (STS, PLY, WIN for example..)
                            }
                            else
                                Send("Can't Register");
                        }
                        if (PROTOCOL == "LAG") // Log Admin user - Administrator script
                        {
                            if (LOGADMINUSER(recived))//check if username and passowrd are right
                            {
                                Send("Logged in, ID: " + Userslogged);
                                Userslogged++;
                                confirmedadmin = true; // sets the player confirmed to be able to run admin codes
                            }
                            else
                                Send("Failed");

                        }
                        if (PROTOCOL == "OUT")
                        {
                            intransmission = false;
                        }
                        if (confirmedadmin) // ADMIN COMMANDS ONLY - ONLY ADMINS CAN REACH TO THESE CODES!
                        {
                            if (PROTOCOL == "AFP") // Admin Protocol - Find Player - Attempts to find a player and if he exists returns his details.
                            {
                                Send(AdminFindPlayer(recived)); // if the username is in the database it sends his status, else sends a message that we couldn't find him in our database.
                            }
                            if (PROTOCOL == "SET") // Sets the new stats
                            {
                                Send(AdminSetStats(recived));
                            }
                            if(PROTOCOL == "GDB") //Get DataBase
                            {
                                Send(File.ReadAllText("database.txt"));
                            }                            
                            if (PROTOCOL =="EDB") // Edit Database
                            {
                                EditDataBase(recived);
                            }
                            if(PROTOCOL == "GLG")// Admin Get log.
                            {
                                Send(File.ReadAllText("log.txt"));
                            }
                            if(PROTOCOL =="GCC") // Admin Get Clients Chat 
                            {
                                if (File.ReadAllText("chat.txt").Length > 0) // checks if the file is empty to know what to send.
                                {
                                    Send(File.ReadAllText("chat.txt"));
                                }
                                else
                                {
                                    Send("Chat file is empty");
                                }
                            }
                            if(PROTOCOL == "DCC") // Admin Delete Client Chat.
                            {
                                File.WriteAllText("chat.txt", ""); // Sets the Chat.txt empty 
                            }
                        }
                        if (confirmedplayer) // ALL THE FUNCTIONS FROM HERE ARE FOR PLAYERS ONLY!!!
                        {
                            if(PROTOCOL == "CHT")
                            {
                                WriteToChat(recived);
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
                                        string towrite = "ID: " + ClientSpecificNumber + "and ID: " + gamers[0].ClientSpecificNumber + " starts a game!";
                                        Console.WriteLine(towrite);
                                        Send("GSN2" + gamers[0].ClientSpecificNumber);
                                        gamers[0].Send("GSN1" + ClientSpecificNumber);
                                        gamers.Clear();
                                    }
                                }
                            }
                            if (PROTOCOL == "STS")//Protocol Stats! Sends stats to the server.
                            {
                                Send(SetUpStats());
                                string towrite = "Sending stats to ID: " + ClientSpecificNumber + ", " + SetUpStats();
                                Console.WriteLine(towrite);
                          
                            }
                            if (PROTOCOL == "LGF")// LGF = Leave Game Finder, Play screen.
                            {
                                this.findgame = false;
                                string towrite = "ID: " + ClientSpecificNumber + " Leaving Play Screen";
                                Console.WriteLine(towrite);
                              
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
                                    string towrite = "ID: " + ClientSpecificNumber + " And ID: " + recived.Substring(3, 1) + " Stopped preparing and now starting to play";
                                    Console.WriteLine(towrite);
                                    Games.Add(new Game(ClientSpecificNumber, int.Parse(recived.Substring(3, 1))));
                                    Send("Stop waiting: -1,-1"); // Gives the first turn to the sender
                                }
                                else
                                {
                                    string towrite = "ID: " + ClientSpecificNumber + "Stopped preparing and waiting for ID: " + recived.Substring(3, 1) + " to finish preparing";
                                    Console.WriteLine(towrite);
                                   
                                }
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
                            if (PROTOCOL == "HIT")// Game Protocol, gets a point and check if it hits the enemy's battle ship, uses field of both players in game.
                            {
                                for (int i = 0; i < Games.Count; i++)
                                {
                                    int col;
                                    int row;
                                    if (Games[i].Player1 == ClientSpecificNumber)
                                    {
                                        msg = msg.Remove(0, 5);//The protocol.
                                        int firstnum = msg.IndexOf(","); // finds the point to split.
                                        col = int.Parse(msg.Substring(0, firstnum)); // The column.
                                        row = int.Parse(msg.Substring(firstnum + 1));// The row.
                                        if (Games[i].HitMap(col, row)) // Starts the HITMAP function by sending them the col and the row I recieved.
                                            Send("C"); // incase Caught one of the enemy's ships.
                                        else
                                            Send("M"); //incase missed.

                                        clntList[Games[i].Player2].Send("Stop waiting: " + col + "," + row);//Sends the other player that's his turn to play
                                    }
                                    if (Games[i].Player2 == ClientSpecificNumber)
                                    {
                                        msg = msg.Remove(0, 5);//The protocol.
                                        int firstnum = msg.IndexOf(","); // finds the point to split.
                                        col = int.Parse(msg.Substring(0, firstnum)); // The column.
                                        row = int.Parse(msg.Substring(firstnum + 1));// The row.
                                        if (Games[i].HitMap(col, row)) // Starts the HITMAP function by sending them the col and the row I recieved.
                                            Send("C"); // incase Caught one of the enemy's ships.
                                        else
                                            Send("M"); //incase missed.

                                        clntList[Games[i].Player1].Send("Stop waiting: " + col + "," + row);//Sends the other player that's his turn to play
                                    }
                                }
                            }
                            if (PROTOCOL == "WIN")// Game Protocol, updates win!
                            {
                                users[ClientSpecificNumber].AddWin(); // updates the wins
                                string towrite = "ID: " + ClientSpecificNumber + " won and now he has " + users[ClientSpecificNumber].GetW() + " wins";
                                Console.WriteLine(towrite);
                                //using (StreamWriter sw = new StreamWriter("log.txt", true)) // Saves "towrite" on the log file.
                                //{
                                //    sw.WriteLine(towrite);
                                //}
                                for (int i = 0; i < Games.Count; i++)
                                {
                                    if (Games[i].Player1 == ClientSpecificNumber)
                                    {
                                        Games[i].GameFinished1 = 1;//Sets the winner player 1.
                                        users[Games[i].Player2].AddLose(); //Adds a lose for the enemy who lose the game 
                                    }
                                    if (Games[i].Player2 == ClientSpecificNumber)
                                    {
                                        Games[i].GameFinished1 = 2;//Sets the winner player 2.
                                        users[Games[i].Player1].AddLose(); //Adds a lose for the enemy who lose the game 
                                    }
                                }
                            }
                            if (PROTOCOL == "CHW")// Game Protocol, Check winner
                            {
                                int gameid = -1; 
                                bool toret = false;
                                for (int i = 0; i < Games.Count; i++)
                                {
                                    if (Games[i].Player1 == ClientSpecificNumber)
                                    {
                                        Thread.Sleep(500);// To avoid running before it should
                                        if (Games[i].GameFinished1 == 2)//Checks if his enemy won
                                        {
                                            toret = true;
                                            gameid = i; // sets the game id (the fact it gets here confirms that)
                                        }
                                    }
                                    if (Games[i].Player2 == ClientSpecificNumber)
                                    {
                                        Thread.Sleep(500);// To avoid running before it should
                                        if (Games[i].GameFinished1 == 1)//Checks if his enemy won
                                        {
                                            toret = true;
                                            gameid = i; // sets the game id (the fact it gets here confirms that)
                                        }
                                    }
                                }
                                if (toret)
                                {
                                    if(gameid != -1)//making sure game id updated!
                                    {
                                        Games.Remove(Games[gameid]);
                                    }
                                    Send("Enemy won");
                                    string towrite = "ID: " + ClientSpecificNumber + " lose and now he has " + users[ClientSpecificNumber].GetL() + " loses";
                                    Console.WriteLine(towrite);
                                }
                                else
                                {
                                    Send("Game on");
                                }
                            }
                        }
                    }
                    if (intransmission)
                    {
                        stream.BeginRead(data, 0, data.Length, HandleRead, null);
                    }
                    else
                    {
                        ClientDisconnected();
                    }
                }
            }
            catch (Exception Exp) // incase of exception throws the client and writes the exception.
            {
                Console.WriteLine(Exp);
                ClientDisconnected();
            }
        }
        /// <summary>
        /// Sets up the stats to send
        /// </summary>
        /// <returns>Returns the stats of the player</returns>
        public string SetUpStats()
        {
            string tosend = "";
            tosend += users[ClientSpecificNumber].GetR() + "/"+users[ClientSpecificNumber].GetW() + "/"+ users[ClientSpecificNumber].GetL();
            return tosend;
        }
        /// <summary>
        ///  Send function, the function that sends the data to the client.
        /// </summary>
        /// <param name="msg">Sends "msg" to the client.</param>
        private void Send(string msg)
        {
            try
            {
                byte[] toSend = Encoding.UTF8.GetBytes(msg);
                stream.Write(toSend, 0, toSend.Length);
                stream.Flush(); // cleans the stream.
                string towrite = String.Format("{0}: {1}", ((IPEndPoint)clnt.Client.RemoteEndPoint).Address.ToString(), msg);
                Console.WriteLine(towrite);
             
            }
            catch(Exception)
            {
                ClientDisconnected();
            }
        }
        /// <summary>
        ///  log user, the function that signing in the users.
        /// </summary>
        /// <param name="msg">"msg" is the string of the username and the password.</param>
        /// <returns> Returns a boolean value if the client logged or not.</returns>
        private bool LOGUSER(string msg) 
        {
            bool tosend = false;
            string[] CodeLines = File.ReadAllLines("database.txt");
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
                            string towrite = givenusername + "'s stats: \n ID: "+Userslogged + "\n Admin Rank: " + users[Userslogged].GetR()+ "\n Wins: "+users[Userslogged].GetW() +"\n Loses: " + users[Userslogged].GetL();
                        Console.WriteLine(towrite);
                     
                    }
                }
            }
            return tosend;
        }

        /// <summary>
        /// Register user, the function that signing up the new users.
        /// </summary>
        /// <param name="msg">"msg" is a string of the username and password connected</param>
        /// <returns> returns a boolean value if the client can register with the given username.</returns>
        public bool REGUSER(string msg)
        {
            string[] CodeLines = File.ReadAllLines("database.txt");
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
                using (StreamWriter sw = new StreamWriter("database.txt", true)) // making new user
                {
                    sw.WriteLine(givenusername);
                    sw.WriteLine(givenpassword);
                    sw.WriteLine("0/0/0");
                }
                users.Add(new User(givenusername, CodeLines.Length, Userslogged));

                string towrite = givenusername + "'s stats: \n ID: " + Userslogged + "\n Admin Rank: " + users[Userslogged].GetR() + "\n Wins: " + users[Userslogged].GetW() + "\n Loses: " + users[Userslogged].GetL();
                Console.WriteLine(towrite);              
            }
            return searching;
        }
        /// <summary>
        /// Admin log in system, checks if the user is an admin and returns a boolean value if yes or no.
        /// </summary>
        /// <param name="msg">"msg" is the string of the username and the password.</param>
        /// <returns> Returns a boolean value if the admin logged or not.</returns>
        private bool LOGADMINUSER(string msg)
        {
            bool tosend = false;
            string[] CodeLines = File.ReadAllLines("database.txt");
            msg = msg.Remove(0, 6);
            string givenusername = msg.Substring(0, msg.IndexOf("\n"));
            string givenpassword = msg.Substring(givenusername.Length + 1, msg.Length - givenusername.Length - 2);
            bool searching = true;
            for (int i = 0; i < CodeLines.Length - 1 && searching; i = i + 3)
            {
                if (givenusername == CodeLines[i]) // checks if the username exists.
                {
                    if (CodeLines[i + 1] == givenpassword)// if the username exists, checks his password.
                    {
                        users.Add(new User(givenusername, i, Userslogged)); // Adds a user to check if he is an admin
                        if (users[Userslogged].GetR() != 0) // confirms that the player isn't a normal user. checks if he is an admin
                        {
                            tosend = true;
                            searching = false;
                            string towrite = givenusername + "'s stats: \n ID: " + Userslogged + "\n Admin Rank: " + users[Userslogged].GetR() + "\n Wins: " + users[Userslogged].GetW() + "\n Loses: " + users[Userslogged].GetL();
                            Console.WriteLine(towrite);                          
                        }
                        else
                        {
                            users.Remove(users[Userslogged]);// If the player isn't an admin, removes his to avoid doubles.. 
                        }
                    }
                }
            }
            return tosend;
        }
        /// <summary>
        ///  ADMIN COMMAND: runs a search on a player in the database :
        ///  if found sends his details but and if not send that the server couldn't find the username on the database
        /// </summary>
        /// <param name="msg"> "msg" is the username the admin search for.</param>
        /// <returns>if the username is exists in database it sends his details(rank/wins/loses) else it will send that he is not in the system</returns>
        private string AdminFindPlayer(string msg)
        {
            string towrite = "";
            string[] CodeLines = File.ReadAllLines("database.txt");
            msg = msg.Remove(0, 6);
            string givenusername = msg;
            bool searching = true;
            for (int i = 0; i < CodeLines.Length - 1 && searching; i = i + 3)
            {
                if (givenusername == CodeLines[i]) // checks if the username exists.
                {
                    users.Add(new User(givenusername, i, Userslogged)); // Adds a user to check if he is an admin
                    towrite = "Player Found: "+ givenusername + "'s stats: \n ID: " + Userslogged + "\n Admin Rank: " + users[Userslogged].GetR() + "\n Wins: " + users[Userslogged].GetW() + "\n Loses: " + users[Userslogged].GetL();
                    Console.WriteLine(towrite);
               
                    string toret = users[Userslogged].GetR() + "/" + users[Userslogged].GetW() + "/" + users[Userslogged].GetL();
                    users.Remove(users[Userslogged]);
                    return toret;
                }
            }
           
                towrite = "Couldn't find " + givenusername + " in our database.";
                Console.WriteLine(towrite);           
            return towrite;
        }
        /// <summary>
        /// ADMIN COMMAND: the admin sets new rank/wins/loses to the user and sets Done.
        /// if the username isn't found sends Couldn't find the user, if there was a problem except of finding the username 
        /// ((like wrong type of var (string and not int for example))) the server sends there was a problem.
        /// </summary>
        /// <param name="msg"> "msg" is the given details in the following form (username - rank - wins - loses) username stands
        /// find the user and the rank wins and loses are to edit in his profile.</param>
        /// <returns> returns if the process successed or not.</returns>
        private string AdminSetStats(string msg) 
        {
            try
            {
                string[] CodeLines = File.ReadAllLines("database.txt");
                msg = msg.Remove(0, 6);
                string givenusername = msg.Substring(0, msg.IndexOf("\n")); // cuts the username.
                msg = msg.Remove(0, givenusername.Length);
                int rank = int.Parse(msg.Substring(0, msg.IndexOf("/"))); // cuts the rank
                int wins = int.Parse(msg.Substring(msg.IndexOf("/") + 1, msg.LastIndexOf("/") - (msg.IndexOf("/") + 1))); // cuts the wins
                int loses = int.Parse(msg.Substring(msg.LastIndexOf("/") + 1)); //cuts the loses
                bool searching = true;
                for (int i = 0; i < CodeLines.Length - 1 && searching; i = i + 3)
                {
                    if (givenusername == CodeLines[i]) // checks if the username exists.
                    {
                        users.Add(new User(givenusername, i, Userslogged)); // Adds a user to check if he is an admin
                        users[Userslogged].SetRank(rank);
                        users[Userslogged].SetWin(wins);
                        users[Userslogged].SetLose(loses);
                        users.Remove(users[Userslogged]); // to avoid duplicates.
                        return "Done";
                    }
                }
                return "Couldn't find the user";
            }
            catch
            {
                return "There was a problem";
            }
        }
        /// <summary>
        ///  player command - the server recieves a message from player and sets it at chat.txt file.
        /// </summary>
        /// <param name="msg">the message to write.</param>
        public void WriteToChat(string msg) 
        {
            msg = msg.Remove(0, 5);
            using (StreamWriter sw = new StreamWriter("chat.txt", true)) // Saves "towrite" on the chat file.
            {
                sw.WriteLine(msg);
            }
            string towrite = "Wrote to chat.txt: " + msg;
            Console.WriteLine(towrite);
       
        }
        /// <summary>
        /// edits the database by recieving new one.
        /// </summary>
        /// <param name="msg"> the edited database.</param>
        public void EditDataBase(string msg)
        {
            msg = msg.Remove(0, 6);
            File.WriteAllText("database.txt", msg);
        }
        /// <summary>
        /// Stop Preparing function, this function checks if both of the players are ready to play, 
        /// if they are, it sends okay and starting a game on Games list
        /// </summary>
        /// <param name="recived"> The form of(the enemy's ID) (-) (this client ID) </param>
        /// <returns> returns if the game started</returns>
        public bool SPPFunction(string recived) 
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
            {
                string towrite = "ID: " + ClientSpecificNumber + " Already started the SPP protocol.";
                Console.WriteLine(towrite);
           
            }
            else
                StopPreparing.Add(msg);
            return false;
        }
        /// <summary>
        /// Client Disconnected, disconnecting from the client and it's stream
        /// </summary>
        public void ClientDisconnected()
        {
            string towrite = "ID: " + ClientSpecificNumber + " has disconnected!";
            Console.WriteLine(towrite);     
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