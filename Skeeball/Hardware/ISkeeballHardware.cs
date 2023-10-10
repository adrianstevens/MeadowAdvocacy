﻿using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Speakers;

namespace Skeeball;

internal interface ISkeeballHardware
{
    public IGraphicsDisplay TopDisplay { get; }
    public IGraphicsDisplay BottomDisplay { get; }

    public IToneGenerator Speaker { get; }

    public ILed[] Leds { get; }

    public IButton StartButton { get; }
    public IButton SelectButton { get; }

    public IButton Score10Switch { get; }
    public IButton Score20Switch { get; }
    public IButton Score30Switch { get; }
    public IButton Score40Switch { get; }
    public IButton Score50Switch { get; }
}
