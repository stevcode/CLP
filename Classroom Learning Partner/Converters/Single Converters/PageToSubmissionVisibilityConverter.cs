using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(CLPPage), typeof(Visibility))]
    public class PageToSubmissionVisibilityConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var page = value as CLPPage;
            if (page == null)
            {
                return Visibility.Hidden;
            }

            return page.Submissions.Any() ? Visibility.Visible : Visibility.Hidden;
        }
    }
}