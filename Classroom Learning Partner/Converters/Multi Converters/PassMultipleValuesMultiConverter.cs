﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class PassMultipleValuesMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}