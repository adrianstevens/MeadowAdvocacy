using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace WaterQualityTracker
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        WaterQualityController waterQualityController;

        IProjectLabHardware projLab;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            waterQualityController.Run();

            return Task.CompletedTask;
        }

        public async override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            waterQualityController = new WaterQualityController();
            await waterQualityController.Initialize(projLab);
        }
    }
}