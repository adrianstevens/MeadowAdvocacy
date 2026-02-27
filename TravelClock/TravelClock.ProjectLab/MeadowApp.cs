using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;
using TravelClock.Core;

namespace TravelClockProjectLab
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab = default!;
        MicroGraphics graphics = default!;
        ClockController controller = default!;

        public override Task Initialize()
        {
            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display)
            {
                Stroke = 1
            };

            controller = new ClockController(graphics, projLab.Speaker);

            projLab.RightButton!.Clicked += (s, e) => controller.NextView();
            projLab.LeftButton!.Clicked  += (s, e) => controller.PreviousView();
            projLab.UpButton!.Clicked    += (s, e) => controller.UpField();
            projLab.DownButton!.Clicked  += (s, e) => controller.DownField();

            return base.Initialize();
        }

        public override Task Run()
        {
            controller.Start();
            return Task.CompletedTask;
        }
    }
}
