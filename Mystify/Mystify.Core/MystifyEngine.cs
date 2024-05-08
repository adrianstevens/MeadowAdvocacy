using Meadow;
using Meadow.Foundation.Graphics;
using System;

namespace Mystify.Core;

public class MystifyEngine
{
    public int PointsPerShape { get; set; } = 4;
    public int NumberOfShapes { get; set; } = 3;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public MystifyShape[] Shapes { get; private set; }

    public MystifyEngine(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Initialize()
    {
        var random = new Random();

        Shapes = new MystifyShape[NumberOfShapes];

        for (int i = 0; i < Shapes.Length; i++)
        {
            //assign the points
            Shapes[i].Points = new Point[PointsPerShape];
            Shapes[i].XSpeed = new int[PointsPerShape];
            Shapes[i].YSpeed = new int[PointsPerShape];

            for (int j = 0; j < Shapes[i].Points.Length; j++)
            {
                Shapes[i].Points[j] = new Point(random.Next(Width), random.Next(Height));
                //assign the speed and ensure it'j not zero
                Shapes[i].XSpeed[j] = random.Next(-2, 2);
                Shapes[i].YSpeed[j] = random.Next(-2, 2);

                if (Shapes[i].XSpeed[j] == 0)
                {
                    Shapes[i].XSpeed[j] = 1;
                }
                if (Shapes[i].YSpeed[j] == 0)
                {
                    Shapes[i].YSpeed[j] = 1;
                }
            }
            //assign the color
            Shapes[i].Color = Color.FromRgb((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
        }
    }

    public void Update()
    {
        for (int j = 0; j < Shapes.Length; j++)
        {
            for (int i = 0; i < Shapes[j].Points.Length; i++)
            {
                Shapes[j].Points[i] = new Point(Shapes[j].Points[i].X + Shapes[j].XSpeed[i],
                                                Shapes[j].Points[i].Y + Shapes[j].YSpeed[i]);

                //bounce the corners off the walls
                if (Shapes[j].Points[i].X < 0 || Shapes[j].Points[i].X > Width)
                {
                    Shapes[j].XSpeed[i] = -Shapes[j].XSpeed[i];
                }
                if (Shapes[j].Points[i].Y < 0 || Shapes[j].Points[i].Y > Height)
                {
                    Shapes[j].YSpeed[i] = -Shapes[j].YSpeed[i];
                }
            }
        }
    }
}