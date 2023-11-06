using System.Windows;

namespace Admin
{
    /// <summary>
    /// Log in Page - creating a connection between the client to the server by creating new ServerHandler
    /// Also sends a log in requests to the server in target to get an okay to pass over to the next window ((MainAdminWindow)).
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainAdminWindow AdminWindow;
        public ServerHandler Client;
        public int usernumber;
        public string myusername;
        /// <summary>
        /// Builder - creates a connection with the server.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Client = new ServerHandler();
        }
        /// <summary>
        ///  Checking the username and password, if it's match to log.txt file it saves it on trueusername and sets the content in Check works, else sets failed.
        /// </summary>
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            string givenusername = Username.Text; // recieves from the client
            string givenpassword = Password.Text; // recieves from the client
            string recievefromserver;
            Client.WriteThread("LAG: \n" + givenusername + "\n" + givenpassword + "\n"); // sends an admin log in request to the server by the form it must to be.
            recievefromserver = Client.ReadThread();
            if (recievefromserver.Contains("Logged in")) // if the server accepts the request.
            {
                usernumber = int.Parse(recievefromserver.Substring(15, recievefromserver.Length - 15));
                myusername = givenusername;
                Check.Content = "works";
                AdminWindow = new MainAdminWindow(Client, myusername, usernumber);
                this.Visibility = Visibility.Hidden;
                AdminWindow.ShowDialog();    
                this.Close();
            }
            else // incase the server deny the client.
            {
                Check.Content = "failed";
            }
        }
        private void IntroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Client.WriteThread("OUT");
            Client.Close();
        }

    }
}
