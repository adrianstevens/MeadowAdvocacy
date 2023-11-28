using Meadow.Peripherals.Leds;
using System.Threading;

namespace Skeeball.Controllers;

internal class LedService
{
    readonly ILed[] leds;

    public LedService()
    {
        leds = new ILed[5];
    }

    public void SwipeLeds()
    {
        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(200);
            ClearLeds();
            leds[i].IsOn = true;
        }
        for (int i = 4; i > 0; i--)
        {
            ClearLeds();
            leds[i].IsOn = true;
            Thread.Sleep(200);
        }
    }

    public void FlashLeds()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var led in leds)
            {
                led.IsOn = true;
            }
            Thread.Sleep(50);
            foreach (var led in leds)
            {
                led.IsOn = false;
            }
            Thread.Sleep(50);
        }
    }

    public void SetLeds(int score)
    {
        ClearLeds();

        if (score >= 50)
        {
            leds[4].IsOn = true;
        }
        if (score >= 40)
        {
            leds[3].IsOn = true;
        }
        if (score >= 30)
        {
            leds[2].IsOn = true;
        }
        if (score >= 20)
        {
            leds[1].IsOn = true;
        }
        if (score >= 10)
        {
            leds[0].IsOn = true;
        }
    }

    public void SetLed(int index, bool state)
    {
        leds[index].IsOn = state;
    }

    public void ClearLeds()
    {
        foreach (var led in leds)
        {
            led.IsOn = false;
        }
    }
}
