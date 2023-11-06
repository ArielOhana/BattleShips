using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// MAIN WINDOW: This window have 6 pages!
    /// 1. LOG IN / REGISTER PAGE: This page takes care about the log in and register request - sends the log in / register details to the server for confirm
    /// 2. Main Page: 3 optional choices - PLAY INFO EXIT: PLAY means you attempts to start a game. INFO means you want to se
    /// </summary>
    public partial class MainWindow : Window
    {
        public ServerHandler Client;
        public Game game;
        private int usernumber;
        private int wins; // Player wins
        private int loses;// Player's loses
        private int rank;// Player's rank
        public int playernumber;//first/second
        public int enemyid;//server's ID
        public string myusername;// player's username
        public MainWindow()
        {
           
            InitializeComponent();
            Client = new ServerHandler();
        }
        /// <summary>
        /// updates stats into rank, wins and loses ints.
        /// </summary>
        public void GetStats()
        {
            Client.WriteThread("STS");
            string recieved = Client.ReadThread();
            int firstcut = recieved.IndexOf("/");
            int secondcut = recieved.LastIndexOf("/") - 1;
            this.rank = int.Parse(recieved.Substring(0, firstcut));
            this.wins = int.Parse(recieved.Substring(firstcut + 1, secondcut - firstcut));
            this.loses = int.Parse(recieved.Substring(secondcut + 2, recieved.Length - (2 + secondcut)));
            
        }
        /// <summary>
        ///Checking the username and password, if it's match to log.txt file it saves it on trueusername and sets the content in Check works, else sets failed.
        /// </summary>
        private void Continue_Click( object sender, RoutedEventArgs e)
        {
            string givenusername = Username.Text;
            string givenpassword = Password.Text;
            string recievefromserver;
            Client.WriteThread("LOG: \n" + givenusername + "\n" + givenpassword + "\n");
            recievefromserver = Client.ReadThread();
            if (recievefromserver.Contains("Logged in"))
            {
                usernumber = int.Parse(recievefromserver.Substring(15, recievefromserver.Length - 15));
                myusername = givenusername;
                Check.Content = "works";
                ShowMainScreen();
            }
            else
            {
                    Check.Content = "failed";
            }
        }
        /// <summary>
        /// Registers user by sending his name and password to the server and if there is no user with his username it sends "registered" but if there is it sends can't register.
        /// </summary>
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string givenusername = Username.Text;
            string givenpassword = Password.Text;

            Client.WriteThread("REG: \n" + givenusername + "\n" + givenpassword + "\n");
            if (Client.ReadThread() == "Registered")
            {
                Check.Content = "Registered";
                myusername = givenusername;
                ShowMainScreen();
            }
            else
            {
                Check.Content = "Can't Register";
            }
        }

        /// <summary>
        ///  starts the info page by switching visibilities
        /// </summary>
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            HelpPage.Visibility = Visibility.Visible;
            MainPage.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// sends a quit message to the server and closes the stream with the server as it closes the application.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Client.WriteThread("OUT");
            Client.Close();
            this.Close();// exit the game 
        }
        /// <summary>
        /// sends to the server a message that the client looks up for a player to play with.
        
        /// </summary>
        private void Play_Click(object sender, RoutedEventArgs e)
        {  
            PlayPage.Visibility = Visibility.Visible;
            MainPage.Visibility = Visibility.Hidden;
            Thread.Sleep(100);
          //  GetStats();// updates stats
            Client.WriteThread("PLY");
          //  Statslabel.Content = "Stats: \n Rank: " + rank + "\n Wins: " + wins + "\n Loses: " + loses;
            Task<bool> wait = new Task<bool>(Play_Click_Extended);
            wait.Start();
            while (!wait.Result)
            {
               
            }
            this.Hide();
            GetStats();// updates stats
            Statslabel.Content = "Stats: \n Rank: " + rank + "\n Wins: " + wins + "\n Loses: " + loses;
            try
            {
                game = new Game(Client, playernumber, myusername, enemyid); // turns to the game with the player number and enemy's client specific number
                game.ShowDialog();//Starts the game and back here as the game finish
                Thread.Sleep(500);
                this.Show();
            }
            catch
            {
                game.Close();
                this.Show();
                MessageBox.Show("Error has been occured, this game willn't count. \nif you was about to win please contact with the admins by chatting them as soon as possible.");
            }
        }
        /// <summary>
        ///  waiting for a GSN and when it finds one when someone else trying to connect to a game it sends a number to the client to update him if he starts or not.
        /// </summary>
        /// <returns> returns if the game is going to start</returns>
        private bool Play_Click_Extended()
        { 
            string recieved = Client.ReadThread();
            bool found = false;
            while (recieved.Substring(0,3) != "GSN")
            {
                recieved = Client.ReadThread();

            }
            found = true;
            playernumber = int.Parse(recieved.Substring(3, 1)); //who starts: 1 starter, 2 play after him
            enemyid = int.Parse(recieved.Substring(4, 1)); // enemy's Client Specific number.
            return found;
        }

        /// <summary>
        /// Exits to Main by a button click
        /// </summary>
        private void ExitToMain_Click(object sender, RoutedEventArgs e) 
        {
            ShowMainScreen();//The function which hides all the other grids and setting the Main one.
        }
        /// <summary>
        /// Exits to Main by a button click on the Play screen and sets the game searching on false.
        /// </summary>
        private void ExitPLY_Click(object sender, RoutedEventArgs e) 
        { 
            Client.WriteThread("LGF"); //Leave Game Finder
            ShowMainScreen();//The function which hides all the other grids and setting the Main one.
        }
        /// <summary>
        /// Hides all the screens and sets the main screen visible.
        /// </summary>
        private void ShowMainScreen() 
        {
            if (Suggestion_Page.IsVisible)
                Suggestion_Page.Visibility = Visibility.Hidden;
            if (register.IsVisible)
                register.Visibility = Visibility.Hidden;
            if (HelpPage.IsVisible)
                HelpPage.Visibility = Visibility.Hidden;
            if (PlayPage.IsVisible)
                PlayPage.Visibility = Visibility.Hidden;
            MainPage.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Closes the window and sends OUT to the server.
        /// </summary>
        private void IntroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) 
        {
            Client.WriteThread("OUT");
            Client.Close();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HelpPage.Visibility = Visibility.Hidden;
            Suggestion_Page.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// sends to the server what the client wrote with his name to set it on the chat.txt file.
        /// </summary>
        private void ChatButton_Click(object sender, RoutedEventArgs e) 
        {
            Client.WriteThread("CHT: " + myusername + ":\n" + ChatBox.Text);
            ShowMainScreen();
            ChatBox.Text = null; // deletes what the client wrote before.
        }
    }
}


