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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ServerHandler Client;
        public Game game;
        private int usernumber;
        private int wins; // Player wins
        private int loses;// Player's loses
        private int rank;// Player's rank
        public int playernumber;
        public int enemyid;
        public string myusername;
        public MainWindow()
        {
           
            InitializeComponent();
            Client = new ServerHandler();
        }
        
        public void GetStats()// updates stats into rank, wins and loses ints.
        {
            Client.WriteThread("STS");
            string recieved = Client.ReadThread();
            int firstcut = recieved.IndexOf("/");
            int secondcut = recieved.LastIndexOf("/") - 1;
            this.rank = int.Parse(recieved.Substring(0, firstcut));
            this.wins = int.Parse(recieved.Substring(firstcut + 1, secondcut - firstcut));
            this.loses = int.Parse(recieved.Substring(secondcut + 2, recieved.Length - (2 + secondcut)));
            
        }

        private void Continue_Click( object sender, RoutedEventArgs e) //Checking the username and password, if it's match to log.txt file it saves it on trueusername and sets the content in Check works, else sets failed.
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
        private void Register_Click(object sender, RoutedEventArgs e) // Registers user
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

      
        private void Info_Click(object sender, RoutedEventArgs e) // starts the info page
        {
            InfoPage.Visibility = Visibility.Visible;
            MainPage.Visibility = Visibility.Hidden;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Client.Close();
            this.Close();// exit the game 
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            PlayPage.Visibility = Visibility.Visible;
            MainPage.Visibility = Visibility.Hidden;
            Thread.Sleep(100);
            GetStats();// updates stats
            Client.WriteThread("PLY");
            Statslabel.Content = "Stats: \n Rank: " + rank + "\n Wins: " + wins + "\n Loses: " + loses;
            Task<bool> wait = new Task<bool>(Play_Click_Extended);
            wait.Start();
            while (!wait.Result)
            {
               
            }
            this.Hide();
            game = new Game(Client, playernumber,myusername, enemyid); // turns to the game with the player number and enemy's client specific number
            game.ShowDialog();
            this.Show();
        }
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


        private void ExitToMain_Click(object sender, RoutedEventArgs e) //Exits to Main by a button click
        {
            ShowMainScreen();//The function which really hides all the other grids and setting the Main one.
        }
        private void ExitPLY_Click(object sender, RoutedEventArgs e) //Exits to Main by a button click on the Play screen and sets the game searching on false.
        {
            Client.WriteThread("LGF"); //Leave Game Finder
            ShowMainScreen();//The function which really hides all the other grids and setting the Main one.
        }
        private void ShowMainScreen()
        {
            if (register.IsVisible)
                register.Visibility = Visibility.Hidden;
            if (InfoPage.IsVisible)
                InfoPage.Visibility = Visibility.Hidden;
            if (PlayPage.IsVisible)
                PlayPage.Visibility = Visibility.Hidden;
            MainPage.Visibility = Visibility.Visible;
        }

        private void IntroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Client.Close();
        }
    }
}


