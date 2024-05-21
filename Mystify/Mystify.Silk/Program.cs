using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Mystify.Core;

namespace Mystify_Silk;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static MystifyEngine? mystify;

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(320, 240, displayScale: 5f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        mystify = new MystifyEngine(display.Width, display.Height);

        mystify.NumberOfShapes = 3;
        mystify.PointsPerShape = 4;

        mystify.Initialize();
    }

    public static void Run()
    {
        if (mystify == null)
        {
            throw new Exception("Mystify isn't instantiated");
        }

        MystifyShape shape;

        Task.Run(() =>
        {
            while (true)
            {
                graphics.Clear();

                graphics.DrawText(10, 10, "Mystify", Color.White);
                // graphics.Show();
                // continue;

                //draw the shapes
                for (int j = 0; j < mystify.Shapes.Length; j++)
                {
                    shape = mystify.Shapes[j];

                    for (int i = 0; i < shape.Points.Length; i++)
                    {
                        graphics.DrawLineAntialiased(
                                     shape.Points[i].X,
                                     shape.Points[i].Y,
                                     shape.Points[(i + 1) % shape.Points.Length].X,
                                     shape.Points[(i + 1) % shape.Points.Length].Y,
                                     shape.Color);
                    }
                }

                graphics.Show();

                mystify.Update();

                Thread.Sleep(10);
            }
        });

        display!.Run();
    }


}

//<!=SNOP=>