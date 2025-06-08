namespace RogueLike;

internal class Enemy
{
    public int X { get; set; }
    public int Y { get; set; }

    public int VisionRange { get; set; } = 3;

    Direction lastDirection = Direction.Left;

    public void Move(Hero hero)
    {
        if(X == hero.X && Y == hero.Y) { return; }

        if(IsHeroInRange(hero))
        {
        }
    }

    bool IsHeroInRange(Hero hero)
    {
        if(Math.Abs(X - hero.X) <= VisionRange &&
            Math.Abs(Y - hero.Y) <= VisionRange)
        {
            return true;
        }
        return false;
    }
}
