using Meadow.Foundation.Graphics;
using System;

namespace Froggit
{
    public partial class FrogItGame
    {
        readonly byte cellSize = 16;

        public void Init(MicroGraphics gl)
        {
            InitBuffers();

            gl.Clear();
            gl.DrawRectangle(0, 0, gl.Width, gl.Height);
            gl.DrawText(3, 3, "Meadow FrogIt");
            gl.DrawText(3, 16, "v0.6.0");

            gl.DrawBuffer(32, 32, frogUp);

            gl.Show();
        }

        public void Update(MicroGraphics gl)
        {
            Update();

            gl.Clear();
            DrawBackground(gl);
            DrawLanesAndCheckCollisions(gl);
            DrawFrog(gl, frogState);
            // DrawLives();
            gl.Show();
        }

        void DrawBackground(MicroGraphics graphics)
        {
            //draw docks
            for (int i = 0; i < 5; i++)
            {
                if (i < FrogsHome)
                {
                    DrawFrog(12 + 24 * i, 0, FrogState.Forward, graphics);
                }
            }

            //draw water
            //graphics.DrawRectangle(0, 16, 320, 48, CarColor, true);
        }

        void DrawLanesAndCheckCollisions(MicroGraphics graphics)
        {
            int startPos, index, x, y;
            int cellOffset;

            double offsetD;

            for (byte row = 0; row < 8; row++)
            {
                startPos = (int)(GameTime * LaneSpeeds[row]) % LaneLength;
                offsetD = 16.0 * GameTime * LaneSpeeds[row];

                cellOffset = ((int)offsetD) % cellSize;

                if (startPos < 0)
                {
                    startPos = LaneLength - ((0 - startPos) % 32);
                }

                y = cellSize * (row + 2);

                //move the frog with the log
                if (row < 3 && y == FrogY)
                {
                    FrogX -= TimeDelta * LaneSpeeds[row] * CellSize;
                }

                //iterate over ever column in the lane
                for (byte i = 0; i < Columns + 2; i++)
                {
                    index = LaneData[row, (startPos + i) % LaneLength];

                    x = (i - 1) * cellSize - cellOffset;

                    //if the frog is on the log and goes off screen, kill it
                    if (index == 0)
                    {
                        if (row < 3)
                        {
                            if (IsFrogCollision(x, y) == true)
                            {
                                KillFrog();
                            }
                        }
                        continue;
                    }

                    //if column is off screen, skip it
                    if (x < 0 || x >= graphics.Width - CellSize)
                    {
                        continue;
                    }


                    switch (row)
                    {
                        case 0:
                        case 1:
                        case 2:
                            DrawLog(x, y, index, graphics);
                            break;
                        case 3: //sidewalk
                            break;
                        case 4:
                        case 6:
                            DrawTruck(x, y, index, graphics);
                            if (IsFrogCollision(x, y)) { KillFrog(); }
                            break;
                        case 5:
                        case 7:
                            DrawCar(x, y, index, graphics);
                            if (IsFrogCollision(x, y)) { KillFrog(); }
                            break;
                    }
                }
            }
        }

        bool IsFrogCollision(int x, int y)
        {
            if (y == FrogY &&
                x > FrogX &&
                x < FrogX + cellSize)
            {
                return true;
            }
            return false;
        }

        void DrawLives(MicroGraphics graphics)
        {
            for (int i = 1; i < Lives; i++)
            {
                DrawFrog(cellSize * (Columns - i), cellSize * (Rows - 1), FrogState.Forward, graphics);
            }
        }

        void DrawFrog(MicroGraphics graphics, FrogState state = FrogState.Forward)
        {
            DrawFrog((int)Math.Truncate(FrogX), (int)Math.Truncate(FrogY), state, graphics);
        }

        void DrawFrog(int x, int y, FrogState state, MicroGraphics graphics)
        {
            if (state == FrogState.Left)
            {
                graphics.DrawBuffer(x, y, frogLeft);
            }
            else if (state == FrogState.Forward)
            {
                graphics.DrawBuffer(x, y, frogUp);
            }
            else if (state == FrogState.Right)
            {
                graphics.DrawBuffer(x, y, frogRight);
            }
            else
            {
                graphics.DrawText(x, y, "X");
            }
        }

        void DrawTruck(int x, int y, int index, MicroGraphics graphics)
        {
            if (index == 1) graphics.DrawBuffer(x, y, truckLeft);
            else if (index == 2) graphics.DrawBuffer(x, y, truckCenter);
            else if (index == 3) graphics.DrawBuffer(x, y, truckRight);
        }

        void DrawLog(int x, int y, int index, MicroGraphics graphics)
        {
            if (index == 1) graphics.DrawBuffer(x, y, logDarkLeft);
            else if (index == 2) graphics.DrawBuffer(x, y, logDarkCenter);
            else if (index == 3) graphics.DrawBuffer(x, y, logDarkRight);
        }

        void DrawCar(int x, int y, int index, MicroGraphics graphics)
        {
            if (index == 1) graphics.DrawBuffer(x, y, carLeft);
            else if (index == 2) graphics.DrawBuffer(x, y, carRight);
        }
    }
}