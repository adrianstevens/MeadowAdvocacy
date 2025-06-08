using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NESEmu
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        NES nes;


        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            Helper.display = projLab.Display;
            nes = new NES();

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            while (true)
            {
                nes.Run();
            }

            return Task.CompletedTask;
        }
    }
}