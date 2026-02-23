using System;

namespace Boids.Core
{
    public class BoidsEngine
    {
        public const int Width = 320;
        public const int Height = 240;
        public const int NumBoids = 80;

        // Neighbor detection radii
        private const float NeighborRadius = 50f;
        private const float SeparationRadius = 15f;
        private const float NeighborRadiusSq = NeighborRadius * NeighborRadius;
        private const float SeparationRadiusSq = SeparationRadius * SeparationRadius;

        // Steering weights
        private const float SeparationWeight = 1.8f;
        private const float AlignmentWeight = 1.0f;
        private const float CohesionWeight = 1.0f;

        // Speed limits (pixels/second)
        private const float MaxSpeed = 80f;
        private const float MinSpeed = 30f;
        private const float MaxForce = 100f;

        private const float WindStrength = 25f;

        // SoA layout for cache-friendly inner loop
        public readonly float[] X = new float[NumBoids];
        public readonly float[] Y = new float[NumBoids];
        public readonly float[] Vx = new float[NumBoids];
        public readonly float[] Vy = new float[NumBoids];

        // Pre-allocated force buffers — no per-frame allocation
        private readonly float[] _fx = new float[NumBoids];
        private readonly float[] _fy = new float[NumBoids];

        private float _windX, _windY;
        private readonly Random _rand = new Random();

        public void SetWind(float wx, float wy)
        {
            _windX = wx;
            _windY = wy;
        }

        public void Initialize()
        {
            for (int i = 0; i < NumBoids; i++)
            {
                X[i] = (float)(_rand.NextDouble() * Width);
                Y[i] = (float)(_rand.NextDouble() * Height);

                float angle = (float)(_rand.NextDouble() * MathF.PI * 2f);
                float speed = MinSpeed + (float)(_rand.NextDouble() * (MaxSpeed - MinSpeed));
                Vx[i] = MathF.Cos(angle) * speed;
                Vy[i] = MathF.Sin(angle) * speed;
            }
        }

        public void Update(float dt)
        {
            // Phase 1: compute steering forces for all boids simultaneously
            // (using velocities/positions from the previous frame for symmetry)
            for (int i = 0; i < NumBoids; i++)
            {
                float xi = X[i], yi = Y[i];
                float vxi = Vx[i], vyi = Vy[i];

                float sepX = 0f, sepY = 0f;
                float aliX = 0f, aliY = 0f;
                float cohX = 0f, cohY = 0f;
                int neighborCount = 0;
                bool hasSep = false;

                for (int j = 0; j < NumBoids; j++)
                {
                    if (i == j) continue;

                    float dx = xi - X[j];
                    float dy = yi - Y[j];

                    // Wrap-aware distance (pick shortest path across wrapped edges)
                    if (dx >  Width  * 0.5f) dx -= Width;
                    else if (dx < -Width  * 0.5f) dx += Width;
                    if (dy >  Height * 0.5f) dy -= Height;
                    else if (dy < -Height * 0.5f) dy += Height;

                    float distSq = dx * dx + dy * dy;

                    if (distSq < NeighborRadiusSq)
                    {
                        // Alignment: accumulate neighbor velocities
                        aliX += Vx[j];
                        aliY += Vy[j];

                        // Cohesion: accumulate neighbor positions
                        cohX += X[j];
                        cohY += Y[j];

                        neighborCount++;

                        // Separation: smooth repulsion, no sqrt needed
                        if (distSq < SeparationRadiusSq && distSq > 0.01f)
                        {
                            float weight = 1f - distSq / SeparationRadiusSq;
                            sepX += dx * weight;
                            sepY += dy * weight;
                            hasSep = true;
                        }
                    }
                }

                float fx = 0f, fy = 0f;

                if (neighborCount > 0)
                {
                    float invN = 1f / neighborCount;

                    // Alignment: steer toward average neighbor velocity
                    aliX = aliX * invN - vxi;
                    aliY = aliY * invN - vyi;
                    Limit(ref aliX, ref aliY, MaxForce);
                    fx += aliX * AlignmentWeight;
                    fy += aliY * AlignmentWeight;

                    // Cohesion: steer toward center of mass
                    cohX = cohX * invN - xi;
                    cohY = cohY * invN - yi;
                    Limit(ref cohX, ref cohY, MaxForce);
                    fx += cohX * CohesionWeight;
                    fy += cohY * CohesionWeight;
                }

                if (hasSep)
                {
                    Limit(ref sepX, ref sepY, MaxForce);
                    fx += sepX * SeparationWeight;
                    fy += sepY * SeparationWeight;
                }

                // Wind nudge from accelerometer tilt
                fx += _windX * WindStrength;
                fy += _windY * WindStrength;

                _fx[i] = fx;
                _fy[i] = fy;
            }

            // Phase 2: apply forces and move
            for (int i = 0; i < NumBoids; i++)
            {
                float vx = Vx[i] + _fx[i] * dt;
                float vy = Vy[i] + _fy[i] * dt;

                // Clamp to speed range
                float speed = MathF.Sqrt(vx * vx + vy * vy);
                if (speed > MaxSpeed)
                {
                    float inv = MaxSpeed / speed;
                    vx *= inv; vy *= inv;
                }
                else if (speed < MinSpeed && speed > 0.001f)
                {
                    float inv = MinSpeed / speed;
                    vx *= inv; vy *= inv;
                }

                Vx[i] = vx;
                Vy[i] = vy;

                // Wrap-around toroidal space
                float x = X[i] + vx * dt;
                float y = Y[i] + vy * dt;
                if (x < 0f) x += Width;
                else if (x >= Width) x -= Width;
                if (y < 0f) y += Height;
                else if (y >= Height) y -= Height;
                X[i] = x;
                Y[i] = y;
            }
        }

        private static void Limit(ref float x, ref float y, float max)
        {
            float lenSq = x * x + y * y;
            if (lenSq > max * max)
            {
                float inv = max / MathF.Sqrt(lenSq);
                x *= inv;
                y *= inv;
            }
        }
    }
}
