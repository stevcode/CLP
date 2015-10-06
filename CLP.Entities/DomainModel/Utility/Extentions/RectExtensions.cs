using System.Windows;
using Catel;

namespace CLP.Entities
{
    public static class RectExtensions
    {
        public static double Area(this Rect rect)
        {
            Argument.IsNotNull("rect", rect);

            return rect.Width * rect.Height;
        }
    }
}