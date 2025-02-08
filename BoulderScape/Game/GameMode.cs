namespace BoulderScape;

public class PuzzleGameMode
{
    PuzzleSet[] puzzleSets;

    public PuzzleGameMode(int size)
    {
        puzzleSets = new PuzzleSet[size];
    }

    public void SetNumberOfSets(int sets)
    {
        puzzleSets = new PuzzleSet[sets];
    }

    bool CheckLevel(int level)
    {
        if (puzzleSets == null)
        {
            return false;
        }

        if (puzzleSets.Length <= level || level < 0)
        {
            return false;
        }

        if (puzzleSets[level] == null)
        {
            return false;
        }

        return true;
    }

    bool CheckIndex(int level, int index)
    {
        if (CheckLevel(level) == false)
        {
            return false;
        }

        if (index >= puzzleSets[level].Count)
        {
            return false;
        }

        return true;
    }


    public int GetPercentComplete(int level)
    {
        if (CheckLevel(level) == false)
        { return 0; }
        return puzzleSets[level].GetPcnt();
    }

    public int GetNumSolved(int level)
    {
        if (CheckLevel(level) == false)
        {
            return 0;
        }
        return puzzleSets[level].GetNumSolved();
    }

    public int GetNumPuzzles(int level)
    {
        if (CheckLevel(level) == false)
        {
            return 0;
        }

        return puzzleSets[level].GetPuzzleCount();
    }

    public Puzzle GetPuzzle(int level, int index)
    {
        if (CheckIndex(level, index) == false)
        {
            return null;
        }

        return puzzleSets[level].GetPuzzle(index);
    }

    public PuzzleSet GetPuzzleSet(int level)
    {
        if (CheckLevel(level) == false)
        {
            return null;
        }

        return puzzleSets[level];
    }

    public int GetNumPuzzlesComplete(int level)
    {
        if (CheckLevel(level) == false)
        {
            return 0;
        }

        return puzzleSets[level].GetTotalPuzzlesComplete();
    }

    public bool AllPuzzlesSolved(int level)
    {
        if (CheckLevel(level) == false)
        {
            return false;
        }

        if (puzzleSets[level].GetPuzzleCount() == puzzleSets[level].GetNumSolved())
        {
            return true;
        }

        return false;
    }

    public int GetBestMoves(int level, int index)
    {
        if (CheckIndex(level, index) == false)
        {
            return 0;
        }

        return puzzleSets[level].GetScore(index);
    }

    public int GetMinMoves(int level, int index)
    {
        if (CheckIndex(level, index) == false)
        { return 0; }
        return puzzleSets[level].GetMinMoves(index);
    }

    public void SavePuzzles()
    {
        for (int i = 0; i < puzzleSets.Length; i++)
        {
            puzzleSets[i]?.Save();
        }
    }

    public int GetLevelCount()
    {
        if (puzzleSets != null)
        {
            return puzzleSets.Length;
        }
        return 0;
    }

    public bool LoadPuzzles(int level, string res)
    {
        if (level >= puzzleSets.Length)
        {
            return false;
        }

        if (puzzleSets[level] == null)
        {
            puzzleSets[level] = new PuzzleSet();
        }

        return puzzleSets[level].LoadPuzzles(res);
    }

    public bool IsLoaded()
    {
        if (puzzleSets == null)
        {
            return false;
        }

        if (puzzleSets.Length == 0)
        {
            return false;
        }

        if (puzzleSets[0] == null)
        {
            return false;
        }

        if (puzzleSets[0].Count == 0)
        {
            return false;
        }

        return true;
    }

    public void SolveAll(bool leaveLastPuz)
    {
        if (puzzleSets == null || puzzleSets.Length == 0)
        {
            return;
        }

        for (int i = 0; i < puzzleSets.Length; i++)
        {
            if (puzzleSets[i] == null)
            {
                break;
            }

            puzzleSets[i].SolveAll(leaveLastPuz);
        }
    }

    public void ResetAll()
    {
        if (puzzleSets == null || puzzleSets.Length == 0)
        {
            return;
        }

        for (int i = 0; i < puzzleSets.Length; i++)
        {
            if (puzzleSets[i] == null)
            {
                break;
            }

            puzzleSets[i].ResetAll();
        }
    }
}