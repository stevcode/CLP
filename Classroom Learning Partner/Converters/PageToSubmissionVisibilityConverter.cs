using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = value as CLPPage;
            if (page == null)
            {
                return Visibility.Hidden;
            }

            return page.Submissions.Any() ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}