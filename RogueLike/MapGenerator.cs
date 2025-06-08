namespace RogueLike;

public enum TileType : byte
{
    Blank,
    Wall,
    Room,
    Path
}

public enum Direction : byte
{
    Left,
    Right,
    Up, 
    Down,
}

internal class MapGenerator
{
    public int Width { get; set; } = 40;
    public int Height { get; set; } = 30;

    public int Rooms { get; set; } = 7;

    public int RoomMinDimension { get; set; } = 6;
    public int RoomMaxDimension { get; set; } = 12;

    public TileType[,]? MapTiles { get; set; }

    Random rand = new Random();

    

    public MapGenerator()
    {
        GenerateMap();
    }

    bool DefineRandomNewRoom()
    {
        //find a room width and height
        int w = rand.Next(RoomMaxDimension - RoomMinDimension) + RoomMinDimension;
        int h = rand.Next(RoomMaxDimension - RoomMinDimension) + RoomMinDimension;

        //find a random x,y coordinate ... stay off the edges
        int x = rand.Next(Width - w - 2) + 1;
        int y = rand.Next(Height - h - 2) + 1;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (MapTiles[x + i, y + j] != TileType.Blank ||
                    MapTiles[x + i + 1, y + j] != TileType.Blank ||
                    MapTiles[x + i - 1, y + j] != TileType.Blank ||
                    MapTiles[x + i, y + j + 1] != TileType.Blank ||
                    MapTiles[x + i, y + j - 1] != TileType.Blank ||
                    MapTiles[x + i + 1, y + j + 1] != TileType.Blank ||
                    MapTiles[x + i + 1, y + j - 1] != TileType.Blank ||
                    MapTiles[x + i - 1, y + j + 1] != TileType.Blank ||
                    MapTiles[x + i - 1, y + j - 1] != TileType.Blank)
                {
                    return false;
                }
            }
        }

        bool westPath = false;
        bool eastPath = false;
        bool northPath = false;
        bool southPath = false;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if(i == 0)
                {
                    MapTiles[x + i, y + j] = TileType.Wall;
                    if(westPath == false)
                    {
                        for (int k = x - 1; k > 0; k--)
                        {
                            if(MapTiles[k, y + j] == TileType.Wall)
                            {
                                //found a wall .... fill in the path
                                for (int p = x - 1; p > k; p--)
                                {
                                    MapTiles[p, y + j] = TileType.Path;
                                }
                                westPath = true;
                                break;
                            }
                        }
                    }
                }
                else if (i == w - 1)
                {
                    MapTiles[x + i, y + j] = TileType.Wall;
                    if (eastPath == false)
                    {
                        for (int k = x + i + 1; k < Width; k++)
                        {
                            if (MapTiles[k, y + j] == TileType.Wall)
                            {
                                //found a wall .... fill in the path
                                for (int p = x + i + 1; p < k; p++)
                                {
                                    MapTiles[p, y + j] = TileType.Path;
                                }
                                eastPath = true;
                                break;
                            }
                        }
                    }
                }
                else if (j == 0)
                {
                    MapTiles[x + i, y + j] = TileType.Wall;
                    if (northPath == false)
                    {
                        for (int k = y - 1; k > 0; k--)
                        {
                            if (MapTiles[x + i, k] == TileType.Wall)
                            {
                                //found a wall .... fill in the path
                                for (int p = y - 1; p > k; p--)
                                {
                                    MapTiles[x + i, p] = TileType.Path;
                                }
                                northPath = true;
                                break;
                            }
                        }
                    }
                }
                else if (j == h - 1)
                {
                    MapTiles[x + i, y + j] = TileType.Wall;
                    if (southPath == false)
                    {
                        for (int k = y + j + 1; k < Height; k++)
                        {
                            if (MapTiles[x + i, k] == TileType.Wall)
                            {
                                //found a wall .... fill in the path
                                for (int p = y + j + 1; p < k; p++)
                                {
                                    MapTiles[x + i, p] = TileType.Path;
                                }
                                southPath = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MapTiles[x + i, y + j] = TileType.Room;
                }
                
            }
        }

        return true;
    }

    public bool GenerateMap()
    {
        Clear();
        for (int r = 0; r < Rooms; r++)
        {
            while (DefineRandomNewRoom() == false) ; ;
        }
        Console.WriteLine("Rooms defined");

        return true;
    }

    void Clear ()
    {
        MapTiles = new TileType[Width, Height];
    }
}
