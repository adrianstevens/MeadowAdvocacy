using Meadow.Units;
using System;

namespace RotatingCubeJuego
{
    public class Cube3d
    {
        public int[,] Wireframe = new int[8, 3];

        private int[,] cubeVertices;

        public Angle XRotation { get; set; }
        public Angle YRotation { get; set; }
        public Angle ZRotation { get; set; }

        public Angle XVelocity { get; set; }
        public Angle YVelocity { get; set; }
        public Angle ZVelocity { get; set; }

        private double rotationX, rotationY, rotationZ;
        private double rotationXX, rotationYY, rotationZZ;
        private double rotationXXX, rotationYYY, rotationZZZ;

        private readonly int originX;
        private readonly int originY;

        public Cube3d(int xCenter, int yCenter, int cubeSize = 60)
        {
            InitVertices(cubeSize);

            originX = xCenter;
            originY = yCenter;
        }

        void InitVertices(int cubeSize)
        {
            cubeVertices = new int[8, 3] {
                 { -cubeSize, -cubeSize,  cubeSize},
                 {  cubeSize, -cubeSize,  cubeSize},
                 {  cubeSize,  cubeSize,  cubeSize},
                 { -cubeSize,  cubeSize,  cubeSize},
                 { -cubeSize, -cubeSize, -cubeSize},
                 {  cubeSize, -cubeSize, -cubeSize},
                 {  cubeSize,  cubeSize, -cubeSize},
                 { -cubeSize,  cubeSize, -cubeSize},
            };
        }

        public void Update()
        {
            XRotation += XVelocity;
            YRotation += YVelocity;
            ZRotation += ZVelocity;

            for (int i = 0; i < 8; i++)
            {
                //rotateY
                rotationZ = cubeVertices[i, 2] * Math.Cos(YRotation.Radians) - cubeVertices[i, 0] * Math.Sin(YRotation.Radians);
                rotationX = cubeVertices[i, 2] * Math.Sin(YRotation.Radians) + cubeVertices[i, 0] * Math.Cos(YRotation.Radians);
                rotationY = cubeVertices[i, 1];

                //rotateX
                rotationYY = rotationY * Math.Cos(XRotation.Radians) - rotationZ * Math.Sin(XRotation.Radians);
                rotationZZ = rotationY * Math.Sin(XRotation.Radians) + rotationZ * Math.Cos(XRotation.Radians);
                rotationXX = rotationX;

                //rotateZ
                rotationXXX = rotationXX * Math.Cos(ZRotation.Radians) - rotationYY * Math.Sin(ZRotation.Radians);
                rotationYYY = rotationXX * Math.Sin(ZRotation.Radians) + rotationYY * Math.Cos(ZRotation.Radians);
                rotationZZZ = rotationZZ;

                //orthographic projection
                rotationXXX += originX;
                rotationYYY += originY;

                //store new vertices values for wireframe drawing
                Wireframe[i, 0] = (int)rotationXXX;
                Wireframe[i, 1] = (int)rotationYYY;
                Wireframe[i, 2] = (int)rotationZZZ;
            }
        }
    }
}
