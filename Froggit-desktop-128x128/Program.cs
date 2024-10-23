using Froggit;
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using SimpleJpegDecoder;
using System.Reflection;

namespace FroggitDesktop_Sample;

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static Keyboard keyboard = default!;

    static FrogItGame game;

    static IButton leftButton = default!;
    static IButton rightButton = default!;
    static IButton upButton = default!;
    static IButton downButton = default!;

    static GameState gameState = GameState.Ready;

    enum GameState
    {
        Ready,
        Playing,
        GameOver
    }

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(128, 128, displayScale: 4f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font12x16(),
            Stroke = 1
        };

        game = new FrogItGame(8, 128);
        game.Init(graphics);

        keyboard = new Keyboard();

        downButton = GetPushButton(keyboard.Pins.Down);

        rightButton = GetPushButton(keyboard.Pins.Right);

        upButton = GetPushButton(keyboard.Pins.Up);

        leftButton = GetPushButton(keyboard.Pins.Left);
    }

    public static void Run()
    {
        game.Reset();

        Task.Run(() =>
        {
            while (game.IsPlaying)
            {
                UpdateGame();
                Thread.Sleep(0);
            }
            gameState = GameState.GameOver;

            // DrawEndScreen();
        });

        display!.Run();
    }

    static void UpdateGame()
    {
        if (leftButton.State == true)
        {
            game.Left();
        }
        else if (rightButton.State == true)
        {
            game.Right();
        }
        else if (upButton.State == true)
        {
            game.Up();
        }
        else if (downButton.State == true)
        {
            game.Down();
        }

        game.Update();
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
        var resourceName = $"SilkDisplayTest.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}