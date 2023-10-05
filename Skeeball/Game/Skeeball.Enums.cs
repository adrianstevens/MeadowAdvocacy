namespace Skeeball;

public partial class SkeeballGame
{
    public enum PointValue
    {
        Ten = 10,
        Twenty = 20,
        Thirty = 30,
        Forty = 40,
        Fifty = 50
    }

    public enum GameMode
    {
        /// <summary>
        /// Classic skeeball, 9 balls, 10-50 points per throw
        /// </summary>
        Classic,
        /// <summary>
        /// Same rules as classic, but a bonus throw is awarded for a 50 point throw
        /// </summary>
        BonusBall,
        /// <summary>
        /// 30 seconds to score as many points as possible
        /// </summary>
        TimeAttack,
        /// <summary>
        /// As many throws as needed to score exactly 250 points
        /// Goal is to score 250 in as few throws as possible 
        /// </summary>
        PerfectScore,
        /// <summary>
        /// The goal is to get each possible point value in as few throws as possible
        /// </summary>
        CompleteSet,
    }

    public enum PlayerPosition
    {
        One = 1,
        Two,
        Three,
        Four,
    }

    public enum GameState
    {
        Initializing,
        ReadyToStart,
        Playing,
        GameOver
    }
}