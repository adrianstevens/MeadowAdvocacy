using Meadow;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using System;

namespace Froggit
{
    public partial class FrogItGame
    {
        const int cellSize = 16;

        MicroAudio moveAudio;
        MicroAudio effectsAudio;
        MicroGraphics graphics;

        public void Init(MicroGraphics gl, MicroAudio moveAudio, MicroAudio effectsAudio)
        {
            this.moveAudio = moveAudio;
            this.effectsAudio = effectsAudio;
            graphics = gl;

            InitBuffers();

            gl.Clear();
            gl.DrawRectangle(0, 0, gl.Width, gl.Height);
            gl.DrawText(3, 3, "Meadow Froggit");
            gl.DrawText(3, 16, "v0.7.5");

            gl.DrawBuffer(32, 32, frogUp);

            gl.Show();
        }

        public void Update()
        {
            UpdateFrame();

            graphics.Clear();
            DrawBackground(graphics);
            DrawLanesAndCheckCollisions(graphics);
            DrawFrog(graphics, frogState);
            // DrawLives();
            graphics.ShowUnsafe();
        }

        void DrawBackground(MicroGraphics graphics)
        {
            //draw docks
            for (int i = 0; i < FrogsHome; i++)
            {
                DrawFrog(12 + 24 * i, 0, FrogState.Forward, graphics);
            }

            //draw water
            //graphics.DrawRectangle(0, 16, 320, 48, CarColor, true);
        }

        void DrawLanesAndCheckCollisions(MicroGraphics graphics)
        {
            int startPos, index, x, y;
            int cellOffset;
            float offsetD;

            //cache the frog's lane row (-1 if not on a lane)
            int frogRow = (FrogY / cellSize) - 2;
            bool frogOnRow;

            for (byte row = 0; row < 8; row++)
            {
                startPos = (int)(GameTime * LaneSpeeds[row]) % LaneLength;
                offsetD = 16f * GameTime * LaneSpeeds[row];

                cellOffset = ((int)offsetD) % cellSize;

                if (startPos < 0)
                {
                    startPos = LaneLength - ((0 - startPos) % 32);
                }

                y = cellSize * (row + 2);
                frogOnRow = row == frogRow;

                //move the frog with the log
                if (frogOnRow && row < 3)
                {
                    FrogX -= (int)(TimeDelta * LaneSpeeds[row] * cellSize);
                    if (FrogX < 0 || FrogX > graphics.Width - cellSize)
                    {
                        KillFrog();
                    }
                }

                //iterate over every column in the lane
                for (byte i = 0; i < Columns + 2; i++)
                {
                    index = LaneData[row, (startPos + i) % LaneLength];

                    x = (i - 1) * cellSize - cellOffset;

                    if (index == 0)
                    {
                        if (frogOnRow && row < 3)
                        {
                            //if the frog is in the water, kill it
                            if (x >= FrogX - cellSize / 2 && x < FrogX + cellSize / 2)
                            {
                                KillFrog();
                            }
                        }
                        continue;
                    }

                    //if column is off screen, skip it
                    if (x < 0 || x >= graphics.Width - cellSize)
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
                            if (frogOnRow && x >= FrogX - cellSize && x < FrogX + cellSize) { KillFrog(); }
                            break;
                        case 5:
                        case 7:
                            DrawCar(x, y, index, graphics);
                            if (frogOnRow && x >= FrogX - cellSize && x < FrogX + cellSize) { KillFrog(); }
                            break;
                    }
                }
            }
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
            DrawFrog(FrogX, FrogY, state, graphics);
        }

        void DrawFrog(int x, int y, FrogState state, MicroGraphics graphics)
        {
            if (state == FrogState.Left)
            {
                graphics.DrawBufferWithTransparencyColor(x, y, frogLeft, Color.Black);
            }
            else if (state == FrogState.Forward)
            {
                graphics.DrawBufferWithTransparencyColor(x, y, frogUp, Color.Black);
            }
            else if (state == FrogState.Right)
            {
                graphics.DrawBufferWithTransparencyColor(x, y, frogRight, Color.Black);
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