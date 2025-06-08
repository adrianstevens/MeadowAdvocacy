namespace RogueLike;

public class RogueGame
{
    MapGenerator map;

    public Hero Hero { get; private set; }

    public Exit Exit { get; protected set; }

    public int Width => map.Width;
    public int Height => map.Height;    

    public TileType[,] MapTiles => map.MapTiles!;

    Random rand = new Random();

    public RogueGame()
    {
        map = new MapGenerator();

        Hero = new Hero();
        Exit = new Exit();
    }

    void LoadNewLevel()
    {
        map.GenerateMap();

        //put our hero on the map
   
        while (true)
        {
            int x = rand.Next(map.Width);
            int y = rand.Next(map.Height);

            if (map.MapTiles[x, y] == TileType.Room)
            {
                Hero.X = x;
                Hero.Y = y;
                break;
            }
        }

        while (true)
        {
            int x = rand.Next(map.Width);
            int y = rand.Next(map.Height);

            if (x == Hero.X && y == Hero.Y)
            {
                continue;
            }

            if (map.MapTiles[x, y] == TileType.Room)
            {
                Exit.X = x;
                Exit.Y = y;
                break;
            }
        }
    }

    public void NewGame()
    {
        LoadNewLevel();
    }

    void CheckEndState()
    {
        if (Hero.X == Exit.X &&
            Hero.Y == Exit.Y)
        {
            //load a new level
            LoadNewLevel();
        }
    }

    public void OnLeft()
    {
        if (map.MapTiles[Hero.X - 1, Hero.Y] != TileType.Blank)
        {
            Hero.X--;
            CheckEndState();
        }
    }

    public void OnRight()
    {
        if (map.MapTiles[Hero.X + 1, Hero.Y] != TileType.Blank)
        {
            Hero.X++;
            CheckEndState();
        }
    }

    public void OnUp()
    {
        if (map.MapTiles[Hero.X, Hero.Y - 1] != TileType.Blank)
        {
            Hero.Y--;
            CheckEndState();
        }
    }

    public void OnDown()
    {
        if (map.MapTiles[Hero.X, Hero.Y + 1] != TileType.Blank)
        {
            Hero.Y++;
            CheckEndState();
        }
    }
}
