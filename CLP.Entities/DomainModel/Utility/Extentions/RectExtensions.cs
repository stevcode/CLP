using System.Windows;
using Catel;

namespace CLP.Entities.Demo
{
    public static class RectExtensions
    {
        public static double Area(this Rect rect)
        {
            Argument.IsNotNull("rect", rect);

            return rect.Width * rect.Height;
        }

        public static Point Center(this Rect rect)
        {
            Argument.IsNotNull("rect", rect);

            return new Point(rect.Left + (rect.Width / 2), rect.Top + (rect.Height / 2));
        }
    }
}