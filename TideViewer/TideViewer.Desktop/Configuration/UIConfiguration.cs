namespace TideViewer.Desktop.Configuration;

/// <summary>
/// UI layout configuration
/// </summary>
public class UIConfiguration
{
    public ColumnPositions ColumnPositions { get; set; } = new();
    public TideGraphSettings TideGraph { get; set; } = new();
}

public class ColumnPositions
{
    public int Col1 { get; set; } = 40;
    public int Col2 { get; set; } = 180;
}

public class TideGraphSettings
{
    public int X { get; set; } = 274;
    public int Y { get; set; } = 200;
    public int Width { get; set; } = 322;
    public int Height { get; set; } = 245;
}
