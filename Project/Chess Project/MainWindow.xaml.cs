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
using System.Windows.Threading;
using System.IO;
using Path = System.IO.Path;
using System.Text.RegularExpressions;

namespace Chess_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Pieces
    {
        public string colour; // colour of the piece e.g. W for white
        public string type; // piece type e.g. B for Bishop
        public int x; // x location on the board, relative to the array e.g. 0 = far left on board
        public int y; // y location on the board, relative to the array e.g. 0 = top of the board
        public Image sprite; // the image for the piece, stored in the resource folder.
        public int hasmoved = 0; // counts the amount of moves made by pieces.
        public bool canPassant = true; // only relevant to pawns. This is the only exclusive variable in the class, making it almost useless.
        public Pieces CopyPiece()
        {
            Pieces pieceCopy = new Pieces();
            if (this == null)
            {
                pieceCopy = null;
            }
            else
            {
                pieceCopy.x = this.x; // copies the contents of the saved undos array to piecesB.
                pieceCopy.y = this.y;
                pieceCopy.type = this.type;
                pieceCopy.colour = this.colour;
                pieceCopy.hasmoved = this.hasmoved;
            }
            return pieceCopy;
        }
    }
    public class Board
    {
        public const int boardSize = 8;
        public List<PlayerBoard> undos = new List<PlayerBoard>();
        public string turn = "White";
        public Pieces[,] fieldPiece = new Pieces[boardSize, boardSize];
        public bool[,] movableSpots = new bool[boardSize, boardSize];
        bool legalCheck = true;
        public Pieces grabbedPiece;
        public int drawBy50Count = 0;
        public int undoCount = -1;
        public int fullMove = 1;
        //resets the movablespots array
        private void ResetMovableSpots()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    movableSpots[i, j] = false; // this sets the movable places in the array to nill, resets them.
                }
            }
        }
        //creates all the initial starting pieces, assigning their coordinates and colour.
        public void CreateInitialPieces()
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080;
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            //creates the white pawns
            for (int i = 0; i < boardSize; i++)
            {
                Pieces piece = new Pieces();
                piece.colour = "White";
                piece.type = "P";
                piece.x = i;
                piece.y = 6;
                fieldPiece[i, 6] = piece;
            }
            // creates the black pawns
            for (int i = 0; i < boardSize; i++)
            {
                Pieces piece = new Pieces();
                piece.colour = "Black";
                piece.type = "P";
                piece.x = i;
                piece.y = 1;
                fieldPiece[i, 1] = piece;
            }
            // creates the 2 kings
            for (int i = 0; i < boardSize; i += 7)
            {
                Pieces piece = new Pieces();
                piece.type = "K";
                piece.x = 4;
                piece.y = i;
                if (i == 0)
                {
                    piece.colour = "Black";
                }
                else if (i == 7)
                {
                    piece.colour = "White";
                }
                fieldPiece[4, i] = piece;
            }
            // Creates the 2 queens
            for (int i = 0; i < boardSize; i += 7)
            {
                Pieces piece = new Pieces();
                piece.type = "Q";
                piece.x = 3;
                piece.y = i;
                if (i == 0)
                {
                    piece.colour = "Black";
                }
                else if (i == 7)
                {
                    piece.colour = "White";
                }
                fieldPiece[3, i] = piece;
            }
            // creates the 2 bishops
            for (int i = 0; i < boardSize; i += 7)
            {
                for (int j = 2; j < 6; j += 3)
                {
                    Pieces piece = new Pieces();
                    piece.type = "B";
                    piece.x = j;
                    piece.y = i;
                    if (i == 0)
                    {
                        piece.colour = "Black";
                    }
                    else if (i == 7)
                    {
                        piece.colour = "White";
                    }
                    fieldPiece[j, i] = piece;
                }
            }
            // creates the 2 knights
            for (int i = 0; i < boardSize; i += 7)
            {
                for (int j = 1; j < 7; j += 5)
                {
                    Pieces piece = new Pieces();
                    piece.type = "N"; // N refers to knight, like 'night' to prevent using K for both or having to use 2 characters
                    piece.x = j;
                    piece.y = i;
                    if (i == 0)
                    {
                        piece.colour = "Black";
                    }
                    else if (i == 7)
                    {
                        piece.colour = "White";
                    }
                    fieldPiece[j, i] = piece;
                }
            }
            // creates the 2 rooks
            for (int i = 0; i < boardSize; i += 7)
            {
                for (int j = 0; j < boardSize; j += 7)
                {
                    Pieces piece = new Pieces();
                    piece.type = "R";
                    piece.x = j;
                    piece.y = i;
                    if (i == 0)
                    {
                        piece.colour = "Black";
                    }
                    else if (i == 7)
                    {
                        piece.colour = "White";
                    }
                    fieldPiece[j, i] = piece;
                }
            }
        }
        //gets the x and y values of the movable pieces array
        public void FetchMBxy(bool spot, ref int X, ref int Y)
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (spot == movableSpots[i, j])
                    {
                        X = i;
                        Y = j;
                    }
                }
            }
        }
        //stores all the rules for all the pieces. When called will update the 'MovableSpots' array to show where the piece being checked can move.
        public void MovingRules()
        {
            int y = grabbedPiece.y;
            int x = grabbedPiece.x;
            string colour = grabbedPiece.colour;
            string type = grabbedPiece.type;
            ResetMovableSpots();
            // Pawn movement rules
            if (type == "P" && y != 7 && y != 0) // possible pawn moves
            {
                if (colour == "White")
                {
                    // non capture moves
                    if (fieldPiece[x, y - 1] == null)
                    {
                        movableSpots[x, y - 1] = true;
                        if (y==6 && fieldPiece[x, y - 2] == null) movableSpots[x, y - 2] = true; // first pawns move can move double
                    }
                    //capture moves         //legalcheck is for checking castling and other board arrangements since the pawn is still attacking that spot, despite no unit being there
                    if (x > 0 && y > 0 && (!legalCheck || (fieldPiece[x - 1, y - 1] != null && fieldPiece[x - 1, y - 1].colour != colour)))
                    {
                        movableSpots[x - 1, y - 1] = true;
                    }                       //legalcheck is for checking castling and other board arrangements since the pawn is still attacking that spot, despite no unit being there
                    if (x < 7 && y > 0 && (!legalCheck || (fieldPiece[x + 1, y - 1] != null && fieldPiece[x + 1, y - 1].colour != colour)))
                    {
                        movableSpots[x + 1, y - 1] = true;
                    }
                    // en passant
                    if (x > 0 && y == 3 && fieldPiece[x - 1, y] != null && fieldPiece[x - 1, y].type == "P" && fieldPiece[x - 1, y].colour != colour  && fieldPiece[x - 1, y].canPassant)
                    {
                        movableSpots[x - 1, y - 1] = true; // we can guarantee this spot is empty since if there is a pawn at this coordinate with 1 move, it must have double moved
                    }
                    if (x < 7 && y == 3 && fieldPiece[x + 1, y] != null && fieldPiece[x + 1, y].type == "P" && fieldPiece[x + 1, y].colour != colour  && fieldPiece[x + 1, y].canPassant)
                    {
                        movableSpots[x + 1, y - 1] = true; // we can guarantee this spot is empty since if there is a pawn at this coordinate with 1 move, it must have double moved
                    }
                }
                if (colour == "Black")
                {
                    // non capture moves
                    if (fieldPiece[x, y + 1] == null)
                    {
                        movableSpots[x, y + 1] = true;
                        if (y == 1 && fieldPiece[x, y + 2] == null) movableSpots[x, y + 2] = true; // first pawns can move double
                    }
                    //capture moves
                    //legalcheck is for checking which spots on the board are attacked by the pawn rather than what spots it can directly move to.
                    if (x > 0 && y < 7 && (!legalCheck || (fieldPiece[x - 1, y + 1] != null && fieldPiece[x - 1, y + 1].colour != colour)))
                    {
                        movableSpots[x - 1, y + 1] = true;
                    }     //legalcheck is for checking which spots on the board are attacked by the pawn rather than what spots it can directly move to.
                    if (x < 7 && y < 7 && (!legalCheck || (fieldPiece[x + 1, y + 1] != null && fieldPiece[x + 1, y + 1].colour != colour)))
                    {
                        movableSpots[x + 1, y + 1] = true;
                    }
                    // en passant
                    if (x > 0 && y == 4 && fieldPiece[x - 1, y] != null && fieldPiece[x - 1, y].type == "P" && fieldPiece[x - 1, y].colour != colour && fieldPiece[x - 1, y].canPassant)
                    {
                        movableSpots[x - 1, y + 1] = true;
                    }
                    if (x < 7 && y == 4 && fieldPiece[x + 1, y] != null && fieldPiece[x + 1, y].type == "P" && fieldPiece[x + 1, y].colour != colour && fieldPiece[x + 1, y].canPassant)
                    {
                        movableSpots[x + 1, y + 1] = true;
                    }
                }
            }
            // Knight movement rules
            if (type == "N")
            {
                for (int i = -2; i < 3; i++)
                {
                    if (i == 2 && x < boardSize - 2 || i == -2 && x > 1) // going out sideways by 2
                    {        // if spot is empty                 or if it is not empty, and the piece is an enemy
                        if (y > 0 && (fieldPiece[x + i, y - 1] == null || (fieldPiece[x + i, y - 1] != null && fieldPiece[x + i, y - 1].colour != colour))) movableSpots[x + i, y - 1] = true;
                        if (y < 7 && (fieldPiece[x + i, y + 1] == null || (fieldPiece[x + i, y + 1] != null && fieldPiece[x + i, y + 1].colour != colour))) movableSpots[x + i, y + 1] = true;
                    }
                    if (i == 1 && x < boardSize - 1 || i == -1 && x > 0) // going up or down by 2
                    {        // if spot is empty                 or if it is not empty, and the piece is an enemy
                        if (y > 1 && (fieldPiece[x + i, y - 2] == null || (fieldPiece[x + i, y - 2] != null && fieldPiece[x + i, y - 2].colour != colour))) movableSpots[x + i, y - 2] = true;
                        if (y < 6 && (fieldPiece[x + i, y + 2] == null || (fieldPiece[x + i, y + 2] != null && fieldPiece[x + i, y + 2].colour != colour))) movableSpots[x + i, y + 2] = true;
                    }
                }
            }
            // Bishop movement rules
            if (type == "B")
            {
                for (int i = x + 1, j = y + 1; i < boardSize && j < boardSize; i++, j++)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x + 1, j = y - 1; i < boardSize && j > -1; i++, j--)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1, j = y + 1; i > -1 && j < boardSize; i--, j++)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1, j = y - 1; i > -1 && j > -1; i--, j--)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
            }
            // rook movement rules
            if (type == "R")
            {
                for (int i = x + 1; i < boardSize; i++)
                {
                    if (fieldPiece[i, y] == null) movableSpots[i, y] = true;
                    else if (fieldPiece[i, y].colour != colour)
                    {
                        movableSpots[i, y] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1; i > -1; i--)
                {
                    if (fieldPiece[i, y] == null) movableSpots[i, y] = true;
                    else if (fieldPiece[i, y].colour != colour)
                    {
                        movableSpots[i, y] = true;
                        break;
                    }
                    else break;
                }
                for (int j = y + 1; j < boardSize; j++)
                {
                    if (fieldPiece[x, j] == null) movableSpots[x, j] = true;
                    else if (fieldPiece[x, j].colour != colour)
                    {
                        movableSpots[x, j] = true;
                        break;
                    }
                    else break;
                }
                for (int j = y - 1; j > -1; j--)
                {
                    if (fieldPiece[x, j] == null) movableSpots[x, j] = true;
                    else if (fieldPiece[x, j].colour != colour)
                    {
                        movableSpots[x, j] = true;
                        break;
                    }
                    else break;
                }
            }
            // Queen movement rules
            if (type == "Q")
            {
                for (int i = x + 1, j = y + 1; i < boardSize && j < boardSize; i++, j++)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x + 1, j = y - 1; i < boardSize && j > -1; i++, j--)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1, j = y + 1; i > -1 && j < boardSize; i--, j++)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1, j = y - 1; i > -1 && j > -1; i--, j--)
                {
                    if (fieldPiece[i, j] == null) movableSpots[i, j] = true;
                    else if (fieldPiece[i, j].colour != colour)
                    {
                        movableSpots[i, j] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x + 1; i < boardSize; i++)
                {
                    if (fieldPiece[i, y] == null) movableSpots[i, y] = true;
                    else if (fieldPiece[i, y].colour != colour)
                    {
                        movableSpots[i, y] = true;
                        break;
                    }
                    else break;
                }
                for (int i = x - 1; i > -1; i--)
                {
                    if (fieldPiece[i, y] == null) movableSpots[i, y] = true;
                    else if (fieldPiece[i, y].colour != colour)
                    {
                        movableSpots[i, y] = true;
                        break;
                    }
                    else break;
                }
                for (int j = y + 1; j < boardSize; j++)
                {
                    if (fieldPiece[x, j] == null) movableSpots[x, j] = true;
                    else if (fieldPiece[x, j].colour != colour)
                    {
                        movableSpots[x, j] = true;
                        break;
                    }
                    else break;
                }
                for (int j = y - 1; j > -1; j--)
                {
                    if (fieldPiece[x, j] == null) movableSpots[x, j] = true;
                    else if (fieldPiece[x, j].colour != colour)
                    {
                        movableSpots[x, j] = true;
                        break;
                    }
                    else break;
                }
            }
            // King movement rules
            if (type == "K")
            {
                // default king movement
                for (int i = -1; i < 2; i++)
                {
                    if ((i == -1 && x > 0) || (i == 1 && x < 7) || i == 0)
                    {
                        if (y > 0 && (fieldPiece[x + i, y - 1] == null || (fieldPiece[x + i, y - 1] != null && fieldPiece[x + i, y - 1].colour != colour))) movableSpots[x + i, y - 1] = true;
                        if (fieldPiece[x + i, y] == null || (fieldPiece[x + i, y] != null && fieldPiece[x + i, y].colour != colour)) movableSpots[x + i, y] = true;
                        if (y < 7 && (fieldPiece[x + i, y + 1] == null || (fieldPiece[x + i, y + 1] != null && fieldPiece[x + i, y + 1].colour != colour))) movableSpots[x + i, y + 1] = true;
                    }
                }
                //castling
                if (grabbedPiece.hasmoved == 0) //
                {
                    // king's & queen's side castle
                    if (fieldPiece[7, y] != null && fieldPiece[7, y].type == "R" && fieldPiece[7, y].hasmoved == 0 && fieldPiece[x + 1, y] == null && fieldPiece[x + 2, y] == null) movableSpots[x + 2, y] = true;
                    if (fieldPiece[0, y] != null && fieldPiece[0, y].type == "R" && fieldPiece[0, y].hasmoved == 0 && fieldPiece[x - 1, y] == null && fieldPiece[x - 2, y] == null && fieldPiece[x - 3, y] == null) movableSpots[x - 2, y] = true;
                }
            }
            // Removes any non legal moves
            if (legalCheck) IsMoveLegal();
        }
        //checks the leaglity of a move, ensures that the king is still not in check after a move is made.
        private void IsMoveLegal()
        {
            legalCheck = false; // prevents this function from running again while checking possible enemy moves in MovinRules, causing an infinite loop
            int X = grabbedPiece.x;
            int Y = grabbedPiece.y;
            Pieces backupPiece = grabbedPiece; // backsup the original values of these arrays and variables to ensure we can restore them after the check is done.
            bool[,] backupSpots = (bool[,])movableSpots.Clone();
            Pieces[,] backupBoard = (Pieces[,])fieldPiece.Clone();
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (backupSpots[i, j])
                    {
                        fieldPiece[i, j] = backupPiece; // moves the piece from original position to movable spot.
                        fieldPiece[X, Y] = null;
                        foreach (Pieces piece in fieldPiece)
                        {
                            if (piece != null && piece.colour != backupPiece.colour)
                            {
                                grabbedPiece = piece; //sets the checked piece to grabbed piece so we are able to reuse the movingrules subroutine which uses MovingRules
                                MovingRules();
                                if (!KingSafe(backupPiece, i)) // checks whether king is in check at any of the points withing the piece's attack range.
                                {
                                    backupSpots[i, j] = false; // removes the move as a valid move
                                    break; // stops searching since move is already invalid and further checking is redundant
                                }
                            }
                        }
                        fieldPiece = (Pieces[,])backupBoard.Clone(); // returns to original board
                    }
                }
            }
            movableSpots = (bool[,])backupSpots.Clone(); // loads the updated possible moves
            grabbedPiece = backupPiece; // restores original piece
            legalCheck = true;
        }
        //checks whether the king is in check in the movable spots / the castle is valid
        public bool KingSafe(Pieces backupPiece, int i)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {                    // this checks that the move is a castle since the desired move is 2 out on the x axis which means it is a castle since a king can only move 1 out
                    if (backupPiece.type == "K" && Math.Abs(backupPiece.x - i) == 2) // if move is castle, checks castle requirements (such as no castling in a spot that check is in between)
                    {
                        int j = backupPiece.y;
                        if (i == 2 && (movableSpots[4, j] || movableSpots[3, j] || movableSpots[2, j])) return false; // queenside castle
                        else if (i == 6 && (movableSpots[4, j] || movableSpots[5, j] || movableSpots[6, j])) return false; //king side castle
                    }
                    else if (fieldPiece[x, y] != null && fieldPiece[x, y].type == "K" && fieldPiece[x, y].colour == backupPiece.colour && movableSpots[x, y]) return false; //checks whether the places that the enemy pieces can go to holds the enemy's king.
                }
            }
            return true;
        }
        // checks whether at the current board the king is in check.
        private bool CheckKingCheck()
        {
            Pieces grabbedPieceBackup = grabbedPiece;
            Pieces king = null;
            foreach (Pieces piece in fieldPiece)
            {
                if (piece != null && piece.colour == turn && piece.type == "K") king = piece; // locates the king we're checking for check
            }
            foreach (Pieces piece in fieldPiece)
            {
                if (piece != null && piece.colour != turn)
                {
                    grabbedPiece = piece;
                    legalCheck = false; // since we don't care whether the piece attacking can actually legally move there, we can ignore usual 'illegal' moves.
                    MovingRules();
                    if (movableSpots[king.x, king.y])
                    {
                        legalCheck = true;
                        grabbedPiece = grabbedPieceBackup;
                        return true;
                    }
                }
            }
            grabbedPiece = grabbedPieceBackup;
            legalCheck = true;
            return false;
        }
        //checks a pieces coordinates, then it assigns the piece to the respective place within the array
        public void SetPiecesToArray()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (fieldPiece[i, j] != null)
                    {
                        int x = fieldPiece[i, j].x;
                        int y = fieldPiece[i, j].y;
                        if (fieldPiece[x, y] != fieldPiece[i, j])
                        {
                            fieldPiece[x, y] = fieldPiece[i, j];
                            fieldPiece[i, j] = null;
                        }
                    }
                }
            }
        }
        //performs a capture of a piece when called. (will be used to add piece capture tracking later on)
        private void pieceCapture(int X, int Y, Canvas ChessBoard)
        {
            ChessBoard.Children.Remove(fieldPiece[X, Y].sprite);
            fieldPiece[X, Y] = null;
            drawBy50Count = 0;
        }
        // checks whether the move being performed by the king is a castle.
        private void CastleCheck(int X, int Y)
        {
            if (Math.Abs(X - grabbedPiece.x) == 2) // if it moves 2 horizontally as king, it must be a castle
            {
                if (X == 6) // kingside
                {
                    fieldPiece[5, Y] = fieldPiece[7, Y];
                    fieldPiece[5, Y].x = 5;
                    fieldPiece[7, Y] = null;
                }
                if (X == 2) // queenside
                {
                    fieldPiece[3, Y] = fieldPiece[0, Y];
                    fieldPiece[3, Y].x = 3;
                    fieldPiece[0, Y] = null;
                }
            }
        }
        // checks whether the move is en passant and performs it if it is.
        private bool enPassantCheck(int X, int Y, Canvas ChessBoard)
        {
            string colour = grabbedPiece.colour;
            if (Math.Abs(grabbedPiece.x - X) == 1 && fieldPiece[X, Y] == null && ((colour == "White" && grabbedPiece.y == 3) || (colour == "Black" && grabbedPiece.y == 4)))
            {
                if (X < grabbedPiece.x)
                {
                    if (colour == "White") Y += 1;
                    else Y -= 1;
                    pieceCapture(X, Y, ChessBoard); // removes the en passant pawn (it doesn't follow usual capture rules)
                    return true; // 
                }
                else
                {
                    if (colour == "White") Y += 1;
                    else Y -= 1;
                    pieceCapture(X, Y, ChessBoard); // removes the en passant pawn (it doesn't follow usual capture rules)
                    return true;
                }
            }
            else return false; // X & Y are not referenced
        }
        // checks whether there is a checkmate
        private bool CheckMate()
        {
            // means king has no movable spots, check for whether the checkmate can be stopped by another piece. All movements done on a vBoard.
            Board vBoard = new Board();
            vBoard.fieldPiece = (Pieces[,])fieldPiece.Clone();
            vBoard.turn = turn;
            foreach (Pieces piece in vBoard.fieldPiece)
            {
                if (piece != null && piece.colour == vBoard.turn)
                {
                    vBoard.grabbedPiece = piece;
                    vBoard.MovingRules();
                    foreach (bool spot in vBoard.movableSpots)
                    {
                        if (spot) return false;
                    }
                }
            }
            return true;
        }
        // checks whether there is a draw
        private int Draw()
        {
            Pieces grabbedPieceBackup = grabbedPiece;
            bool staleMate = true;
            int boardRepetitions = 0;
            if (!CheckKingCheck()) //stalemate
            {
                foreach (Pieces piece in fieldPiece)
                {
                    if (piece != null && piece.colour == turn)
                    {
                        grabbedPiece = piece; // locates the king we're checking for stalemate
                        MovingRules();
                        foreach (bool field in movableSpots)
                        {
                            if (field) staleMate = false; // if there is a movable spot, will conclude there is no stalemate.
                        }
                    }
                }
                if (staleMate) return 1;
            }
            foreach (PlayerBoard Board in undos) // threefold repetition rule
            {
                bool similar = true;
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        if (fieldPiece[i, j] == null && Board.fieldPiece[i,j] != null) similar = false;
                        else if (Board.fieldPiece[i, j] == null && fieldPiece[i, j] != null) similar = false;
                        else if (fieldPiece[i, j] != null && ((Board.fieldPiece[i, j].x != fieldPiece[i, j].x) || (Board.fieldPiece[i, j].y != fieldPiece[i, j].y) || (Board.fieldPiece[i, j].type != fieldPiece[i, j].type) || (Board.fieldPiece[i, j].colour != fieldPiece[i, j].colour))) similar = false;
                    }
                }
                if (similar) boardRepetitions++;
                if (boardRepetitions == 3) return 2;
            }
            if (drawBy50Count == 100) return 3; // the reason it is 100 is because a turn in this case is counted as a player completing a move followed by the enemy player. Therefore, double the count to account for that.
            grabbedPiece = grabbedPieceBackup;
            return 0;
        }
        //checks for check, checkmate or stalemate and updates relevant UI information.
        public int GameStatusCheck()
        {
            int DrawStatus = Draw(); // checks for draws
            if (DrawStatus > 0) //then it is a draw
            {
                if (DrawStatus == 1) return 3; //stalemate
                if (DrawStatus == 2) return 4; //threefold
                if (DrawStatus == 3) return 5; //50 moves
            }
            if (CheckKingCheck()) //if in check
            {
                if (CheckMate()) return 2; //if checkmate
                else return 1; // else just check
            }
            else return 0; // nothing, normal board state.
        }
        //makes a copy of the board.
        public Board DeepCopyBoard()
        {
            //Board copiedBoard = (Board) this.MemberwiseClone();
            Board copiedBoard = new Board();
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {

                    if (fieldPiece[i, j] == null)
                    {
                        copiedBoard.fieldPiece[i, j] = null;
                    }
                    else
                    {
                        copiedBoard.fieldPiece[i, j] = new Pieces(); // new piece to prevent referencing.
                        copiedBoard.fieldPiece[i, j].x = fieldPiece[i, j].x; // copies the contents of the saved undos array to piecesB.
                        copiedBoard.fieldPiece[i, j].y = fieldPiece[i, j].y;
                        copiedBoard.fieldPiece[i, j].type = fieldPiece[i, j].type;
                        copiedBoard.fieldPiece[i, j].colour = fieldPiece[i, j].colour;
                        copiedBoard.fieldPiece[i, j].hasmoved = fieldPiece[i, j].hasmoved;
                    }
                }
            }
            return copiedBoard;
        }
        // finishes the turn
        public void executeTurn(int X, int Y, Canvas ChessBoard)
        {
            clearPassant();
            grabbedPiece.hasmoved++; //adds one to the total moves of tha piece.
            drawBy50Count++; // draw by 50 move repetition. resets when piece is taken or pawn is moved.
            if (grabbedPiece.type == "P")
            {
                enPassantCheck(X, Y, ChessBoard); // code is made in a way that if en passant is checked afterward, it will attempt to capture 2 different spots twice, crashing, hence is checked first.
                drawBy50Count = 0;
            }
            if (fieldPiece[X, Y] != null) pieceCapture(X, Y, ChessBoard); // if the place being moved to is not empty, perform capture.
            if (grabbedPiece.type == "K") CastleCheck(X, Y); // if moved piece is a king, check whether the move is a castle.
            if (turn == "White") turn = "Black"; //piece has been moved, switch turns.
            else { turn = "White"; fullMove++; } // if black, add 1 to full move since black has just moved.
            grabbedPiece.x = (X);// updates the coordinates of the moved pece.
            grabbedPiece.y = (Y);
            SetPiecesToArray();
        }
        // clears en passant rights from pawns that have moved a turn.
        public void clearPassant()
        {
            foreach (var item in fieldPiece)
            {
                if (item != null && item.type == "P" && item.hasmoved > 0) item.canPassant = false;
            }
        }
        //saves board to file
        public void saveFEN(string fileName)
        {
            int emptyCounter = 0;
            string FEN = "";
            string pieces = "";
            string saveturn = turn.Substring(0, 1).ToLower();
            string whitecastle = "";
            string blackcastle = "";
            string totalcastle = "";
            string passant = "";

            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x <= 7; x++)
                {
                    if (fieldPiece[x, y] == null) emptyCounter++;
                    else
                    {
                        if (emptyCounter != 0) pieces += emptyCounter;
                        emptyCounter = 0;
                        if (fieldPiece[x, y].colour == "Black") pieces += fieldPiece[x, y].type.ToLower();
                        else pieces += fieldPiece[x, y].type.ToUpper();
                        if (fieldPiece[x, y].type == "K" && fieldPiece[x, y].hasmoved != 0 && fieldPiece[x, y].colour == "White") whitecastle = "-";
                        else if (fieldPiece[x, y].type == "K" && fieldPiece[x, y].hasmoved != 0 && fieldPiece[x, y].colour == "Black") blackcastle = "-";
                        else if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].hasmoved == 0 && fieldPiece[x, y].colour == "White" && fieldPiece[x, y].x == 0 && whitecastle != "-") whitecastle += "Q";
                        else if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].hasmoved == 0 && fieldPiece[x, y].colour == "White" && fieldPiece[x, y].x == 7 && whitecastle != "-") whitecastle += "K";
                        else if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].hasmoved == 0 && fieldPiece[x, y].colour == "Black" && fieldPiece[x, y].x == 0 && blackcastle != "-") blackcastle += "q";
                        else if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].hasmoved == 0 && fieldPiece[x, y].colour == "Black" && fieldPiece[x, y].x == 7 && blackcastle != "-") blackcastle += "k";

                        if (fieldPiece[x, y].type == "P" && fieldPiece[x, y].canPassant && fieldPiece[x, y].hasmoved == 1 && (y==3||y==4))
                        {
                            passant += Char.ConvertFromUtf32(x + 97);
                            if (fieldPiece[x, y].colour == "White") passant += 3;
                            else passant += 6;
                        }
                    }
                }
                if (emptyCounter != 0) pieces += emptyCounter;
                if (y != 0) pieces += "/";
                emptyCounter = 0;
            }
            if (passant == "") passant = "-";

            if (whitecastle == "QK") whitecastle = "KQ";
            if (blackcastle == "qk") blackcastle = "kq";
            if (whitecastle == "") whitecastle = "-";
            if (blackcastle == "") blackcastle = "-";
            if (whitecastle == "-" && blackcastle == "-") totalcastle = "-";
            else if (whitecastle == "-") totalcastle = blackcastle;
            else if (blackcastle == "-") totalcastle = whitecastle;
            else totalcastle = whitecastle + blackcastle;

            FEN = FEN + pieces + " " + saveturn + " " + totalcastle + " " + passant + " " + drawBy50Count + " " + fullMove;
            File.WriteAllText(@"saves\" + fileName + ".txt", FEN);
        }
        
        //loads using FEN files
        public void loadFEN(string loadName)
        {
            string loadData = File.ReadAllText(@"saves\" + loadName + ".txt");
            string[] loadParts = loadData.Split();
            string[] loadPieces = loadParts[0].Split('/');
            int skipCount = 0;
            int xCounter = 0; 
            bool wKingRook = false;
            bool wQueenRook = false;
            bool bKingRook = false;
            bool bQueenRook = false;
            foreach (char letter in loadParts[2])
            {
                if (letter == 'K') wKingRook = true;
                if (letter == 'Q') wQueenRook = true;
                if (letter == 'k') bKingRook = true;
                if (letter == 'q') bQueenRook = true;
            }
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x <= 7; x++)
                {
                    //loads pieces and skips if there was an empty number before until it runs out.
                    if (skipCount == 0)
                    {
                        char[] rowPieces = loadPieces[7 - y].ToCharArray();
                        string currentPiece = rowPieces[xCounter].ToString();
                        if (int.TryParse(currentPiece, out skipCount)) skipCount--; // checks if it's an int, if so set that as the ski
                        else
                        {
                            fieldPiece[x, y] = new Pieces();
                            fieldPiece[x, y].type = currentPiece.ToUpper();
                            if (fieldPiece[x, y].y != 1 || fieldPiece[x, y].y != 6) fieldPiece[x, y].canPassant = false;
                            if (Char.IsUpper(currentPiece, 0)) fieldPiece[x, y].colour = "White";
                            else fieldPiece[x, y].colour = "Black";
                            fieldPiece[x, y].x = x;
                            fieldPiece[x, y].y = y;
                            //adds a move to rooks, making them uncastlable if specified.
                            if (x == 0)
                            {
                                if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].colour == "White" && !wQueenRook) fieldPiece[x, y].hasmoved = 1;
                                if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].colour == "Black" && !bQueenRook) fieldPiece[x, y].hasmoved = 1;
                            }
                            if (x == 7)
                            {
                                if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].colour == "White" && !wKingRook) fieldPiece[x, y].hasmoved = 1;
                                if (fieldPiece[x, y].type == "R" && fieldPiece[x, y].colour == "Black" && !bKingRook) fieldPiece[x, y].hasmoved = 1;
                            }
                        };
                        xCounter++;
                    }
                    else skipCount--;
                }
                xCounter = 0;
            }
            if (loadParts[3] != "-")
            {
                int x = (int)loadParts[3][0] - 97;
                if (loadParts[3][1] == '6')
                {
                    fieldPiece[x, 3].canPassant = true;
                    fieldPiece[x, 3].hasmoved = 1;
                }
                else
                {
                    fieldPiece[x, 4].canPassant = true;
                    fieldPiece[x, 4].hasmoved = 1;
                }
            }
            if (loadParts[1] == "w") turn = "White";
            else turn = "Black";
            drawBy50Count = int.Parse(loadParts[4]);
            fullMove = int.Parse(loadParts[5]);
        }
        //deletes save
        public void deleteFEN(string deleteName)
        {
            File.Delete(@"saves\" + deleteName + ".txt");
        }
        public void deletePieces()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    fieldPiece[i, j] = null;
                }
            }
        }

    }
    public class PlayerBoard : Board
    {
        public string PlayerColour = "White";
        public bool ComputerOpponent = false;
        public void StartGame(Canvas ChessBoard)
        {
            resetBoard(ChessBoard);
            if (PlayerColour == "Black") ExecutePCTurn(ChessBoard);
        }
        // responsible for allowing the movement of pieces. moves them in the virtual box. Is ran after a click is made. Only does so for real players, not AI.
        public void executeGUITurn(Canvas ChessBoard)
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080;
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            Point cursor = Mouse.GetPosition(ChessBoard);
            int X = Convert.ToInt16(cursor.X - 100) / Convert.ToInt16(100 * wRatio);
            int Y = Convert.ToInt16(cursor.Y - 100) / Convert.ToInt16(100 * hRatio);
            if (X >= 0 && X < boardSize && Y >= 0 && Y < boardSize && (cursor.X-100)>=0 && (cursor.Y - 100) >= 0) // ensures mouse is within bounds of board to prevent out of bounds exception 
            {
                if (movableSpots[X, Y]) // ensures the spot can be moved to by the piece
                {
                    executeTurn(X, Y, ChessBoard);
                    if (grabbedPiece.type == "P") pawnConversionCheck(Y, ChessBoard); // checks whether the pawn is now at the end of the board, converting it if it is. purely for players.
                    SaveMove(); // saves the new board to the saved boards.
                    PerformGameEnd(); // checks whether there is a check, checkmate or stalemate.
                    ExecutePCTurn(ChessBoard);
                }
            } // if the piece can't move there, its coordinates do not change and it will allign to its original position
            AllignPieces();
            grabbedPiece = null;
        }
        // checks whether there is a pawn to be converted, if so, starts the conversion
        public bool pawnConversionCheck(int Y, Canvas ChessBoard)
        {
            if (Y == 0)
            {
                bool whiteExchange = true;
                Conversion(whiteExchange, ChessBoard);
                return true;
            }
            else if (Y == 7)
            {
                bool whiteExchange = false;
                Conversion(whiteExchange, ChessBoard);
                return true;
            }
            else return false;
        }
        public void ExecutePCTurn(Canvas ChessBoard)
        {
            if (ComputerOpponent && PlayerColour != turn)
            {
                EngineFunctions EngineGame = new EngineFunctions();
                PlayerBoard GameBoard = this;
                EngineGame.PerformPCMove(GameBoard, ChessBoard);
                AllignPieces();
                PerformGameEnd(); // checks whether there is a check, checkmate or stalemate.
                SaveMove(); // saves the new board to the saved boards.
                grabbedPiece = null;
                DeleteSprites(ChessBoard);
                RedrawSprites(ChessBoard);
            }
        }
        // Performs procedures depending on whether there is a check, checkmate or stalemate.
        public void PerformGameEnd()
        {
            MainWindow gameWindow = (MainWindow)Application.Current.MainWindow;
            int GameStatus = GameStatusCheck();
            if (GameStatus > 2)
            {
                grabbedPiece = null; // Since the window message box stops the code, we'd like the piece to still look nice and placed after the message comes up!
                AllignPieces();
                if (GameStatus == 3) //stalemate
                {
                    MessageBox.Show("It's a draw by stalemate!");
                    gameWindow.CheckStatus.Content = "Draw by stalemate!";
                }
                if (GameStatus == 4) // threefold
                {
                    MessageBox.Show("It's a draw by threefold repetition!");
                    gameWindow.CheckStatus.Content = "Draw by threefold repetition!";
                }
                if (GameStatus == 5) // 50 move draw
                {
                    MessageBox.Show("It's a draw by 50 moves!");
                    gameWindow.CheckStatus.Content = "Draw by the 50 move rule!";
                }
                turn = "GAMEOVER";
            }
            else if (GameStatus == 2)
            {
                grabbedPiece = null; // Since the window message box stops the code, we'd like the piece to still look nice and placed after the message comes up!
                AllignPieces();
                MessageBox.Show("Check mate! " + turn + " lost!");
                gameWindow.CheckStatus.Content = "Check mate! " + turn + " lost!";
                turn = "GAMEOVER";
            }
            else if (GameStatus == 1)
            {
                gameWindow.CheckStatus.Content = turn + " in Check!";
            }
            else gameWindow.CheckStatus.Content = null;
            if (GameStatus >= 2) ComputerOpponent = false;
        }
        // saves board states into a list to be able to be called when undoing a move (deep copy)
        public void SaveMove()
        {
            undos.Add(new PlayerBoard()); // adds a new board array to the list
            undoCount++; // keeps track of turns to accurately recall board states when undoing
            undos[undoCount].drawBy50Count = drawBy50Count; //updates the 50 move draw counter
            undos[undoCount].turn = turn;
            undos[undoCount].ComputerOpponent = ComputerOpponent;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {

                    if (fieldPiece[i, j] == null)
                    {
                        undos[undoCount].fieldPiece[i, j] = null;
                    }
                    else
                    {

                        undos[undoCount].fieldPiece[i, j] = new Pieces(); // new piece to prevent referencing.
                        undos[undoCount].fieldPiece[i, j].x = fieldPiece[i, j].x; // copies the contents of the saved undos array to piecesB.
                        undos[undoCount].fieldPiece[i, j].y = fieldPiece[i, j].y;
                        undos[undoCount].fieldPiece[i, j].type = fieldPiece[i, j].type;
                        undos[undoCount].fieldPiece[i, j].colour = fieldPiece[i, j].colour;
                        undos[undoCount].fieldPiece[i, j].hasmoved = fieldPiece[i, j].hasmoved;
                    }
                }
            }
        }
        // loads the board from the previous board state
        public void LoadUndo(Canvas ChessBoard)
        {
            DeleteSprites(ChessBoard); // deletes the current sprites
            undos.RemoveAt(undoCount); // removes the board state that will no longer be used
            undoCount--; // controls turn count to recall board states accurately
            turn = undos[undoCount].turn;
            ComputerOpponent = undos[undoCount].ComputerOpponent;
            drawBy50Count = undos[undoCount].drawBy50Count;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (undos[undoCount].fieldPiece[i, j] == null)
                    {
                        fieldPiece[i, j] = null;
                    }
                    else
                    {

                        fieldPiece[i, j] = new Pieces(); // to prevent referencing pieces from the list
                        fieldPiece[i, j].x = undos[undoCount].fieldPiece[i, j].x; // copies the contents of the saved undos array to piecesB.
                        fieldPiece[i, j].y = undos[undoCount].fieldPiece[i, j].y;
                        fieldPiece[i, j].type = undos[undoCount].fieldPiece[i, j].type;
                        fieldPiece[i, j].colour = undos[undoCount].fieldPiece[i, j].colour;
                        fieldPiece[i, j].hasmoved = undos[undoCount].fieldPiece[i, j].hasmoved;
                    }
                }
            }
            RedrawSprites(ChessBoard);
            PerformGameEnd(); //ensures that once the save is loaded, we can see whether the game is in check, checkmate etc.
        }
        // when called, draws a sprite for each piece in the array depending on its type and colour
        public void RedrawSprites(Canvas ChessBoard)
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080; // allows the sprites to be drawn in proportion to user's monitor resolution
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (fieldPiece[i, j] != null)
                    {
                        if (fieldPiece[i, j].type == "P" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whitepawn.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "P" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackpawn.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "N" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whiteknight.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "N" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackknight.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "B" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whitebishop.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "B" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackbishop.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "R" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whiterook.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "R" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackrook.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "Q" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whitequeen.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "Q" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackqueen.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "K" && fieldPiece[i, j].colour == "White")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/whiteking.png", UriKind.Relative)) };
                        if (fieldPiece[i, j].type == "K" && fieldPiece[i, j].colour == "Black")
                            fieldPiece[i, j].sprite = new Image() { Source = new BitmapImage(new Uri("Resources/blackking.png", UriKind.Relative)) };
                        fieldPiece[i, j].sprite.Height = 100 * hRatio; // sets the resolution of sprite to 100x100 to fit the squares.
                        fieldPiece[i, j].sprite.Width = 100 * wRatio;
                        ChessBoard.Children.Add(fieldPiece[i, j].sprite);
                        MainWindow gameWindow = (MainWindow)Application.Current.MainWindow;
                        fieldPiece[i, j].sprite.MouseDown += gameWindow.MainWindow_MouseDown;
                    }
                }
            }
            AllignPieces(); //alligns pieces so they look nice.
        }
        // finds ther square that has been clicked by searching through the array of the board squares and then comparing it to the clicked object.
        public void FindClickedSquare(object sender)
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (fieldPiece[i, j] != null && sender == fieldPiece[i, j].sprite) // ensures clicked piece is not empty to prevent out of bounds exception.
                    {
                        if (turn == fieldPiece[i, j].colour)// if the piece is not held, we pick up the newly clicked one and the turn matches to the clicked piece
                        {
                            grabbedPiece = fieldPiece[i, j];
                        }
                    }
                }
            }
        }
        //alligns the pieces to the center of each field so they look nice
        public void AllignPieces()
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080; // screen ratio, so the game is playable on different resolutions.
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            foreach (var piece in fieldPiece)
            {
                if (piece != null && piece.sprite != null) // to prevent it from attempting to allign piece when clicking at the start of the game (pre pieces placed)
                {
                    Canvas.SetLeft(piece.sprite, 100 + piece.x * 100 * wRatio); // resets the pieces to the piece's X coordinates in proportion to the grid (locked in center of square)
                    Canvas.SetTop(piece.sprite, 100 + piece.y * 100 * hRatio);
                }
            }
        }
        // deletes the sprites of all existing pieces, since when updating a sprite, the old one still stays unless removed.
        public void DeleteSprites(Canvas ChessBoard)
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (fieldPiece[i, j] != null)
                    {
                        ChessBoard.Children.Remove(fieldPiece[i, j].sprite);
                    }
                }
            }
        }
        // starts the conversion process of the pawn
        public void Conversion(bool whiteExchange, Canvas ChessBoard)
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080;
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            int X = grabbedPiece.x;
            int Y = grabbedPiece.y;
            grabbedPiece = null; //render no longer continues grabbing animation now.
            AllignPieces(); // alligns the pawn 
            SetPiecesToArray(); //and gives it its coordinates (X,Y) into the PiecesB array for us to use.
            Window1 exchange = new Window1(whiteExchange); //opens up the exchange window and gathers a return for the selected piece
            exchange.ShowDialog(); // ensure mainwindow doesn't run while selecting a piece
            ChessBoard.Children.Remove(fieldPiece[X, Y].sprite); // removes the old sprite 
            fieldPiece[X, Y].type = Window1.selectedExchange.type; // updates the selected pieces
            fieldPiece[X, Y].sprite = Window1.selectedExchange.sprite;
            ChessBoard.Children.Add(fieldPiece[X, Y].sprite); // adds the new sprite to canvas
            fieldPiece[X, Y].sprite.Width = 100 * wRatio; // changes the resolution of picture to fit the board.
            fieldPiece[X, Y].sprite.Height = 100 * hRatio; // changes the resolution of picture to fit the board.
            MainWindow gameWindow = (MainWindow)Application.Current.MainWindow;
            fieldPiece[X, Y].sprite.MouseDown += gameWindow.MainWindow_MouseDown; // adds it to the click event.
            exchange.Close();
        }
        // resets the board
        public void resetBoard(Canvas ChessBoard)
        {
            DeleteSprites(ChessBoard); // deletes the current sprites
            deletePieces();
            undos.Clear();
            undoCount = -1;
            fullMove = 1;
            turn = "White";
            CreateInitialPieces();
            RedrawSprites(ChessBoard);
            SaveMove();
        }
        //updates selection box for saves
        public void getSaves()
        {
            string[] saves = System.IO.Directory.GetFiles(@"saves\");
            MainWindow gameWindow = (MainWindow)Application.Current.MainWindow; 
            gameWindow.saveFiles.Items.Clear();
            foreach (string save in saves)
            { // goes through every file and adds their name to the listbox
                string croppedSave = Path.GetFileNameWithoutExtension(save);
                gameWindow.saveFiles.Items.Add(croppedSave);
            }
        }
    }
    public class EngineFunctions
    {
        private int boardSize = 8;
        private string EngineTurn = "Black";
        private string PlayerTurn = "White";
        public void PerformPCMove(PlayerBoard GameBoard, Canvas ChessBoard)
        {
            PlayerTurn = GameBoard.PlayerColour;
            if (PlayerTurn == "Black") EngineTurn = "White";
            int X = 0;
            int Y = 0;
            SimpleEngine(GameBoard, ChessBoard, ref X, ref Y);
            GameBoard.executeTurn(X, Y, ChessBoard); //executes the turn with the grabbed piece from SimpleEngine to the square with the updated X&Y
            PawnConversion(GameBoard, ChessBoard);
        }
        private void SimpleEngine(PlayerBoard GameBoard, Canvas ChessBoard, ref int X, ref int Y)
        {
            MainWindow gameWindow = (MainWindow)Application.Current.MainWindow;
            int nodeCount = 0;
            int RX = 0;
            int RY = 0;
            Pieces RgrabbedPiece = null;
            Board BoardCopy = GameBoard.DeepCopyBoard();
            int depth = Convert.ToInt16(gameWindow.depth.Value); // depth value
            int maxdepth = depth;
            var watch = System.Diagnostics.Stopwatch.StartNew(); //debug
            Max(BoardCopy, ChessBoard, ref RX, ref RY, ref RgrabbedPiece, depth, maxdepth, -1000000000, 1000000000, ref nodeCount); // depth fed in
            X = RX; // the X&Y coordinates of the space that the move is going to be made to
            Y = RY;
            int pX = RgrabbedPiece.x; // the x&y coordinates of the piece making the move.
            int pY = RgrabbedPiece.y;
            GameBoard.grabbedPiece = GameBoard.fieldPiece[pX, pY]; 
            TimeSpan turnTime = watch.Elapsed; //debug
            if (nodeCount != 0)
            {
                TimeSpan perMove = new TimeSpan(turnTime.Ticks / nodeCount); //debug
                gameWindow.Label1.Content = perMove; //debug
                gameWindow.Label2.Content = nodeCount; //debug
                gameWindow.Label3.Content = turnTime; //debug
            }
        }
        private int Max(Board BoardCopy, Canvas ChessBoard, ref int RX, ref int RY, ref Pieces RgrabbedPiece, int depth, int maxdepth, int alpha, int beta, ref int nodeCount)
        {
            int GameStatus = BoardCopy.GameStatusCheck();
            if (GameStatus == 2) return -10000000;
            else if (GameStatus >= 3) return 0;
            if (depth == 0) return BoardAnalysis(BoardCopy, ref nodeCount);
            Board Backup = BoardCopy.DeepCopyBoard(); // copies the current board so it can be restored
            int max = -1000000000; // sets max to the lowest possible value, any move is better than this so a move is eventually returned.
            Pieces[] pieces = ReturnPieces(BoardCopy, EngineTurn); // returns an array pieces that belong to the specified player
            foreach (Pieces piece in pieces) // goes through every piece.
            {
                BoardCopy.grabbedPiece = BoardCopy.fieldPiece[piece.x, piece.y]; // selects a piece that will be in play 
                int[,] moves = ReturnMoves(BoardCopy); // returns the moves in an array of the currently grabbed piece
                for (int i = 0; i < (moves.Length / 2); i++) // divided by 2 since there is double elements, to represent X & Y. Goes through every move.
                {
                    int X = moves[0, i]; // sets the X coordinate of the turn to be performed.
                    int Y = moves[1, i]; // sets the y coordinate of the turn to be performed.
                    BoardCopy.executeTurn(X, Y, ChessBoard); // executes the turn with the held piece.
                    PawnConversion(BoardCopy, ChessBoard); // converts a pawn to a queen if relevant.
                    int score = Min(BoardCopy, ChessBoard, ref RX, ref RY, ref RgrabbedPiece, depth - 1, maxdepth, alpha, beta, ref nodeCount); // asks min what is the best score it can give us.
                    if (score > alpha) alpha = score; //if the current best score is better than the best achievable score, update it.
                    if (beta < alpha) return score; // if the best min score is better than the best max score, this path is useless if player plays best, so it breaks
                    BoardCopy = Backup.DeepCopyBoard(); // returns the saved copy of the board.
                    BoardCopy.grabbedPiece = BoardCopy.fieldPiece[piece.x, piece.y]; // grabs the piece at the set coordinates again.
                    if (score > max) // if score is better than the current best score
                    {
                        max = score;
                        if (depth == maxdepth) // if at max depth, this is the main AI move
                        {
                            RX = X; // saves the coordinates of the best move.
                            RY = Y;
                            RgrabbedPiece = piece.CopyPiece(); // saves the best piece to move with.
                        }
                    }
                }
            }
            return max;
        }
        private int Min(Board BoardCopy, Canvas ChessBoard, ref int RX, ref int RY, ref Pieces RgrabbedPiece, int depth, int maxdepth, int alpha, int beta, ref int nodeCount)
        {
            int GameStatus = BoardCopy.GameStatusCheck();
            if (GameStatus == 2) return 10000000;
            else if (GameStatus >= 3) return 0;
            if (depth == 0) return BoardAnalysis(BoardCopy, ref nodeCount); // refer to the max algorithm, same thing but with no move savement at the end and applies to a different colour.
            Board Backup = BoardCopy.DeepCopyBoard();
            int min = 1000000000;
            Pieces[] pieces = ReturnPieces(BoardCopy, PlayerTurn);
            foreach (Pieces piece in pieces)
            {
                BoardCopy.grabbedPiece = BoardCopy.fieldPiece[piece.x, piece.y];
                int[,] moves = ReturnMoves(BoardCopy);
                for (int i = 0; i < (moves.Length / 2); i++)
                {
                    int X = moves[0, i];
                    int Y = moves[1, i];
                    BoardCopy.executeTurn(X, Y, ChessBoard);
                    PawnConversion(BoardCopy, ChessBoard);
                    int score = Max(BoardCopy, ChessBoard, ref RX, ref RY, ref RgrabbedPiece, depth - 1, maxdepth, alpha, beta, ref nodeCount);
                    if (score < beta) beta = score;
                    if (beta < alpha) return score;
                    BoardCopy = Backup.DeepCopyBoard();
                    BoardCopy.grabbedPiece = BoardCopy.fieldPiece[piece.x, piece.y];
                    if (score < min) min = score;
                }
            }
            return min;
        }
        private Pieces[] ReturnPieces(Board BoardCopy, string Turn)
        {
            int max = 0;
            foreach (Pieces piece in BoardCopy.fieldPiece) { if (piece != null && piece.colour == Turn) max++; }//states how many pieces of the needed colour our there.
            Pieces[] pieces = new Pieces[max]; 
            max = 0;
            foreach (Pieces piece in BoardCopy.fieldPiece) 
            {
                if (piece != null && piece.colour == Turn)
                {
                    pieces[max] = piece.CopyPiece(); //adds to array, in order starting at 0 
                    max++; 
                }
            }
            return pieces;
        }
        private int[,] ReturnMoves(Board BoardCopy)
        {
            BoardCopy.MovingRules();
            int max = 0;
            foreach (bool spot in BoardCopy.movableSpots) { if (spot) max++; } // counts the amount of movable fields there are.
            int[,] moves = new int[2, max];
            max = 0;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (BoardCopy.movableSpots[i, j])
                    {
                        moves[0, max] = i; // sets the X coordinate of the spot at 0 index.
                        moves[1, max] = j; // sets the Y coordinate of the spot at 1 index.
                        max++;
                    }
                }
            }
            return moves;
        }
        private int BoardAnalysis(Board BoardCopy, ref int nodeCount)
        {
            int minorPieceCount = 0;
            nodeCount++;
            int score = 0;
            foreach (Pieces piece in BoardCopy.fieldPiece)
            {
                if (piece != null)
                {
                    if (piece.type == "P")
                    {
                        if (piece.colour == EngineTurn)
                        {
                            score += 100;
                            if (piece.colour == "White") score += PawnTable[piece.x + piece.y * 8];
                            else score += PawnTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }
                        else
                        {
                            score -= 100;
                            if (piece.colour == "White") score -= PawnTable[piece.x + piece.y * 8];
                            else score -= PawnTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }

                    }
                    if (piece.type == "N")
                    {
                        if (piece.colour == EngineTurn)
                        {
                            score += 320;
                            if (piece.colour == "White") score += KnightTable[piece.x + piece.y * 8];
                            else score += KnightTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }
                        else
                        {
                            score -= 320;
                            if (piece.colour == "White") score -= KnightTable[piece.x + piece.y * 8];
                            else score -= KnightTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }
                        minorPieceCount++;
                    }
                    if (piece.type == "B")
                    {
                        if (piece.colour == EngineTurn)
                        {
                            score += 325;
                            if (piece.colour == "White") score += BishopTable[piece.x + piece.y * 8];
                            else score += BishopTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }
                        else
                        {
                            score -= 325;
                            if (piece.colour == "White") score -= BishopTable[piece.x + piece.y * 8];
                            else score -= BishopTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                        }
                        minorPieceCount++;
                    }
                    if (piece.type == "R")
                    {
                        if (piece.colour == EngineTurn) score += 500;
                        else score -= 500;
                        minorPieceCount++;
                    }
                    if (piece.type == "Q")
                    {
                        if (piece.colour == EngineTurn) score += 975;
                        else score -= 975;
                        minorPieceCount += 2;
                    }
                    if (piece.type == "K")
                    {
                        if (piece.colour == EngineTurn)
                        {
                            score += 32767;
                            if (minorPieceCount <= 2)
                            {
                                if (piece.colour == "White") score += KingTableEndGame[piece.x + piece.y * 8];
                                else score += KingTableEndGame[(7 - piece.x) + ((7 - piece.y) * 8)];
                            }
                            else
                            {
                                if (piece.colour == "White") score += KingTable[piece.x + piece.y * 8];
                                else score += BlackKingTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                            }
                        }
                        else
                        {
                            score -= 32767;
                            if (minorPieceCount <= 2)
                            {
                                if (piece.colour == "White") score -= KingTableEndGame[piece.x + piece.y * 8];
                                else score -= KingTableEndGame[(7 - piece.x) + ((7 - piece.y) * 8)];
                            }
                            else
                            {
                                if (piece.colour == "White") score -= KingTable[piece.x + piece.y * 8];
                                else score -= BlackKingTable[(7 - piece.x) + ((7 - piece.y) * 8)];
                            }
                        }
                    }
                }
            }
            return score;
        }
        // converts pawns to queens, i persume the queen to always be the best conversion.
        private void PawnConversion(Board GameBoard, Canvas ChessBoard)
        {
            for (int i = 0; i < boardSize; i++)
            {
                if (GameBoard.fieldPiece[i, 0] != null && GameBoard.fieldPiece[i, 0].type == "P") GameBoard.fieldPiece[i, 0].type = "Q";
                if (GameBoard.fieldPiece[i, 7] != null && GameBoard.fieldPiece[i, 7].type == "P") GameBoard.fieldPiece[i, 7].type = "Q";
            }
        }

        // Piece tables for analysis
        private static readonly short[] PawnTable = new short[]
        {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 10, 27, 27, 10,  5,  5,
         0,  0,  0, 25, 25,  0,  0,  0,
         5, -5,-10,  0,  0,-10, -5,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
        };
        private static readonly short[] KnightTable = new short[]
        {
         -50,-40,-30,-30,-30,-30,-40,-50,
         -40,-20,  0,  0,  0,  0,-20,-40,
         -30,  0, 10, 15, 15, 10,  0,-30,
         -30,  5, 15, 20, 20, 15,  5,-30,
         -30,  0, 15, 20, 20, 15,  0,-30,
         -30,  5, 11, 15, 15, 11,  5,-30,
         -40,-20,  0,  5,  5,  0,-20,-40,
         -50,-30,-20,-30,-30,-20,-30,-50,
        };
        private static readonly short[] BishopTable = new short[]
        {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-40,-10,-10,-40,-10,-20,
        };
        private static readonly short[] KingTable = new short[]
        {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
         20,  20,   0, -20, -20, -20,  20,  20,
         20,  30,  40, -20,   0, -20,  40,  20
        };
        private static readonly short[] BlackKingTable = new short[]
        {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
         20,  20, -20, -20, -20,   0,  20,  20,
         20,  40, -20,   0, -20,  40,  30,  20
        };
        private static readonly short[] KingTableEndGame = new short[]
        {
        -50,-40,-30,-20,-20,-30,-40,-50,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -50,-30,-30,-30,-30,-30,-30,-50
        };
    }
    public partial class MainWindow : Window
    {
        public PlayerBoard mBoard = new PlayerBoard();
        private const int boardSize = 8;
        Rectangle[,] boardSquares = new Rectangle[boardSize, boardSize];
        Rectangle previousSquare = null;
        public MainWindow()
        {
            InitializeComponent();
            CreateBoard();
        }
        //creates the actual board itself
        public void CreateBoard()
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080; // screen ratio, so the game is playable on different resolutions.
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    Rectangle square = new Rectangle // creates the board squares
                    {
                        Height = 100 * hRatio,
                        Width = 100 * wRatio,
                        StrokeThickness = 2
                    };
                    ChessBoard.Children.Add(square);
                    boardSquares[i, j] = square;
                    Canvas.SetLeft(boardSquares[i, j], 100 + (100 * i * wRatio)); // sets their coordinates with +100 to account for board edges.
                    Canvas.SetTop(boardSquares[i, j], 100 + (100 * j * hRatio));
                    if (i % 2 == 0) // if the x coord is odd in proportion to the board,
                        if (j % 2 != 0) boardSquares[i, j].Fill = Brushes.Peru; // then check if it's even in proportion to y, is it is, it is a black square
                        else boardSquares[i, j].Fill = Brushes.PeachPuff; // otherwise it's a white square
                    else
                        if (j % 2 != 0) boardSquares[i, j].Fill = Brushes.PeachPuff; // if x is even AND y is even, set the square to white
                    else boardSquares[i, j].Fill = Brushes.Peru; // else, set it to black
                    boardSquares[i, j].MouseDown += MainWindow_MouseDown; // add the squares as a clickable event.
                }
            }
            for (int i = 0; i < 8; i++) // the following for loops create all the labels with board coordinates
            {
                Label label = new Label();
                ChessBoard.Children.Add(label);
                char letter = (char)(65 + i); //converts to ascii characters A-H in capitals (A=65)
                label.Content = letter;
                label.FontSize = 50 * hRatio;
                label.FontFamily = new FontFamily("Comic Sans MS");
                Canvas.SetTop(label, 115 + 800 * hRatio);
                Canvas.SetLeft(label, 115 + (100 * i * wRatio));
            }
            for (int i = 7; i >= 0; i--)
            {
                Label label = new Label();
                ChessBoard.Children.Add(label);
                char letter = (char)(56 - i); //converts to ascii characters 1-8.
                label.Content = letter;
                label.FontSize = 50 * hRatio;
                label.FontFamily = new FontFamily("Comic Sans MS");
                Canvas.SetTop(label, 109 + (100 * i * hRatio));
                Canvas.SetLeft(label, 50);
            }
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            mBoard.getSaves();
        }
        //runs commands every frame, allows for animating the pieces by updating their location on the screen.
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            double hRatio = SystemParameters.FullPrimaryScreenHeight / 1080;
            double wRatio = SystemParameters.FullPrimaryScreenWidth / 1920;
            Point cursor = Mouse.GetPosition(this);
            if (mBoard.grabbedPiece != null)
            {
                Canvas.SetLeft(mBoard.grabbedPiece.sprite, cursor.X - 50 * wRatio); // alligns piece to centre of the mouse (halway of piece)
                Canvas.SetTop(mBoard.grabbedPiece.sprite, cursor.Y - 50 * hRatio); // alligns piece to centre of the mouse (halway of piece)
                Canvas.SetZIndex(mBoard.grabbedPiece.sprite, 129); //ensures held piece is in front of all the other pieces.
                int X = Convert.ToInt16(cursor.X - 100) / Convert.ToInt16(100 * wRatio);
                int Y = Convert.ToInt16(cursor.Y - 100) / Convert.ToInt16(100 * hRatio);
                if ((cursor.X - 100) < 0 || (cursor.Y - 100) < 0) { X = -1; Y = -1; } // if the coordinate is negative, due to rounding it rounds up to 0 so to counterract we manually set it to -1.
                if (X >= 0 && X < boardSize && Y >= 0 && Y < boardSize)
                {
                    if (mBoard.movableSpots[X, Y]) boardSquares[X, Y].Stroke = Brushes.Green;
                    else boardSquares[X, Y].Stroke = Brushes.Red;
                    if (previousSquare != null && previousSquare != boardSquares[X, Y])
                        previousSquare.Stroke = null;
                    previousSquare = boardSquares[X, Y];
                }
                else if (previousSquare != null) previousSquare.Stroke = null;
            }
            if (mBoard.undoCount > 0) Undo.IsEnabled = true; // following are responsbile for updating variables live as input boxes are edited.
            else Undo.IsEnabled = false;
            if (saveName.Text == "") FENSave.IsEnabled = false;
            else FENSave.IsEnabled = true;
            if (gameType.Text == "Player")
            {
                playerColour.IsEnabled = false;
                depth.IsEnabled = false;
            }
            else
            {
                playerColour.IsEnabled = true;
                depth.IsEnabled = true;
            }
            LabeDepth.Content = Convert.ToString(depth.Value); // updates the slider value
        }
        //the subroutine activated upon a mouseclick
        public void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (previousSquare != null) previousSquare.Stroke = null; //checks the previous square that has been highlited is not empty, and then sets it empty since a piece has been put down / picked up.
            if (mBoard.grabbedPiece != null) // if a piece is currently grabbed, means it will be put down, therefore we lock in the pieces coordinates with setPieceCoords and reduce it's Z index. (layer)
            {
                mBoard.executeGUITurn(ChessBoard);
                if (mBoard.grabbedPiece != null) Canvas.SetZIndex(mBoard.grabbedPiece.sprite, 128); // after pawn exchange, the grabbed piece becomes null and this allows the layers to still work.
            }
            else
            {
                mBoard.FindClickedSquare(sender);
            }
            if (mBoard.grabbedPiece != null) // sees whether a piece has been picked up, if picked up, check where it can move.
            {
                mBoard.MovingRules();
            }
        }
        // responsible for sending the requested exchange piece from other window, gets and sets it from the different window class
        public Pieces exchangedPiece { get; set; }
        // runs when the undo button is clicked
        private void Button_Click(object sender, RoutedEventArgs e) // undo button
        {
            mBoard.LoadUndo(ChessBoard);
        }
        // starts the game or resets it
        private void GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (gameType.Text == "Computer") mBoard.ComputerOpponent = true; // fetches the game properties
            else mBoard.ComputerOpponent = false;
            mBoard.PlayerColour = playerColour.Text;
            mBoard.StartGame(ChessBoard); // starts game
        }
        // saves the current board state
        private void FENSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = saveName.Text; 
            mBoard.saveFEN(fileName);
            mBoard.getSaves(); //updates to see new save in menu
        }
        // loads selected save
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string loadName = saveFiles.SelectedItem.ToString(); // the following functions are not stored within loadFEN since this is a board class function, not a playerboard function
            mBoard.DeleteSprites(ChessBoard); // clears all sprites so they do not duplicat
            mBoard.deletePieces();
            mBoard.loadFEN(loadName);
            if (gameType.Text == "Computer") mBoard.ComputerOpponent = true;
            else mBoard.ComputerOpponent = false;
            mBoard.PlayerColour = playerColour.Text; // following functions reset undos, clean up the board and ensure if the game has ended, it does end. 
            mBoard.undos.Clear();
            mBoard.undoCount = -1;
            mBoard.SaveMove();
            mBoard.RedrawSprites(ChessBoard);
            mBoard.AllignPieces();
            mBoard.PerformGameEnd();
            mBoard.ExecutePCTurn(ChessBoard);
        }
        // deletes selected save
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string deleteName = saveFiles.SelectedItem.ToString(); 
            mBoard.deleteFEN(deleteName); 
            mBoard.getSaves(); //resets the saves list so you can see the save disappear.
        }
    }
}