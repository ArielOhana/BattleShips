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
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Window
    {
        int x = -1;
        int y = -1;
        Rectangle rec = new Rectangle();
        Image[] xlimgs = new Image[2];// placeable extra large ships = 2
        Image[] limgs = new Image[3];//placeable large ships = 3
        Image[] mimgs = new Image[4];// placeable medium ships = 4
        Image[] simgs = new Image[3];// placeable medium ships = 3
        ServerHandler Client;
        int playernumber;
        int enemyid;
        string enemyname;
        int textblockcounter;//counts how many lines were written to purge it
        int[,] field; // 0 means empty, 1 means full, 2 means hit, 3 means shooted in an empty space
        bool preparing = true;
        public Game(ServerHandler Client, int playernumber,string myusername, int enemyid) // Creates a game
        {
            this.textblockcounter = 0;
            this.rec.Fill = Brushes.Red;
            this.rec.Opacity = 0.7;
            InitializeComponent();
            this.Client = Client;
            this.playernumber = playernumber;
            this.enemyid = enemyid;
            Client.WriteThread("GEN"+enemyid);//Get Enemy Name
            enemyname = Client.ReadThread();
            field = new int[20, 20];
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = 0;
                }
            }

            if(playernumber == 1) //writes on the upper labels the names of the players by their player number.
            {
                Player1lbl.Content = Player1lbl.Content+ myusername;
                Player2lbl.Content = Player2lbl.Content + enemyname;
            }
            else
            {
                Player1lbl.Content = Player1lbl.Content + enemyname;
                Player2lbl.Content = Player2lbl.Content + myusername;

            }
        }
        private int[] GetMouseLoc() //Gets the mouse location
        {
            var point = Mouse.GetPosition(board);
            int row = 0;
            int col = 0;
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;

            // calc row mouse was over
            foreach (var rowDefinition in board.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                    break;
                row++;
            }

            // calc col mouse was over
            foreach (var columnDefinition in board.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                    break;
                col++;
            }
            int[] tosend = new int[2];
            tosend[0] = col;
            tosend[1] = row;
            return tosend;
        }
        //private void Mouse_Down(object sender, MouseButtonEventArgs e)
        //{
        //    Image Slot = (Image)sender;
        //    if (Slot.Opacity == 1)
        //        Slot.Opacity = 0;
        //    else
        //        Slot.Opacity = 1;
        //}
        private void Mouse_Move(object sender, MouseEventArgs e) //Event that happens as the mouse moves
        {
            int[] getloc = GetMouseLoc();
            int col = getloc[0];
            int row = getloc[1];
           
            if (!(playernumber == 1 && col < 10 || playernumber == 2 && col >= 10 ) &&(field[col,row] == 0))// checks if the placement is in the zone
            {               
                board.Children.Remove(rec);
                Grid.SetColumn(rec, col);
                Grid.SetRow(rec, row);
                board.Children.Add(rec);
            }
            else//if it's inbound
            {
                if (board.Children.Contains(rec))
                    board.Children.Remove(rec);
            }
        }

        private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int[] getloc = GetMouseLoc();
            int col = getloc[0]; // sets last col to the current one
            int row = getloc[1];// sets last row to the current one
            // X and Y presents the cordinates of the 1 before the last tap location.
            // Col and Row presents the cordinates of tapped location.
            if (preparing)
            {
                if (playernumber == 1 && col < 10 || playernumber == 2 && col >= 10)// checks if the placement is in the zone
                {

                    if (x != -1 && y != -1) //means last tap was to place or hadn't tapped before.
                    {
                        if (field[x, y] == 0)
                        {
                            if (x == col - 3 && y == row)
                            {
                                CreateExtraLargeSubFlat(x, y);

                            }
                            if (x - 3 == col && y == row)
                            {
                                CreateExtraLargeSubFlat(col, y);
                            }
                            if (x == col - 2 && y == row)
                            {
                                CreateLargeSubFlat(x, y);

                            }
                            if (x - 2 == col && y == row)
                            {
                                CreateLargeSubFlat(col, y);
                            }
                            if (x == col - 1 && y == row)
                            {
                                CreateMediumSubFlat(x, y);
                            }
                            if (x - 1 == col && y == row)
                            {
                                CreateMediumSubFlat(col, y);
                            }
                            if (x == col && y == row)
                            {
                                CreateSmallSub(col, row);
                            }

                            x = -1;
                            y = -1;
                        }
                        else//pressed on a battleship
                        {
                            RemoveShip(x, y);
                            x = -1;
                            y = -1;
                        }
                    }
                    else // didn't tapped before
                    {
                        x = col;
                        y = row;

                    }
                }
                else //incase it's out of range
                {
                    Writeintotextblock("Out of range");
                }
                if (UpdateShipsLeft()) // updates the "shipsleft" label and enters if there are no ships left
                {
                    Writeintotextblock("No ships left, Press ENTER to start playing");
                }
            }// all of that happens if the players are still preparing their ships for the game.
            else if(playernumber == 2 && col < 10 || playernumber == 1 && col >= 10)
            {


            }
        }

        private void CreateExtraLargeSubFlat(int x, int y) // Creates extra large ship and sets it into the field, board and array.
        {
            bool full = true;
            bool placeable = false;
            Image img = new Image();
            for (int i = 0; i < xlimgs.Length && !placeable; i++)// checks if there are large ships left
            {
                if (xlimgs[i] == null)
                {
                    if (field[x, y] == 0 && field[x + 1, y] == 0 && field[x + 2, y] == 0 && field[x + 3, y] == 0)
                    {
                        xlimgs[i] = img;
                        placeable = true;
                    }
                    full = false;
                }
            }
            if (placeable)
            {
                board.Children.Add(img);

                if (x + 3 < 10) //first half x = loc 3 = additational space 10 = half board
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine4Left.png"));
                else
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine4Right.png"));

                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Grid.SetColumnSpan(img, 4);
                field[x, y] = 1; // fills the array
                field[x + 1, y] = 1;
                field[x + 2, y] = 1;
                field[x + 3, y] = 1;
            }
            else
            {
                if (field[x, y] == 1 || field[x + 1, y] == 1 || field[x + 2, y] == 1 || field[x + 3, y] == 1)
                {
                    Writeintotextblock("You can't place a ship on another one.");
                }
                if (full)
                {
                    Writeintotextblock("No more ships left!");
                }
                if (!full || !(field[x, y] == 1 || field[x + 1, y] == 1 || field[x + 2, y] == 1 || field[x + 3, y] == 1))
                {
                    //Any other reason.
                }

            }
        }

        private void CreateLargeSubFlat(int x, int y) // Creates large ship and sets it into the field, board and array.
        {
            bool full = true;
            bool placeable = false;
            Image img = new Image();
            for (int i = 0; i < limgs.Length && !placeable; i++)// checks if there are large ships left
            {
                if (limgs[i] == null)
                {
                    if (field[x, y] == 0 && field[x + 1, y] == 0 && field[x + 2, y] == 0)
                    {
                        limgs[i] = img;
                        placeable = true;
                    }
                    full = false;
                }
            }
            if (placeable)
            {
                board.Children.Add(img);

                if (x + 2 < 10) //first half x = loc 3 = additational space 10 = half board
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine3Left.png"));
                else
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine3Right.png"));

                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Grid.SetColumnSpan(img, 3);
                field[x, y] = 1; // fills the array
                field[x + 1, y] = 1;
                field[x + 2, y] = 1;
            }
            else
            {
                if (field[x, y] == 1 || field[x + 1, y] == 1 || field[x + 2, y] == 1)
                {
                    Writeintotextblock("You can't place a ship on another one.");
                }
                if (full)
                {
                    Writeintotextblock("No more ships left!");
                }
                if(!full || !(field[x, y] == 1 || field[x + 1, y] == 1 || field[x + 2, y] == 1))
                {
                    //Any other reason.
                }
                
            }
        }
        private void CreateMediumSubFlat(int x, int y) // Creates medium ship and sets it into the field, board and array.
        {
            bool full = true;
            bool placeable = false;
            Image img = new Image();
            for (int i = 0; i < mimgs.Length && !placeable; i++)// Checks if there are medium ships left 
            {
                if (mimgs[i] == null)
                {
                    if (field[x, y] == 0 && field[x + 1, y] == 0)
                    {
                        mimgs[i] = img;
                        placeable = true;
                    }
                    full = false;
                }
            }
            
            if (placeable)
            {
                board.Children.Add(img);

                if(x + 1 < 10) //first half x = loc 1 = additational space 10 = half board
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine2Left.png"));
                else
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine2Right.png"));

                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Grid.SetColumnSpan(img, 2);
                field[x, y] = 1; // fills the array
                field[x + 1, y] = 1;
            }

                else
            {
                    if (field[x, y] == 1 || field[x + 1, y] == 1)
                    {
                        Writeintotextblock("You can't place a ship on another one.");
                    }
                    if (full)
                    {
                        Writeintotextblock("No more ships left!");
                    }
                    if (!full || !(field[x, y] == 1 || field[x + 1, y] == 1))
                    {
                        //Any other reason.
                    }

                }
            
        }
        private void CreateSmallSub(int x, int y) // Creates small ship and sets it into the field, board and array.
        {

            bool full = true;
            bool placeable = false;
            Image img = new Image();
            for (int i = 0; i < simgs.Length && !placeable; i++)// Checks if there are medium ships left 
            {
                if (simgs[i] == null)
                {
                    if (field[x, y] == 0)
                    {
                        simgs[i] = img;
                        placeable = true;
                    }
                    full = false;
                }
            }

            if (placeable)
            {
                board.Children.Add(img);

                if (x < 10) //first half x = loc 2 = additational space 10 = half board
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine1Left.png"));
                else
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine1Right.png"));

                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                field[x, y] = 1; // fills the array
            }

            else
            {
                if (field[x, y] == 1)
                {
                    Writeintotextblock("You can't place a ship on another one.");
                }
                if (full)
                {
                    Writeintotextblock("No more ships left!");
                }
                if (!full || !(field[x, y] == 1))
                {
                    //Any other reason.
                }

            }
        }
        private void RemoveShip(int x, int y) //Removes ship from it's array, remove it from the field and board childrens
        {
            bool foundship = false;
            for (int i = 0; i < simgs.Length && !foundship; i++)
                if (simgs[i] != null)
                {
                    if (Grid.GetColumn(simgs[i]) == x && Grid.GetRow(simgs[i]) == y)
                    {
                        board.Children.Remove(simgs[i]);
                        simgs[i] = null;
                        field[x, y] = 0;
                        foundship = true;
                    }
                }
            for (int i = 0; i < mimgs.Length && !foundship; i++)
                if (mimgs[i] != null)
                {
                    if ((Grid.GetColumn(mimgs[i]) == x) && (Grid.GetRow(mimgs[i]) == y))
                    {
                        board.Children.Remove(mimgs[i]);
                        mimgs[i] = null;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        foundship = true;
                    }
                    else if ((Grid.GetColumn(mimgs[i]) == x - 1) && (Grid.GetRow(mimgs[i]) == y))
                    {
                        board.Children.Remove(mimgs[i]);
                        mimgs[i] = null;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        foundship = true;

                    }

                }
            for (int i = 0; i < limgs.Length && !foundship; i++)
                if (limgs[i] != null)
                {
                    if ((Grid.GetColumn(limgs[i]) == x) && (Grid.GetRow(limgs[i]) == y))
                    {
                        board.Children.Remove(limgs[i]);
                        limgs[i] = null;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        field[x + 2, y] = 0;
                        foundship = true;
                    }
                    else if ((Grid.GetColumn(limgs[i]) == x - 1) && (Grid.GetRow(limgs[i]) == y))
                    {
                        board.Children.Remove(limgs[i]);
                        limgs[i] = null;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        foundship = true;

                    }
                    else if ((Grid.GetColumn(limgs[i]) == x - 2) && (Grid.GetRow(limgs[i]) == y))
                    {
                        board.Children.Remove(limgs[i]);
                        limgs[i] = null;
                        field[x - 2, y] = 0;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        foundship = true;

                    }

                }
            for (int i = 0; i < xlimgs.Length && !foundship; i++)
                if (xlimgs[i] != null)
                {
                    if ((Grid.GetColumn(xlimgs[i]) == x) && (Grid.GetRow(xlimgs[i]) == y))
                    {
                        board.Children.Remove(xlimgs[i]);
                        xlimgs[i] = null;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        field[x + 2, y] = 0;
                        field[x + 3, y] = 0;
                        foundship = true;
                    }
                    else if ((Grid.GetColumn(xlimgs[i]) == x - 1) && (Grid.GetRow(xlimgs[i]) == y))
                    {
                        board.Children.Remove(xlimgs[i]);
                        xlimgs[i] = null;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        field[x + 2, y] = 0;
                        foundship = true;

                    }
                    else if ((Grid.GetColumn(xlimgs[i]) == x - 2) && (Grid.GetRow(xlimgs[i]) == y))
                    {
                        board.Children.Remove(xlimgs[i]);
                        xlimgs[i] = null;
                        field[x - 2, y] = 0;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        field[x + 1, y] = 0;
                        foundship = true;

                    }
                    else if ((Grid.GetColumn(xlimgs[i]) == x - 3) && (Grid.GetRow(xlimgs[i]) == y))
                    {
                        board.Children.Remove(xlimgs[i]);
                        xlimgs[i] = null;
                        field[x - 3, y] = 0;
                        field[x - 2, y] = 0;
                        field[x - 1, y] = 0;
                        field[x, y] = 0;
                        foundship = true;

                    }
                }
            if(foundship)
            {
                Writeintotextblock("Ship Removed");
            }
        }

        private void Writeintotextblock(string thingtowrite)
        {
            if (textblockcounter >= 18)// max lines
            {
                textblock.Text = thingtowrite + "\n";
                textblockcounter = 0;
            }
            else
            {
                textblock.Text += thingtowrite+ "\n";
            }
            textblockcounter++;
        }
        private bool UpdateShipsLeft() //Updates how many ships left and sets it into "Shipleft" label and sends true if no ships left
        {
            int extralarge = 0;
            int large = 0;
            int medium = 0;
            int small = 0;
            for (int i = 0; i < xlimgs.Length; i++)
                if (xlimgs[i] == null)
                    extralarge++;
            for (int i = 0; i < limgs.Length; i++)
                if (limgs[i] == null)
                    large++;
            for (int i = 0; i < mimgs.Length; i++)
                if (mimgs[i] == null)
                    medium++;
            for (int i = 0; i < simgs.Length; i++)
                if (simgs[i] == null)
                    small++;
            Shipsleft.Content = "extra large ships left: " + extralarge + "\n" + "large ships left: " + large + "\n" + "medium ships left: " + medium + "\n" + "small ships left: " + small + "\n";
            if (small + medium + large + extralarge == 0)
                return true;
            return false;

        }
        private int GetHowManyShipsLeft() //Updates how many ships left and sets it into "Shipleft" label and sends true if no ships left
        {
            int extralarge = 0;
            int large = 0;
            int medium = 0;
            int small = 0;
            for (int i = 0; i < xlimgs.Length; i++)
                if (xlimgs[i] == null)
                    extralarge++;
            for (int i = 0; i < limgs.Length; i++)
                if (limgs[i] == null)
                    large++;
            for (int i = 0; i < mimgs.Length; i++)
                if (mimgs[i] == null)
                    medium++;
            for (int i = 0; i < simgs.Length; i++)
                if (simgs[i] == null)
                    small++;
            return small + medium + large + extralarge;

        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (GetHowManyShipsLeft() == 0)
            {
                
                Client.WriteThread("SPP" + enemyid);
                Finishcontinue();
            }
            else
                Writeintotextblock("You need to place more ships!");
        }
        private async void Finishcontinue()
        {
            preparing = false;
            Finish.Visibility = Visibility.Hidden;
            textblock.Text = "Waiting for the second player to finish his preparing, please wait";
            Shipsleft.Visibility = Visibility.Hidden;
            bool c;
            Task<bool> waiting = new Task<bool>(Finish2);
            waiting.Start();

            c = await waiting;
            if (c)
            {
                textblock.Text = "Start Playing"; 
                SendField();
                
            }
        }
        private bool Finish2()
        {
            if (Client.ReadThread() == "Start Playing")
                return true;
            return false;
        }
        private void SendField()
        {
            string tosendfield = "FLD:\n";
            for (int i = 0; i < field.GetLength(1); i++)
            {
                for (int j = 0; j < field.GetLength(0); j++)
                {
                    tosendfield += field[i,j];
                }
                tosendfield += "\n";
            }
            Client.WriteThread(tosendfield);
        }
    }
     
}

