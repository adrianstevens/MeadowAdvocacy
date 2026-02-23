using System;

namespace FallingSand.Core
{
    public class FallingSandEngine
    {
        public const int Cols = 160;
        public const int Rows = 120;
        private const int GridSize = Cols * Rows;
        private const int MaxParticles = 3000;

        // Flat arrays for cache-friendly access
        public readonly byte[] Grid = new byte[GridSize];
        public readonly byte[] Shade = new byte[GridSize];

        public int GravX { get; private set; } = 0;
        public int GravY { get; private set; } = 1;
        public int ParticleCount { get; private set; }

        private readonly Random _rand = new Random();

        public void Initialize(int particleCount = 1500)
        {
            for (int i = 0; i < particleCount; i++)
            {
                int x = _rand.Next(Cols);
                int y = _rand.Next(Rows);
                int idx = y * Cols + x;
                if (Grid[idx] == 0)
                {
                    Grid[idx] = 1;
                    Shade[idx] = (byte)_rand.Next(8);
                    ParticleCount++;
                }
            }
        }

        // ax, ay are gravity components in g — positive ax tilts right, positive ay tilts forward/down
        public void SetGravity(float ax, float ay)
        {
            float absX = ax < 0 ? -ax : ax;
            float absY = ay < 0 ? -ay : ay;

            if (absX < 0.15f && absY < 0.15f)
                return; // flat enough — keep current gravity

            int newGX, newGY;
            if (absX >= absY * 2f)
            {
                // predominantly horizontal
                newGX = ax > 0 ? 1 : -1;
                newGY = 0;
            }
            else if (absY >= absX * 2f)
            {
                // predominantly vertical
                newGX = 0;
                newGY = ay > 0 ? 1 : -1;
            }
            else
            {
                // diagonal ~45°
                newGX = ax > 0 ? 1 : -1;
                newGY = ay > 0 ? 1 : -1;
            }

            GravX = newGX;
            GravY = newGY;
        }

        public void SpawnParticles(int count = 2)
        {
            if (ParticleCount >= MaxParticles) return;

            int gx = GravX, gy = GravY;

            for (int i = 0; i < count && ParticleCount < MaxParticles; i++)
            {
                int x, y;
                if (gy > 0)      { x = _rand.Next(1, Cols - 1); y = 0; }
                else if (gy < 0) { x = _rand.Next(1, Cols - 1); y = Rows - 1; }
                else if (gx > 0) { x = 0;        y = _rand.Next(1, Rows - 1); }
                else             { x = Cols - 1; y = _rand.Next(1, Rows - 1); }

                int idx = y * Cols + x;
                if (Grid[idx] == 0)
                {
                    Grid[idx] = 1;
                    Shade[idx] = (byte)_rand.Next(8);
                    ParticleCount++;
                }
            }
        }

        public void Update()
        {
            int gx = GravX;
            int gy = GravY;

            // Iterate against gravity direction so moved particles aren't processed twice
            int xStart, xEnd, xStep;
            int yStart, yEnd, yStep;

            if (gx > 0)      { xStart = Cols - 1; xEnd = -1;   xStep = -1; }
            else if (gx < 0) { xStart = 0;        xEnd = Cols;  xStep = 1;  }
            else             { xStart = 0;         xEnd = Cols;  xStep = 1;  }

            if (gy > 0)      { yStart = Rows - 1; yEnd = -1;   yStep = -1; }
            else if (gy < 0) { yStart = 0;         yEnd = Rows;  yStep = 1;  }
            else             { yStart = 0;          yEnd = Rows;  yStep = 1;  }

            // Per-frame diagonal bias to prevent directional artifacts
            int diag = _rand.Next(2) == 0 ? 1 : -1;

            for (int y = yStart; y != yEnd; y += yStep)
            {
                int rowOffset = y * Cols;
                for (int x = xStart; x != xEnd; x += xStep)
                {
                    int idx = rowOffset + x;
                    if (Grid[idx] != 1) continue;

                    // Primary: move in gravity direction
                    if (TryMove(idx, x + gx, y + gy)) continue;

                    // Secondary: spread perpendicular to gravity
                    if (gy != 0 && gx == 0)
                    {
                        // Pure vertical — spread sideways while falling
                        if (!TryMove(idx, x + diag, y + gy))
                            TryMove(idx, x - diag, y + gy);
                    }
                    else if (gx != 0 && gy == 0)
                    {
                        // Pure horizontal — spread vertically while sliding
                        if (!TryMove(idx, x + gx, y + diag))
                            TryMove(idx, x + gx, y - diag);
                    }
                    else
                    {
                        // Diagonal gravity — try each axis independently
                        if (diag > 0)
                        {
                            if (!TryMove(idx, x + gx, y))
                                TryMove(idx, x, y + gy);
                        }
                        else
                        {
                            if (!TryMove(idx, x, y + gy))
                                TryMove(idx, x + gx, y);
                        }
                    }
                }
            }
        }

        private bool TryMove(int fromIdx, int toX, int toY)
        {
            if ((uint)toX >= Cols || (uint)toY >= Rows) return false;

            int toIdx = toY * Cols + toX;
            if (Grid[toIdx] != 0) return false;

            byte shade = Shade[fromIdx];
            Grid[fromIdx] = 0;
            Shade[fromIdx] = 0;
            Grid[toIdx] = 1;
            Shade[toIdx] = shade;

            return true;
        }
    }
}
