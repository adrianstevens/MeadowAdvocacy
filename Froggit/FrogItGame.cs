using Meadow;
using Meadow.Foundation.Audio;
using System;
using System.Diagnostics;

namespace Froggit
{
    public partial class FrogItGame
    {
        enum FrogState
        {
            Forward,
            Left,
            Right,
            Dead
        }

        FrogState frogState;

        //each lane has a velocity
        public float[] LaneSpeeds { get; private set; } = new float[8] { 1.8f, -2.0f, 1.5f, 0, -1.0f, 2.0f, -1.5f, 1.5f };
        public byte[,] LaneData { get; private set; } = new byte[8, 32]
        {
            //no data for docks
            {1,2,3,0,1,2,3,0,0,0,1,2,3,0,1,3,0,0,0,0,1,2,3,0,0,0,0,1,2,3,0,0 },//logs
            {0,0,1,3,0,0,0,1,3,0,0,0,1,3,0,0,1,2,3,0,0,0,0,0,1,3,0,0,1,3,0,0 },//logs
            {1,2,3,0,1,2,3,0,0,0,1,2,3,0,1,2,3,0,0,0,1,2,2,3,0,0,0,1,2,3,0,0 },//logs
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },//sidewalk
            {0,0,1,3,0,1,3,0,0,0,0,0,0,0,0,0,1,3,0,0,0,0,0,0,1,3,0,0,1,3,0,0 },//trucks
            {0,0,1,2,0,0,0,0,0,0,0,1,2,0,0,0,1,2,0,0,0,1,2,0,1,2,0,0,0,0,0,0 },//cars
            {1,2,3,0,0,0,0,0,0,0,0,1,2,3,0,0,0,0,0,1,2,3,0,0,0,0,0,1,2,3,0,0 },//trucks
            {0,0,1,2,0,0,0,0,0,0,0,1,2,0,0,0,1,2,0,0,0,1,2,0,1,2,0,0,0,0,0,0 },//cars
            //no data for start lane
        };

        public bool IsPlaying { get; private set; }

        public bool Winner { get; private set; }

        public int FROG_GOAL = 4;

        public float GameTime { get; private set; }

        public int Deaths { get; private set; }

        public float TimeDelta => GameTime - lastTime;

        public int LaneLength => 32;
        public int Columns { get; private set; } = 20;
        public int Rows => 12;

        public int FrogX { get; set; }
        public int FrogY { get; private set; }

        public int Lives { get; private set; }
        public int FrogsHome { get; private set; }


        DateTime gameStart;

        public FrogItGame(int width = 320)
        {
            Columns = width / cellSize;
            Reset();
        }

        public void Reset()
        {
            gameStart = DateTime.Now;
            ResetFrog();
            Lives = 3;
            Deaths = 0;
            FrogsHome = 0;
            IsPlaying = true;
            Winner = false;
        }

        void ResetFrog()
        {
            FrogX = Columns * cellSize / 2;
            FrogY = (Rows - 1) * cellSize;
            frogState = FrogState.Forward;
        }

        float lastTime;
        int count = 0;
        readonly Stopwatch sw = new();

        void UpdateFrame()
        {
            if (count == 0)
            {
                sw.Start();
            }
            else if (count == 100)
            {
                sw.Stop();
                Resolver.Log.Info($"100 frames took {sw.Elapsed}");
                Resolver.Log.Info($"FPS: {100 / sw.Elapsed.TotalSeconds}");
            }

            count++;

            lastTime = GameTime;
            GameTime = (float)(DateTime.Now - gameStart).TotalSeconds;
        }

        public void Up() => MoveFrogUp();

        public void Down() => MoveFrogDown();

        public void Left() => MoveFrogLeft();

        public void Right() => MoveFrogRight();

        public void Quit() => IsPlaying = false;


        void MoveFrogUp()
        {
            frogState = FrogState.Forward;
            if (FrogY >= cellSize) { FrogY -= cellSize; }

            if (FrogY == 0)
            {
                FrogsHome++;
                _ = effectsAudio?.PlayGameSound(GameSoundEffect.Victory);

                if (FrogsHome >= FROG_GOAL)
                {
                    IsPlaying = false;
                    Winner = true;
                }
                else
                {
                    ResetFrog();
                }
            }
            else
            {
                _ = moveAudio?.PlayGameSound(GameSoundEffect.Footstep);
            }
        }


        void MoveFrogDown()
        {
            frogState = FrogState.Forward;
            if (FrogY < Rows * cellSize - cellSize) { FrogY += cellSize; }
            _ = moveAudio?.PlayGameSound(GameSoundEffect.Footstep);
        }

        void MoveFrogLeft()
        {
            frogState = FrogState.Left;
            if (FrogX > cellSize) { FrogX -= cellSize; }
            _ = moveAudio?.PlayGameSound(GameSoundEffect.Footstep);
        }

        void MoveFrogRight()
        {
            frogState = FrogState.Right;
            if (FrogX <= Columns * cellSize - cellSize) { FrogX += cellSize; }
            _ = moveAudio?.PlayGameSound(GameSoundEffect.Footstep);
        }

        void KillFrog()
        {
            _ = effectsAudio?.PlayGameSound(GameSoundEffect.EnemyDeath);
            frogState = FrogState.Dead;
            Deaths++;
            ResetFrog();
        }
    }
}