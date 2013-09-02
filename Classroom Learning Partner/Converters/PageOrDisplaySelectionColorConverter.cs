﻿using System;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class PageOrDisplaySelectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;

            if((bool)value) //IsSelected
            {
                return dict["MainColor"];
            }
            return dict["GrayBorderColor"];
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
