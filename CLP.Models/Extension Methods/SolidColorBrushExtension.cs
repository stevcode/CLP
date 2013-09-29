using System;
using System.Windows.Media;

namespace CLP.Models
{
    /// <summary>
    /// Use this Extension to convert your SolidColorBrush in Format which is storable
    /// </summary>
    /// <example>
    /// SolidColorBrush scb = new SolidColorBrush(Colors.White);
    /// SaveBrush(scb.ToARGB());
    /// ...
    /// string color = LoadColor();
    /// SolidColorBrush scb = new SolidColorBrush().FromARGB(color);
    /// //or scb.FromARGB(...)
    /// </example>
    /// <remarks>
    /// no warranty, use as you like it
    /// author: daniel bedarf
    /// </remarks>
    public static class SolidColorBrushExtension
    {
        /// <summary>
        /// Convert the Brush to a ARGB - Color. 
        /// </summary>
        /// <param name="brush">your object</param>
        /// <returns>
        /// White = #ffffffff
        /// Green = #ff00ff00
        /// </returns>
        public static string ToARGB(this SolidColorBrush brush)
        {
            if(brush == null)
                throw new ArgumentNullException();
            var c = brush.Color;
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// set the current brush to a new color based on the #argb string
        /// </summary>
        /// <param name="brush">your object</param>
        /// <param name="argb">The #ARGB Color</param>
        /// <returns>the same object as you run the function</returns>
        public static SolidColorBrush FromARGB(this SolidColorBrush brush, string argb)
        {
            if(argb.Length != 9)
                throw new FormatException("we need #aarrggbb as color");

            byte a = Convert.ToByte(int.Parse(argb.Substring(1, 2), System.Globalization.NumberStyles.HexNumber));
            byte r = Convert.ToByte(int.Parse(argb.Substring(3, 2), System.Globalization.NumberStyles.HexNumber));
            byte g = Convert.ToByte(int.Parse(argb.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
            byte b = Convert.ToByte(int.Parse(argb.Substring(7, 2), System.Globalization.NumberStyles.HexNumber));
            var c = Color.FromArgb(a, r, g, b);
            brush.Color = c;
            return brush;
        }
    }
}
