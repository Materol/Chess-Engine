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

namespace Chess_Project
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        static public Pieces selectedExchange;
        Pieces[] shownPieces = new Pieces[4];
        public Window1(bool whiteExchange)
        {
            InitializeComponent();
            this.MouseDown += Window1_MouseDown;
            CreateExchangeUnits(whiteExchange);
        }
        //adds sprites for all the possible pieces that can be chosen to exchange into
        private void CreateExchangeUnits(bool whiteExchange)
        {
            ClearPieces(); // clears the array of the pieces to show
            if (whiteExchange) // if white, add white sprites.
            {
                Pieces WQueen = new Pieces();
                WQueen.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whitequeen.png", UriKind.Relative)) };
                WQueen.type = "Q";
                shownPieces[0] = WQueen;
                Pieces WRook = new Pieces();
                WRook.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whiterook.png", UriKind.Relative)) };
                WRook.type = "R";
                shownPieces[1] = WRook;
                Pieces WBishop = new Pieces();
                WBishop.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whitebishop.png", UriKind.Relative)) };
                WBishop.type = "B";
                shownPieces[2] = WBishop;
                Pieces WKnight = new Pieces();
                WKnight.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whiteknight.png", UriKind.Relative)) };
                WKnight.type = "N";
                shownPieces[3] = WKnight;
            }
            else // else it's black, add black sprites.
            {
                Pieces BQueen = new Pieces();
                BQueen.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackqueen.png", UriKind.Relative)) };
                BQueen.type = "Q";
                shownPieces[0] = BQueen;
                Pieces BRook = new Pieces();
                BRook.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackrook.png", UriKind.Relative)) };
                BRook.type = "R";
                shownPieces[1] = BRook;
                Pieces BBishop = new Pieces();
                BBishop.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackbishop.png", UriKind.Relative)) };
                BBishop.type = "B";
                shownPieces[2] = BBishop;
                Pieces BKnight = new Pieces();
                BKnight.sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackknight.png", UriKind.Relative)) };
                BKnight.type = "N";
                shownPieces[3] = BKnight;
            }
            DisplayPieces(); // actually draws the pieces.
        }
        //draws the pieces that need to be selected
        private void DisplayPieces()
        {
            for (int i = 0; i < 4; i++)
            {
                shownPieces[i].sprite.Width = 50;
                shownPieces[i].sprite.Height = 50;
                ExchangePick.Children.Add(shownPieces[i].sprite);
                Canvas.SetLeft(shownPieces[i].sprite, 50 + i * 100); // seperates them with a 100 pixel gap.
                Canvas.SetTop(shownPieces[i].sprite, 200);
                shownPieces[i].sprite.MouseDown += Window1_MouseDown;
            }
        }
        //clears the array for the pieces that need to be shown
        private void ClearPieces()
        {
            if (shownPieces[0] != null) // cleares the array if it's not empty so it's clear for the next pawn exchange.
            {
                for (int i = 0; i < 4; i++)
                {
                    ExchangePick.Children.Remove(shownPieces[i].sprite);
                    shownPieces[i] = null;
                }
            }
        }
        //runs when a click is detected in the window
        private void Window1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if(sender == shownPieces[i].sprite) //compares whatever was clicked to the array of the pieces for selection
                {
                    selectedExchange = shownPieces[i]; // sets the selected piece to 'selectedExchange' which is called in the main application
                    ExchangePick.Children.Remove(selectedExchange.sprite); // removes this canvas as a parent.
                    selectedExchange.sprite.MouseDown -= Window1_MouseDown; // removes it from the click event for this window.
                    ClearPieces();
                    e.Handled = true; // so the code will no longer run detecting clicks after we're finished, it must be said that it has been handled.
                    this.Hide(); 
                    break;
                }
            }
        }
    }
}
