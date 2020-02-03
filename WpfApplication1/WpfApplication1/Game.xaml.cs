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
        Image[] xlimgs = new Image[2];
        Image[] limgs = new Image[3];//placeable large ships = 3
        Image[] mimgs = new Image[4];// placeable medium ships = 4
        Image[] simgs = new Image[3];// placeable medium ships = 3
        ServerHandler Client;
        int playernumber;
        int enemyid;
        string enemyname;
        int textblockcounter;//counts how many lines were written to purge it
        int[,] field; // 0 means empty, 1 means full, 2 means hit, 3 means shooted in an empty space
        public Game(ServerHandler Client, int playernumber,string myusername, int enemyid)
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

            if(playernumber == 1)
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
        private int[] GetMouseLoc()
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
        private void Mouse_Move(object sender, MouseEventArgs e)
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
            int col = getloc[0];
            int row = getloc[1];
            // X and Y presents the cordinates of the 1 before the last tap location.
            // Col and Row presents the cordinates of tapped location.
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
                else
                {
                    x = col;
                    y = row;

                }
            }
            else
            {
                Writeintotextblock("Out of range");
            }
        }

        private void CreateExtraLargeSubFlat(int x, int y)//Creating flat large submarine (one row)
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

        private void CreateLargeSubFlat(int x, int y)//Creating flat large submarine (one row)
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
        private void CreateMediumSubFlat(int x, int y)//Creating flat large submarine (one row)
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
        private void CreateSmallSub(int x, int y)
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
        private void RemoveShip(int x, int y)
        {
            bool foundship = false;
            for (int i = 0; i < simgs.Length && !foundship; i++)
                if(simgs[i] != null)
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
                    else if  ((Grid.GetColumn(mimgs[i]) == x-1) && (Grid.GetRow(mimgs[i]) == y))
                        {
                        board.Children.Remove(mimgs[i]);
                        mimgs[i] = null;
                        field[x-1, y] = 0;
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

        
    }
}
