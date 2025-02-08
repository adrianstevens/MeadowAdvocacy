namespace BoulderScape;

public enum PieceType
{
    horizonal2,
    horizontal3,
    vertical2,
    vertical3,
    solve
}

public class PuzzlePiece
{
    public int X { get; set; }
    public int Y { get; set; }
    public PieceType Piecetype { get; set; }
    public bool IsSolved => (Piecetype == PieceType.solve && X == 2 && Y == 4);

    public int Length => (IsHorizontal ? Width : Height);

    public bool IsHorizontal
    {
        get
        {
            return Piecetype switch
            {
                PieceType.horizonal2 or PieceType.horizontal3 => true,
                _ => false,
            };
        }
    }

    public int Width;
    public int Height;

    public PuzzlePiece()
    { }

    public PuzzlePiece(int x, int y, PieceType pieceType)
    {
        X = x;
        Y = y;

        Width = pieceType switch
        {
            PieceType.horizonal2 => 2,//case PieceType.solve:
            PieceType.horizontal3 => 3,
            _ => 1,
        };
        Height = pieceType switch
        {
            PieceType.vertical2 => 2,
            PieceType.solve => 2,
            PieceType.vertical3 => 3,
            _ => 1,
        };
    }

    public void MovePiece(int x, int y)
    {
        X = x;
        Y = y;
    }
}