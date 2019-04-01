using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Bio.Controls
{
    /// <summary>
    /// Color collection used
    /// </summary>
    public class ColorCollection : ObservableCollection<Color>
    {
    }

    /// <summary>
    /// Utility class for color conversions
    /// </summary>
    static class ColorUtilities
    {
        /// <summary>
        /// This converts a standard WPF Color structure into discrete hue/saturation/brightness values.
        /// Taken from http://www.codeplex.com/Kaxaml.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="b">Brightness</param>
        public static void ToHsb(this Color color, out double h, out double s, out double b)
        {
            int imax = color.R, imin = color.R;

            if (color.G > imax) 
                imax = color.G; 
            else if (color.G < imin) 
                imin = color.G;

            if (color.B > imax) 
                imax = color.B; 
            else if (color.B < imin) 
                imin = color.B;

            double max = imax / 255.0, min = imin / 255.0;
            double value = max;
            double saturation = (max > 0) ? (max - min) / max : 0.0;
            double hue = 0;

            if (imax > imin)
            {
                double f = 1.0 / ((max - min) * 255.0);
                hue = (imax == color.R) ? 0.0 + f * (color.G - color.B)
                    : (imax == color.G) ? 2.0 + f * (color.B - color.R)
                    : 4.0 + f * (color.R - color.G);
                hue = hue * 60.0;
                if (hue < 0.0)
                    hue += 360.0;
            }

            h = hue / 360;
            s = saturation;
            b = value;
        }

        /// <summary>
        /// This converts a hue/saturation/brightness value into standard WPF Color structure.
        /// See http://blogs.msdn.com/cjacks/archive/2006/04/12/575476.aspx and
        /// http://www.codeplex.com/Kaxaml for the original algorithm used here.
        /// </summary>
        /// <param name="a">Alpha</param>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="b">Brightness</param>
        /// <returns></returns>
        public static Color ColorFromAhsb(byte a, double h, double s, double b)
        {
            if (0 > a || 255 < a)
                throw new ArgumentOutOfRangeException("a");
            if (0f > h || 1f < h)
                throw new ArgumentOutOfRangeException("h");
            if (0f > s || 1f < s)
                throw new ArgumentOutOfRangeException("s");
            if (0f > b || 1f < b)
                throw new ArgumentOutOfRangeException("b");

            double red = 0.0, green = 0.0, blue = 0.0;

            if (s == 0.0)
                red = green = blue = b;
            else
            {
                double hVal = h * 360;
                while (hVal >= 360.0)
                    hVal -= 360.0;
                hVal = hVal / 60f;
                
                int sextant = (int)hVal;

                double fVal = hVal - sextant;
                double rVal = b * (1.0 - s);
                double sVal = b * (1.0 - s * fVal);
                double tVal = b * (1.0 - s * (1.0 - fVal));

                switch (sextant)
                {
                    case 0: red = b; green = tVal; blue = rVal; break;
                    case 1: red = sVal; green = b; blue = rVal; break;
                    case 2: red = rVal; green = b; blue = tVal; break;
                    case 3: red = rVal; green = sVal; blue = b; break;
                    case 4: red = tVal; green = rVal; blue = b; break;
                    case 5: red = b; green = rVal; blue = sVal; break;
                }
            }

            return Color.FromArgb(a, (byte)(red * 255.0),  (byte)(green * 255.0),  (byte)(blue * 255.0));
        }
    }
}
