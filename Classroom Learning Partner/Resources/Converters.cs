﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CLP.Models;
using Net.Sgoliver.NRtfTree.Core;

namespace Classroom_Learning_Partner.Resources
{
    public class ProjectedDisplayBackgroundConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Colors.PaleGreen);
            }
            else
            {
                return new SolidColorBrush(Colors.AliceBlue);
            }
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
    public class GroupLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value as string;
            return "Group " + val;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }

    public class IntToQuestionMarkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val = (int)value;
            return val == 0 ? "?" : val.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class GrouptoBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = value.ToString().IndexOf("Group");
            int index2 = value.ToString().IndexOf("group"); 
            if(index != -1 || index2 != -1)
            {
                return true;
            }

            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class SumbissiontoBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = value.ToString().IndexOf("Individual");
            int index2 = value.ToString().IndexOf("Collective");
            if(index != -1 || index2 != -1)
            {
                return true;
            }
            
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleantoGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            bool b = (bool)value;
            if(b==true)
            {
                return "Collective";
            }
            else
            {
                return "Individual";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }

    //Converts to shorter date time
  /**  public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is DateTime)
            {
                DateTime d = value as DateTime;
                return (DateTime) value.ToShortTimeString();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("not implemented");
        }
    }
    */

    public class PartsStringConverter : IMultiValueConverter
    {
        public object Convert(object[] value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (!value[0].GetType().Equals((DependencyProperty.UnsetValue).GetType()) && !value[1].GetType().Equals((DependencyProperty.UnsetValue).GetType()))
            {
                int parts = (int)value[0];
                bool interpreted = (bool)value[1];
                if (parts <= 0)
                {
                    //return (interpreted) ? "?" : "";
                    return "?";
                }
            }
            return value[0].ToString();
        }

        public object[] ConvertBack(object value,
            Type[] targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PartsLineVisibilityConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if((bool)value && !App.MainWindowViewModel.IsAuthoring)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UserNameGroupTrimConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            string nameandgroup = value as string;
            string trimmedName = nameandgroup.Split(new char[] { ',' })[0];

            return trimmedName;
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LengthConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) +
                   System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HalfLengthConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            double half = System.Convert.ToDouble(value) / 2;
            return half - System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HeaderVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            double thickness = 0;
            if(!value[0].GetType().Equals((DependencyProperty.UnsetValue).GetType()) && !value[1].GetType().Equals((DependencyProperty.UnsetValue).GetType()))
            {
                Visibility editingModeVisibility = (Visibility)value[0];
                string header = (string)value[1];

                //TODO: Steve - Fix this, absolute mess.
                try
                {
                    RtfTree tree = new RtfTree();
                    tree.LoadRtfText(header);

                    if(editingModeVisibility == Visibility.Visible || tree.Text != "\r\n")
                    {
                        thickness = 1;
                    }
                }
                catch(System.Exception)
                {
                    thickness = 1;
                }
            }
            return thickness;
        }

        public object[] ConvertBack(object value,
            Type[] targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LengthSubtractConverter : IMultiValueConverter
    {
        public object Convert(object[] value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            double newLength = 0;
            if(!value[0].GetType().Equals((DependencyProperty.UnsetValue).GetType()) && !value[1].GetType().Equals((DependencyProperty.UnsetValue).GetType()))
            {
                double length = (double)value[0];
                double subtractedLength = (double)value[1];
                newLength = length - subtractedLength;
            }
            return newLength;
        }

        public object[] ConvertBack(object value,
            Type[] targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NegativeLengthSubtractConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            return - System.Convert.ToDouble(value) -
                   System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            if (vis == Visibility.Visible)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageInteractionModeConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (String.Equals(value.ToString(), parameter.ToString()))
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class LastItemInCollectionConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            System.Console.WriteLine("***************TYPE:" + value.GetType());
            ReadOnlyCollection<object> items = value as ReadOnlyCollection<object>;
            List<Object> pages = new List<Object>();
            //System.Console.WriteLine(TypeOf(value));
            //Object page = items.First();
            //pages.Add(page);
            //ReadOnlyCollection<object> items2 = new ReadOnlyCollection<object>
            return items;
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a double to 3/4 of its value
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class ThreeFourthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Declare variables
            double width = 0;

            try
            {
                // Get value
                width = (double)value;
            }
            catch(Exception)
            {
                // Trace
                Trace.TraceError("Failed to cast '{0}' to Double", value);

                // Return 0
                return 0;
            }

            // Convert
            return (width > 0) ? (width / 4) * 3 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(int))]
    public class HalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Declare variables
            int width = 0;

            try
            {
                // Get value
                width = (int)value;
            }
            catch(Exception)
            {
                // Trace
                Trace.TraceError("Failed to cast '{0}' to Double", value);

                // Return 0
                return 0;
            }

            // Convert
            return (width > 0) ? (width /2 )  : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Converters
    {
    }

    public static class UIHelper
    {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }
}
