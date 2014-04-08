using System;
using System.Globalization;
using System.Windows.Data;
using CLP.Models;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = (ICLPPage)value;
            if(page.Submitter == null)
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
