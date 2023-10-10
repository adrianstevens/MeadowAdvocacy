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
        Bonus,
        /// <summary>
        /// 30 seconds to score as many points as possible
        /// </summary>
        Timed,
        /// <summary>
        /// Goal is to score 250 in as few throws as possible 
        /// </summary>
        Exact,
        /// <summary>
        /// The goal is to throw a 10, 20, 30, 40 and 50 in order in as few throws as possible
        /// </summary>
        Sequence,
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
        GameOver,
    }
}