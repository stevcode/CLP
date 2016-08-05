using System.Windows.Media;

namespace Classroom_Learning_Partner
{
    public static class VisualExtensions
    {
        public static T GetVisualChild<T>(this Visual visual) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(visual);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(visual, i);
                child = v as T ?? GetVisualChild<T>(v);
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        public static T GetVisualParent<T>(this Visual visual) where T : Visual
        {
            var p = (Visual)VisualTreeHelper.GetParent(visual);
            var parent = p as T ?? GetVisualParent<T>(p);

            return parent;
        }
    }
}