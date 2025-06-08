using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using RogueLike;
using SimpleJpegDecoder;
using System.Reflection;

namespace SilkDisplay_Image_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static Keyboard keyboard = default!;

    static TextDisplayMenu menu = default!;

    static RogueGame game = default!;

    static IButton left = default!;
    static IButton right = default!;
    static IButton up = default!;
    static IButton down = default!;

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(640, 480, displayScale: 1f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        keyboard = new Keyboard();

        down = GetPushButton(keyboard.Pins.Down);
        right = GetPushButton(keyboard.Pins.Right);
        up = GetPushButton(keyboard.Pins.Up);
        left = GetPushButton(keyboard.Pins.Left);

        down.Clicked += Down_Clicked;
        right.Clicked += Right_Clicked;
        up.Clicked += Up_Clicked;
        left.Clicked += Left_Clicked;
      
        game = new RogueGame();
        game.NewGame();
    }

    private static void Left_Clicked(object? sender, EventArgs e)
    {
        game.OnLeft();
    }

    private static void Up_Clicked(object? sender, EventArgs e)
    {
        game.OnUp();
    }

    private static void Right_Clicked(object? sender, EventArgs e)
    {
        game.OnRight();
    }

    private static void Down_Clicked(object? sender, EventArgs e)
    {
        game.OnDown();
    }

    public static void Run()
    {
        int scale = 16;

        graphics.Clear();

   
        Task.Run(() =>
        {
            while (true)
            {
                graphics.Clear();

                for (int i = 0; i < game.Width; i++)
                {
                    for (int j = 0; j < game.Height; j++)
                    {
                        if (game.MapTiles[i, j] == TileType.Room)
                        {
                            graphics.DrawRectangle(i * scale, j * scale, scale, scale, Color.LightGray, true);
                        }
                        if (game.MapTiles[i, j] == TileType.Wall)
                        {
                            graphics.DrawRectangle(i * scale, j * scale, scale, scale, Color.DarkGray, true);
                        }
                        if (game.MapTiles[i, j] == TileType.Path)
                        {
                            graphics.DrawRectangle(i * scale, j * scale, scale, scale, Color.Brown, true);
                        }
                    }
                }

                graphics.DrawRectangle(game.Hero.X * scale, game.Hero.Y * scale, scale, scale, Color.Cyan, true);
                graphics.DrawRectangle(game.Exit.X * scale, game.Exit.Y * scale, scale, scale, Color.Red, true);

                graphics.Show();

                Thread.Sleep(20);
            }
        });

        display!.Run();

    }

    private static IButton GetPushButton(IPin pin)
    {
        if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
        {
            return new PushButton(pin, ResistorMode.InternalPullDown);
        }
        else
        {
            return new PollingPushButton(pin, ResistorMode.InternalPullDown);
        }
    }

    private static void Menu_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Console.WriteLine($"Value changed for {e.ItemID} to {e.Value}");
    }

    static IPixelBuffer LoadJpeg()
    {
        var jpgData = LoadResource("maple.jpg");

        var decoder = new JpegDecoder();
        var jpg = decoder.DecodeJpeg(jpgData);

        Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");

        return new BufferRgb888(decoder.Width, decoder.Height, jpg);
    }

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"RogueLike.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

//<!=SNOP=>