using SunCalcNet;
using System;

namespace TideViewer;

public enum MoonPhase
{
    NewMoon,
    WaxingCrescent,
    FirstQuarter,
    WaxingGibbous,
    FullMoon,
    WaningGibbous,
    LastQuarter,
    WaningCrescent
}

public static class MoonPhaseCalculator
{
    /// <summary>
    /// Returns the moon phase as an enum for the given date/time.
    /// </summary>
    /// <param name="dateTime">Date/time in UTC</param>
    /// <returns>MoonPhase enum value</returns>
    public static MoonPhase GetMoonPhase(DateTime dateTime)
    {
        var illum = MoonCalc.GetMoonIllumination(dateTime);
        double phase = illum.Phase; // 0 = new, 0.25 = first quarter, 0.5 = full, 0.75 = last quarter

        if (phase < 0.03 || phase > 0.97)
            return MoonPhase.NewMoon;
        if (phase < 0.22)
            return MoonPhase.WaxingCrescent;
        if (phase < 0.28)
            return MoonPhase.FirstQuarter;
        if (phase < 0.47)
            return MoonPhase.WaxingGibbous;
        if (phase < 0.53)
            return MoonPhase.FullMoon;
        if (phase < 0.72)
            return MoonPhase.WaningGibbous;
        if (phase < 0.78)
            return MoonPhase.LastQuarter;
        return MoonPhase.WaningCrescent;
    }
}