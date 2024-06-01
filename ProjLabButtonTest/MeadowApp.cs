using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace JuegoEyeball
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        readonly IProjectLabHardware projLab;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            //    projLab = ProjectLab.Create();

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            var projLab = ProjectLab.Create();

            var button = projLab.UpButton!;
            button.PressStarted += (s, e) => Console.WriteLine("Button Press started");
            button.PressEnded += (s, e) => Console.WriteLine("Button Press ended");

            Console.WriteLine("Waiting for up button press");

            while (true)
            {
                await Task.Delay(10);
            }
        }
    }
}