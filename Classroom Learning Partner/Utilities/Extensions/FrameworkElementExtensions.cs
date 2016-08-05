using System;
using System.Windows;
using System.Windows.Media;
using Catel;

namespace Classroom_Learning_Partner
{
    public static class FrameworkElementExtensions
    {
        public static T FindNamedChild<T>(this FrameworkElement frameworkElement, string name)
        {
            Argument.IsNotNull("frameworkElement", frameworkElement);
            Argument.IsNotNull("name", name);

            var dependencyObject = frameworkElement as DependencyObject;
            var ret = default(T);

            if (dependencyObject == null)
            {
                return ret;
            }

            var childcount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < childcount; i++)
            {
                var childDep = VisualTreeHelper.GetChild(dependencyObject, i);
                var child = childDep as FrameworkElement;

                if (child != null &&
                    child.GetType() == typeof(T) &&
                    child.Name == name)
                {
                    ret = (T)Convert.ChangeType(child, typeof(T));
                    break;
                }

                ret = FindNamedChild<T>(child, name);
                if (ret != null)
                {
                    break;
                }
            }

            return ret;
        }
    }
}