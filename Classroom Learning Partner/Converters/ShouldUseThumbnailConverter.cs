using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.Converters
{
    public class ShouldUseThumbnailConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            CLPPage current = values[0] as CLPPage;
            CLPPage compared = values[1] as CLPPage;

            if(current != null && compared != null && current.ID == compared.ID && current.OwnerID == compared.OwnerID)
            {
                return false;
            }

            return compared != null && compared.PageThumbnail != null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
