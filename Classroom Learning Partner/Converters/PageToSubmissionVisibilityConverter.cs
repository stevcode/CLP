using System;
using System.Globalization;
using System.Windows.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = (CLPPage)value;
            if(page.Owner == null)
            {
                return "Hidden";
            }
            return "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
