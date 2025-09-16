using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace ArducamMini
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();


            Console.WriteLine("initialized");


            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

        }
    }
}