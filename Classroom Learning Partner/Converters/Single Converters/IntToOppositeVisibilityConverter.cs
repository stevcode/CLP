﻿using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntToOppositeVisibilityConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var val = value.ToInt();
            if (val == null)
            {
                return Visibility.Visible;
            }

            return (int)val > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}