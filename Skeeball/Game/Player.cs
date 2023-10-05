using System.Collections.Generic;

namespace Skeeball;

public class Player
{
    public int BallsRemaining { get; private set; }
    public int Score { get; private set; }
    public int BallsThrown { get; private set; }

    public List<SkeeballGame.PointValue> BallScores { get; private set; } = new List<SkeeballGame.PointValue>();

    public SkeeballGame.PlayerPosition Position { get; set; }

    private const int DefaultBallsPerGame = 9;

    public Player()
    {
        Reset();
    }

    public void Reset(int ballsPerGame = DefaultBallsPerGame)
    {
        BallsRemaining = ballsPerGame;
        Score = 0;
        BallsThrown = 0;
        BallScores.Clear();
    }

    public void ThrowBall(SkeeballGame.PointValue pointValue, bool countScore = true)
    {
        BallsRemaining--;
        BallsThrown++;

        if (countScore)
        {
            Score += (int)pointValue;
        }
        BallScores.Add(pointValue);
    }

    public void AddBonusBall()
    {
        BallsRemaining++;
    }
}