using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CLP.Entities;
using Net.Sgoliver.NRtfTree.Core;

namespace Classroom_Learning_Partner.Resources
{
    public class HalfLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double half = System.Convert.ToDouble(value) / 2;
            return half - System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }

    public class HeaderVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double thickness = 0;
            if(!value[0].GetType().Equals((DependencyProperty.UnsetValue).GetType()) &&
               !value[1].GetType().Equals((DependencyProperty.UnsetValue).GetType()))
            {
                Visibility editingModeVisibility = (Visibility)value[0];
                string header = (string)value[1];

                //TODO: Steve - Fix this, absolute mess.
                try
                {
                    RtfTree tree = new RtfTree();
                    tree.LoadRtfText(header);

                    if(editingModeVisibility == Visibility.Visible ||
                       tree.Text != "\r\n")
                    {
                        thickness = 1;
                    }
                }
                catch(Exception)
                {
                    thickness = 1;
                }
            }
            return thickness;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }

    public class LengthSubtractConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double newLength = 0;
            if(!value[0].GetType().Equals((DependencyProperty.UnsetValue).GetType()) &&
               !value[1].GetType().Equals((DependencyProperty.UnsetValue).GetType()))
            {
                double length = (double)value[0];
                double subtractedLength = (double)value[1];
                newLength = length - subtractedLength;
            }
            return newLength;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }

    public class NegativeLengthSubtractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return - System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}