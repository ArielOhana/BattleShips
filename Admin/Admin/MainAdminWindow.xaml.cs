using System.Windows;

namespace Admin
{
    /// <summary>
    /// Admin main window - that window deals with everything admin can do with his abilities - includes messing with the database,
    /// chat and log files which are located in the server's floder.
    /// </summary>
    public partial class MainAdminWindow : Window
    {
        ServerHandler Client;
        string username;
        int usernumber;
        string FindPlayerusername;
        /// <summary>
        /// BUILDER - Recieves the username of the admin, the client's connection with the server and his usernumber.
        /// </summary>
        /// <param name="Client"> The transmission with the server</param>
        /// <param name="username"> The username of the client</param>
        /// <param name="usernumber"> The ID.</param>
        public MainAdminWindow(ServerHandler Client,string username,int usernumber) 
        {
            InitializeComponent();
            this.Client = Client;
            this.username = username;
            this.usernumber = usernumber;
        }
        /// <summary>
        /// Sets up the Find Player screen after being pressed.
        /// </summary>
        private void Find_Player_Click(object sender, RoutedEventArgs e)  
        {
            
            FindPlayerGrid.Visibility = Visibility.Visible;
            InsideFindPlayerGrid.Visibility = Visibility.Hidden;
            Edit_DatabaseGrid.Visibility = Visibility.Hidden;
            Show_LogGrid.Visibility = Visibility.Hidden;
            Show_ChatGrid.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Start the search on the username and shows the results.
        /// </summary>
        private void FindPlayerContinue_Click(object sender, RoutedEventArgs e)
        {
            
            Client.WriteThread("AFP: \n" + Username.Text);// PROTOCOL = Admin Find Player - checks if there is a username with the additational username.
            string recieved = Client.ReadThread();
            if (recieved.Contains("Couldn't find ")) // Incase he couldn't find the username.
            {
                InsideFindPlayerGrid.Visibility = Visibility.Hidden; // Removes (incase it's on) the detials grid.
                Status.Content = recieved; // Writes that the system couldn't find the user
            }
            else
            {
                FindPlayerusername = Username.Text;
                int rank = int.Parse(recieved.Substring(0, recieved.IndexOf("/")));
                int wins = int.Parse(recieved.Substring(recieved.IndexOf("/") +1, recieved.LastIndexOf("/") -(recieved.IndexOf("/") + 1)));
                int loses = int.Parse(recieved.Substring(recieved.LastIndexOf("/")+1));
                ShowPlayerDetails(rank, wins, loses); // cuts the details which recieved from the server
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rank"> the admin level of the client<</param>
        /// <param name="wins">the wins ammount of the client</param>
        /// <param name="loses"> the loses ammount of the client</param>
        public void ShowPlayerDetails(int rank, int wins, int loses)
        {
            InsideFindPlayerGrid.Visibility = Visibility.Visible;
            TextBlockRank.Text = rank.ToString();
            TextBlockWins.Text = wins.ToString();
            TextBlockLoses.Text = loses.ToString();

            
        }
        /// <summary>
        /// attempts to save the changes that the client done.
        /// </summary>
        private void FindPlayerSaveChanges_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                Client.WriteThread("SET: \n"+ FindPlayerusername+"\n" +TextBoxRank.Text+"/"+TextBoxWins.Text+"/"+TextBoxLoses.Text);
                SaveStatus.Content = Client.ReadThread();
                TextBoxLoses.Text = "";
                TextBoxRank.Text = "";
                TextBoxWins.Text = "";
            }
            catch
            {
                SaveStatus.Content = "Failed to save";
            }
        }
        /// <summary>
        /// Recieves the database from the server and shows it.
        /// </summary>
        private void Edit_DataBase_Click(object sender, RoutedEventArgs e)
        {
            InsideFindPlayerGrid.Visibility = Visibility.Hidden;
            FindPlayerGrid.Visibility = Visibility.Hidden;
            Show_LogGrid.Visibility = Visibility.Hidden;
            Show_ChatGrid.Visibility = Visibility.Hidden;
            Edit_DatabaseGrid.Visibility = Visibility.Visible;
            Client.WriteThread("GDB: "); // Admin Get DataBase.
            string recieved = Client.ReadThread(); // Recieves the database.
            DatabaseTextBox.Text = recieved;// sets the database into database's Text Box.
        }
        /// <summary>
        /// Sends the server the changes
        /// </summary>
        private void EditDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.WriteThread("EDB: \n" + DatabaseTextBox.Text); // Sends the new database to the server.
        }
        /// <summary>
        /// Recieves the log from the sever and shows it.
        /// </summary>
        private void Show_Log_Click(object sender, RoutedEventArgs e)
        {
            FindPlayerGrid.Visibility = Visibility.Hidden;
            InsideFindPlayerGrid.Visibility = Visibility.Hidden;
            Edit_DatabaseGrid.Visibility = Visibility.Hidden;
            Show_LogGrid.Visibility = Visibility.Visible;
            Show_ChatGrid.Visibility = Visibility.Hidden;
            Client.WriteThread("GLG: ");// Admin command Get Log
            string recieved = Client.ReadThread(); // recieves the Log.
            LogTextBox.Text = recieved;// sets the log into log's Text Box.
        }
        /// <summary>
        /// Recieves the chat from the sever and shows it.
        /// </summary>
        private void Clients_Chat_Click(object sender, RoutedEventArgs e)
        {
            FindPlayerGrid.Visibility = Visibility.Hidden;
            InsideFindPlayerGrid.Visibility = Visibility.Hidden;
            Edit_DatabaseGrid.Visibility = Visibility.Hidden;
            Show_LogGrid.Visibility = Visibility.Hidden;
            Show_ChatGrid.Visibility = Visibility.Visible;
            Client.WriteThread("GCC: ");// Admin command Get Client Chat
            string recieved= Client.ReadThread(); //Gets the Chat from the server.
            ChatBox.Text = recieved; // Sets the Chat that recieved from the server in the text box.
        }
        /// <summary>
        /// Deletes the chat in the server.
        /// </summary>
        private void DeleteClientChatButton_Click(object sender, RoutedEventArgs e)
        {
            Client.WriteThread("DCC: " + username); //Admin Command, Delete Client Chat.
            ChatBox.Text = "Client Chat Deleted, Press on Client Chat Button for re-check.";
        }
    }
}
