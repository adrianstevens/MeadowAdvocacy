namespace BoulderScape;

public class GameBoard
{
    public bool[,] Board { get; private set; }

    public GameBoard()
    {
        Board = new bool[6, 6];
    }

    public bool IsPiece(int x, int y)
    {
        return Board[x, y];
    }

    public void SetPiece(int x, int y)
    {
        SetPiece(x, y, true);
    }

    public void SetPiece(int x, int y, bool b)
    {
        Board[x, y] = b;
    }

    public string GetHash()
    {
        string s = "";

        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                s += Board[x, y] == true ? "1" : "0";
            }
        }

        return s;
    }

    public bool IsLocationFree(ref PuzzlePiece piece)
    {
        return IsLocationFree(piece.X, piece.Y, piece.Piecetype);
    }

    public bool IsLocationFree(int x, int y, PieceType pieceType)
    {
        int length = 2;
        bool isHorizontal = true;

        if (pieceType == PieceType.horizontal3 || pieceType == PieceType.vertical3)
        {
            length = 3;
        }

        if (pieceType == PieceType.vertical2 || pieceType == PieceType.vertical3 || pieceType == PieceType.solve)
        {
            isHorizontal = false;
        }

        if (isHorizontal == true)
        {   //make sure we're on the Board
            if (x + length > 6)
            {
                return false;
            }

            if (x < 0)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (Board[x + i, y] == true)
                {
                    return false;
                }
            }
        }
        else
        {   //make sure we're on the Board
            if (y + length > 6)
            {
                return false;
            }

            if (y < 0)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (Board[x, y + i] == true)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void Clear()
    {
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                Board[x, y] = false;
            }
        }
    }
}