﻿using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;

namespace WaterQualityTracker
{
    internal class DisplayController
    {
        readonly MicroGraphics graphics;

        public DisplayController(IPixelDisplay display)
        {
            graphics = new MicroGraphics(display);
        }

        public void UpdateDisplay(
           (Meadow.Units.ConcentrationInWater? DissolvedOxygen,
            Meadow.Units.ConcentrationInWater? Chlorophyl,
            Meadow.Units.ConcentrationInWater? BlueGreenAlgae,
            Meadow.Units.Conductivity? ElectricalConductivity,
            Meadow.Units.PotentialHydrogen? PH,
            Meadow.Units.Turbidity? Turbidity,
            Meadow.Units.Temperature? Temperature,
            Meadow.Units.Voltage? OxidationReductionPotential) data)
        {
            graphics.Clear();

            graphics.DrawText(0, 0, "Y4000 Water Quality", Color.Red, ScaleFactor.X2);

            graphics.DrawText(0, 60, $"Temp: {data.Temperature.Value.Celsius:0.0}C", Color.White, ScaleFactor.X2);
            graphics.DrawText(0, 90, $"pH: {data.PH.Value:0.0}pH", Color.White, ScaleFactor.X2);
            graphics.DrawText(0, 120, $"Disolved O2: {data.DissolvedOxygen.Value:0.0}", Color.White, ScaleFactor.X2);
            graphics.DrawText(0, 150, $"Algae: {data.BlueGreenAlgae.Value:0.0}", Color.White, ScaleFactor.X2);

            graphics.Show();
        }
    }
}
