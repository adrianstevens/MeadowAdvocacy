using HalloweenEyeball;
using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace JuegoEyeball
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        EyeballController eyeballController;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            eyeballController = new EyeballController(projLab.Display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            eyeballController.DrawEyeball();

            while (true)
            {
                eyeballController.Delay();
                eyeballController.RandomEyeMovement();
                eyeballController.Delay();
                eyeballController.CenterEye();
            }
        }
    }
}