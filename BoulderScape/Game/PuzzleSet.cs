using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BoulderScape;

public class PuzzleSet
{
    public string FileName
    {
        get => FileManager.GetFileInStorage(_fileName);
        set => _fileName = value;
    }
    string _fileName;

    readonly List<Puzzle> puzzles = new();

    int lastSolved = 0;
    int currentLevel = 0;
    int highestPuzzleUnlocked = 0;

    byte[] scores; //we'll declare this when we load the puzzles

    bool changed = true; //do we need to save?

    public int Count => puzzles.Count;

    public PuzzleSet()
    {
    }

    public bool LoadPuzzles(string FileName)
    {
        Console.WriteLine("Load Puzzles: " + FileName);

        this.FileName = FileName;

        string szFile = "Puzzles/" + FileName + ".txt";

        if (File.Exists(szFile) == false)
        {
            return false;
        }

        puzzles.Clear();

        StreamReader sr;

        try
        {
            var f = File.OpenRead(FileManager.GetFileInApp(szFile));

            sr = new StreamReader(f, true);
        }
        catch
        {
            return false;
        }

        using (BinaryReader r = new(sr.BaseStream))
        {
            int iCount = r.ReadInt32();
            int iPieceCount = 0;

            var piece = new PuzzlePiece();

            for (int i = 0; i < iCount; i++)
            {
                var puzzle = new Puzzle
                {
                    MinimumMoves = r.ReadInt32()
                };

                iPieceCount = r.ReadInt32();

                for (int j = 0; j < iPieceCount; j++)
                {
                    //to rotate
                    piece.Piecetype = SwitchPieceType((PieceType)r.ReadInt32());
                    piece.Y = r.ReadInt32();
                    piece.X = r.ReadInt32();

                    puzzle.AddPiece(piece.X, piece.Y, piece.Piecetype);
                }

                puzzles.Add(puzzle);
            }

            sr.Close();
        }

        LoadScores(this.FileName + ".sc");
        LoadSettings();

        return true;
    }

    private bool SaveSettings()
    {
        var fileName = FileName + ".set";

        try
        {
            FileStream fs = File.OpenWrite(fileName);

            var bin = new BinaryWriter(fs);

            bin.Write(lastSolved);
            bin.Write(currentLevel);
            bin.Write(highestPuzzleUnlocked);

            for (int i = 0; i < puzzles.Count; i++)
            {
                bin.Write(puzzles[i].IsUnlocked);
            }

            bin.Flush();
            bin.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("PuzzleSet.SaveSettings - " + ex.Message);
        }

        return true;
    }

    private bool LoadSettings()
    {
        var fileName = FileName + ".set";

        if (File.Exists(fileName) == false)
        {
            return false;
        }

        try
        {
            FileStream fs = File.OpenRead(fileName);

            using BinaryReader r = new(fs);
            try { lastSolved = r.ReadInt32(); } catch { }
            try { currentLevel = r.ReadInt32(); } catch { }
            try { highestPuzzleUnlocked = r.ReadInt32(); } catch { }

            try
            {
                for (int i = 0; i < puzzles.Count; i++)
                {
                    puzzles[i].IsUnlocked = r.ReadBoolean();
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            Console.WriteLine("PuzzleSet.LoadSettings - " + ex.Message);
        }
        changed = true;

        return true;
    }

    //scores as in moves
    private bool LoadScores(string FileName)
    {
        //reset the byte array
        scores = new byte[puzzles.Count];

        for (int i = 0; i < puzzles.Count; i++)
        {
            scores[i] = 0;
        }

        FileStream file;

        try
        {
            file = new FileStream(FileName, FileMode.Open);
        }
        catch
        {
            return false;
        }

        using var r = new BinaryReader(file);
        int count = r.ReadInt32();

        if (count > scores.Length)
        {
            count = scores.Length;
        }

        for (int i = 0; i < count; i++)
        {
            scores[i] = r.ReadByte();
        }
        return true;
    }

    private bool SaveScores(string fileName)
    {
        if (scores == null || scores.Length == 0)
        {
            return false;
        }

        var bin = new BinaryWriter(new FileStream(fileName, FileMode.Create));

        bin.Write(scores.Length);

        foreach (byte b in scores)
        {
            bin.Write(b);
        }

        bin.Close();

        return true;
    }

    public Puzzle GetPuzzle(int iIndex)
    {
        if (CheckIndex(iIndex) == false)
        {
            return null;
        }

        currentLevel = iIndex;
        highestPuzzleUnlocked = Math.Max(currentLevel, highestPuzzleUnlocked);

        return puzzles[iIndex];
    }

    public bool HasPuzBeenSolved(int iIndex)
    {
        if (CheckIndex(iIndex) == false)
        {
            return false;
        }

        if (scores != null && scores.Length > iIndex)
        {
            if (scores[iIndex] > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateScore()
    {
        UpdateScore(GetCurrentIndex(), GetPuzzle(GetCurrentIndex()).MoveCount);
        changed = true;
    }

    public int UpdateScore(int iIndex, int iMoves)
    {
        changed = true;

        if (scores[iIndex] == 0 || iMoves < scores[iIndex])
        {
            if (iMoves != 0)
            {
                scores[iIndex] = (byte)iMoves;
            }
        }

        lastSolved = iIndex;

        SaveScores(FileName + ".sc");

        return scores[iIndex];
    }

    public double GetPercentage()
    {
        if (scores == null || scores.Length == 0)
        {
            return 0;
        }

        int count = 0;

        foreach (byte b in scores)
        {
            if (b > 0)
            {
                count++;
            }
        }

        return 100.0 * count / scores.Length;
    }

    public int GetCurrentIndex() => currentLevel;

    public void SetCurrentIndex(int level)
    {
        changed = true;

        if (level < 0 || level >= puzzles.Count)
        {
            return;
        }

        currentLevel = level;
        highestPuzzleUnlocked = Math.Max(currentLevel, highestPuzzleUnlocked);
    }

    public int GetTotalPuzzlesComplete()
    {
        if (scores == null || scores.Length == 0)
        {
            return 0;
        }

        int iTotal = 0;

        for (int i = 0; i < scores.Length; i++)
        {
            iTotal++;

        }
        return iTotal;
    }

    public bool SetScore(int index, int score)
    {
        changed = true;

        if (CheckIndex(index) == false)
        {
            return false;
        }

        if (score == 0)
        {
            return false;
        }

        byte scoreAsByte = (byte)score;

        if (score >= 255)
        {
            scoreAsByte = 255;
        }

        if (scores == null || index > scores.Length)
        {
            scores = new byte[puzzles.Count];
        }

        scores[index] = scoreAsByte;

        lastSolved = index;

        SaveScores(FileName + ".sc");

        return true;
    }

    private bool CheckIndex(int iIndex)
    {
        if (iIndex < 0)
        {
            return false;
        }

        if (iIndex >= puzzles.Count)
        {
            return false;
        }

        return true;
    }

    private PieceType SwitchPieceType(PieceType p)
    {
        return p switch
        {
            PieceType.horizonal2 => PieceType.vertical2,
            PieceType.horizontal3 => PieceType.vertical3,
            PieceType.vertical2 => PieceType.horizonal2,
            PieceType.vertical3 => PieceType.horizontal3,
            _ => p,
        };
    }

    public void Save()
    {
        if (changed == false)
        {
            return;
        }

        SaveScores(FileName + ".sc");
        SaveSettings();

        changed = false;
    }

    public int GetPcnt()
    {
        return 100 * GetNumSolved() / GetPuzzleCount();
    }

    public int GetHighestIndexPlayed()
    {
        int ret = 0;

        for (int i = 0; i < GetPuzzleCount(); i++)
        {
            if (GetScore(i) > 0)
            {
                ret = i;
            }
        }

        return ret;
    }

    public int GetNumSolved()
    {
        if (highestPuzzleUnlocked == 0)
        {
            return 0;
        }

        int solved = 0;

        for (int i = 0; i < GetPuzzleCount(); i++)
        {
            if (GetScore(i) > 0)
            {
                solved++;
            }
        }

        return solved;
    }

    public int GetPuzzleCount() => puzzles.Count;

    public int GetMinMoves(int iIndex)
    {
        return puzzles[iIndex].MinimumMoves;
    }

    public int GetScore(int iIndex)
    {
        if (scores == null || iIndex > scores.Length)
        {
            return 999;
        }

        return scores[iIndex];
    }

    public bool UnlockPuzzle(int index
        )
    {
        changed = true;

        index--;

        if (index < 0 ||
           puzzles == null ||
           puzzles.Count <= index)
        {
            return false;
        }

        if (puzzles[index].IsUnlocked == true)
        {
            return false;
        }

        puzzles[index].IsUnlocked = true;
        SaveSettings();
        return true;
    }

    public bool SolveAll(bool leaveLastPuz)
    {
        if (puzzles == null || puzzles.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < puzzles.Count - 1; i++)
        {
            SetScore(i, puzzles[i].MinimumMoves);
        }

        int index = puzzles.Count - 1;
        var p = puzzles[index];

        if (leaveLastPuz == true)
        {
            p.MoveCount = 0;
            p.IsUnlocked = true;
            scores[index] = 0;
        }
        else
        {
            SetScore(index, p.MinimumMoves);
        }
        return true;
    }

    public bool ResetAll()
    {
        if (puzzles == null || puzzles.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < puzzles.Count; i++)
        {
            puzzles[i].MoveCount = 0;
            puzzles[i].IsUnlocked = false;
            scores[i] = 0;
        }

        return true;
    }
}