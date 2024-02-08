using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Hid;

namespace ProjLab_Demo
{
    public class DisplayController
    {
        readonly MicroGraphics graphics;

        public AnalogJoystickPosition JoystickPosition
        {
            get => joystickPosition;
            set
            {
                joystickPosition = value;
                Update();
            }


        }
        AnalogJoystickPosition joystickPosition;

        public bool CButtonState
        {
            get => upButtonState;
            set
            {
                upButtonState = value;
                Update();
            }
        }
        bool upButtonState = false;

        public bool ZButtonState
        {
            get => downButtonState;
            set
            {
                downButtonState = value;
                Update();
            }
        }
        bool downButtonState = false;

        bool isUpdating = false;
        bool needsUpdate = false;

        public DisplayController(IPixelDisplay display)
        {
            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x16()
            };

            graphics.Clear(true);
        }

        public void Update()
        {
            if (isUpdating)
            {   //queue up the next update
                needsUpdate = true;
                return;
            }

            isUpdating = true;

            graphics.Clear();
            Draw();
            graphics.Show();

            isUpdating = false;

            if (needsUpdate)
            {
                needsUpdate = false;
                Update();
            }
        }

        void DrawStatus(string label, string value, Color color, int yPosition)
        {
            graphics.DrawText(x: 2, y: yPosition, label, color: color);
            graphics.DrawText(x: 238, y: yPosition, value, alignmentH: HorizontalAlignment.Right, color: color);
        }

        void Draw()
        {
            graphics.DrawText(x: 2, y: 0, "Hello PROJ LAB!", WildernessLabsColors.AzureBlue);

            DrawStatus("Joystick:", $"{JoystickPosition.Horizontal:0.00}, {JoystickPosition.Vertical:0.00}", WildernessLabsColors.ChileanFire, 40);

            DrawStatus("Down:", $"{(ZButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 180);
            DrawStatus("Up:", $"{(CButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 160);
        }
    }
}