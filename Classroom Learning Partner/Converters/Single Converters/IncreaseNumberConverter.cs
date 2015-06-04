﻿using System;
using System.Windows.Data;
using Catel.Windows.Data.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (double), typeof (double))]
    public class IncreaseNumberConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            double originalValue;
            try
            {
                originalValue = System.Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return 0;
            }

            double increasedAmount;
            try
            {
                increasedAmount = System.Convert.ToDouble(parameter);
            }
            catch (Exception)
            {
                return originalValue;
            }

            return originalValue + increasedAmount;
        }
    }
}