using Graphics.MicroGraphics.Dither;
using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TideViewer.assets;

namespace TideViewer
{
    public class Resources
    {
        private static readonly Dictionary<string, Image> _images = new();
        private static readonly Dictionary<string, IPixelBuffer> _ditheredBuffers = new();
        private static readonly string _assemblyName = "TideViewer.Core";

        private static Color[] palette = new Color[]
        {
            Color.Black,
            Color.White,
            Color.Green,
            Color.Blue,
            Color.Red,
            Color.Yellow,
            Color.Orange
        };

        public static IPixelBuffer GetDitheredIcon(IconType icon)
        {
            var name = $"{icon}";

            if (!_ditheredBuffers.ContainsKey(name))
            {
                var img = GetImageResource($"{icon}.bmp");

                var dithered = PixelBufferDither.ToIndexed4(img.DisplayBuffer!, palette, DitherMode.FloydSteinberg, true);
                _ditheredBuffers.Add(name, dithered);
            }

            return _ditheredBuffers[name];
        }

        public static Image GetImageResource(string name)
        {
            if (!_images.ContainsKey(name))
            {
                try
                {
                    _images.Add(name, Image.LoadFromResource($"{name}"));
                }
                catch
                {
                    var availableResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                    var match = availableResources.FirstOrDefault(r => r.Contains(name, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(match);
                        _images.Add(name, Image.LoadFromStream(stream));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return _images[name];
        }
    }
}
