using System.Reflection;
using System.Windows.Media;
using Catel;

namespace CLP.Entities.Demo
{
    public static class ColorExtensions
    {
        public static string ToColorName(this Color color)
        {
            Argument.IsNotNull("color", color);

            var leastDifference = 0;
            var colorName = string.Empty;

            foreach (var systemColor in typeof (Color).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                var systemColorValue = (Color)systemColor.GetValue(null, null);

                if (systemColorValue == color)
                {
                    colorName = systemColor.Name;
                    break;
                }

                var a = color.A - systemColorValue.A;
                var r = color.R - systemColorValue.R;
                var g = color.G - systemColorValue.G;
                var b = color.B - systemColorValue.B;
                var difference = a * a + r * r + g * g + b * b;

                if (difference >= leastDifference)
                {
                    continue;
                }

                colorName = systemColor.Name;
                leastDifference = difference;
            }

            return colorName;
        }
    }
}