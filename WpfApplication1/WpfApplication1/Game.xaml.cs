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
        Image[] limgs = new Image[3];//placeable large ships = 3
        Image[] mimgs = new Image[4];// placeable medium ships = 4
        Image[] simgs = new Image[3];// placeable medium ships = 3
        ServerHandler Client;
        int playernumber;
        int enemyid;
        string enemyname;
        int[,] field; // 0 means empty, 1 means full, 2 means hit, 3 means shooted in an empty space
        public Game(ServerHandler Client, int playernumber,string myusername, int enemyid)
        {
            InitializeComponent();
            this.Client = Client;
            this.playernumber = playernumber;
            this.enemyid = enemyid;
            Client.WriteThread("GEN"+enemyid);//Get Enemy Name
            enemyname = Client.ReadThread();
            field = new int[20, 20];

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

        //private void Mouse_Down(object sender, MouseButtonEventArgs e)
        //{
        //    Image Slot = (Image)sender;
        //    if (Slot.Opacity == 1)
        //        Slot.Opacity = 0;
        //    else
        //        Slot.Opacity = 1;
        //}
        //private void Mouse_Enter(object sender, MouseEventArgs e)
        //{
        //    Image Slot = (Image)sender;
        //    if (Slot.Opacity != 1)
        //        Slot.Opacity = 0.5;
        //}
        //private void Mouse_Leave(object sender, MouseEventArgs e)
        //{
        //    Image Slot = (Image)sender;
        //    if (Slot.Opacity != 1)
        //        Slot.Opacity = 0;
        //}

        private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            // X and Y presents the cordinates of the 1 before the last tap location.
            // Col and Row presents the cordinates of tapped location.
            if (playernumber == 1 && col < 10 || playernumber == 2 && col >= 10)// checks if the placement is in the zone
            {
                
                if (x != -1 && y != -1) //means last tap was to place or hadn't tapped before.
                {
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

                    x = -1;
                    y = -1;
                }
                else
                {
                    x = col;
                    y = row;

                }
            }
            else
            {
                textblock.Text += "Out of range \n";
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


                img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine3.png"));
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Grid.SetColumnSpan(img, 3);
                field[x, y] = 1; // fills the array
                field[x + 1, y] = 1;
                field[x + 2, y] = 1;
            }
            else
            {
                if (field[x, y] == 0 || field[x + 1, y] == 0 || field[x + 2, y] == 0)
                {
                    textblock.Text += "You can't place a ship on another one.\n";
                }
                if (full)
                {
                    textblock.Text += "No more ships left!\n";
                }
                if(!full && !(field[x, y] == 0 || field[x + 1, y] == 0 || field[x + 2, y] == 0))
                {
                    //Any other reason.
                }
                
            }
        }
        private void CreateMediumSubFlat(int x, int y)//Creating flat large submarine (one row)
        {
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
                }
            }
            
            if (placeable)
            {
                board.Children.Add(img);


                img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/submarine2.png"));
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Grid.SetColumnSpan(img, 2);
                field[x, y] = 1; // fills the array
                field[x + 1, y] = 1;
            }
            else
            {
                // ADD SOMETHING TO SAY THERE IS NO OPEN SLOTS!
            }
        }
    }
}
