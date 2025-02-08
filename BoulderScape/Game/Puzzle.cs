using System;
using System.Collections.Generic;

namespace BoulderScape;

public class Puzzle
{
    public List<PuzzlePiece> Pieces;
    public GameBoard board;

    public int MinimumMoves { get; set; } = -1;
    public int MoveCount { get; set; }

    public bool IsUnlocked { get; set; } = false;

    public int NumberOfBlocks => Pieces.Count;

    readonly Random rand = new();

    public Puzzle()
    {
        Pieces = new List<PuzzlePiece>();
        board = new GameBoard();
    }

    public Puzzle Clone()
    {
        var p = new Puzzle();

        foreach (PuzzlePiece piece in Pieces)
        {
            p.AddPiece(piece.X, piece.Y, piece.Piecetype);
        }

        return p;
    }

    public bool AddPiece(int x, int y, PieceType pieceType)
    {
        var p = new PuzzlePiece(x, y, pieceType);

        for (int hor = 0; hor < p.Width; hor++)
        {
            for (int vert = 0; vert < p.Height; vert++)
            {
                board.SetPiece(x + hor, y + vert);
            }
        }

        Pieces.Add(p);

        return true;
    }

    public void MovePiece(int xFrom, int yFrom, int xTo, int yTo)
    {
        PuzzlePiece p = Find(xFrom, yFrom);

        if (p == null)
        {
            return;
        }

        for (int hor = 0; hor < p.Width; hor++)
        {
            for (int vert = 0; vert < p.Height; vert++)
            {
                board.SetPiece(xFrom + hor, yFrom + vert, false);
            }
        }

        p.MovePiece(xTo, yTo);

        for (int hor = 0; hor < p.Width; hor++)
        {
            for (int vert = 0; vert < p.Height; vert++)
            {
                board.SetPiece(xTo + hor, yTo + vert, true);
            }
        }
    }

    public PuzzlePiece Find(int x, int y)
    {
        foreach (PuzzlePiece piece in Pieces)
        {
            if (piece.X == x && piece.Y == y)
            {
                return piece;
            }
        }

        return null;
    }

    public bool IsPuzzleSolved()
    {
        foreach (PuzzlePiece piece in Pieces)
        {
            if (piece.IsSolved)
            {
                return true;
            }
        }

        return false;
    }

    public bool CreateRandomPuzzle(int pieceCount)
    {
        Pieces.Clear();
        board.Clear();

        int count = 1;

        AddPiece(0, 2, PieceType.solve);

        int x, y;
        PieceType pieceType;

        int loopCount = 0;

        while (count < pieceCount)
        {
            pieceType = (PieceType)rand.Next((int)PieceType.solve);
            x = rand.Next(0, 6);
            y = rand.Next(0, 6);

            if (y == 2 &&
                (pieceType == PieceType.horizonal2 ||
                 pieceType == PieceType.horizontal3))
            {
                continue;
            }

            if (board.IsLocationFree(x, y, pieceType))
            {
                if (AddPiece(x, y, pieceType))
                {
                    count++;
                }
            }

            loopCount++;

            if (loopCount > 1000)
            {
                break;
            }
        }

        if (count < pieceCount)
        {
            Pieces.Clear();
            return false;
        }

        return true;
    }
}