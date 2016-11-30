using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CLP.Entities.Ann;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Converters
{
    class DivisionPositionToColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = 0;
            if(value[1] is ObservableCollection<CLPArrayDivision>)
            {
                foreach(var division in (ObservableCollection<CLPArrayDivision>)value[1])
                {
                    index += 1;
                    if((double)value[0] == division.Position)
                    {
                        break;
                    }
                }
                if(index == ((ObservableCollection<CLPArrayDivision>)value[1]).Count)
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                else if(index % 2 == 0)
                {
                    return new SolidColorBrush(Colors.MediumPurple);
                }
                return new SolidColorBrush(Colors.SpringGreen);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
