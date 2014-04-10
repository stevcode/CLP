using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CLP.Models;

namespace Classroom_Learning_Partner.Converters
{
    public class PageMatchToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;

            ICLPPage current = values[0] as ICLPPage;
            ICLPPage compared = values[1] as ICLPPage;

            // same page
            if(current != null && current.UniqueID == compared.UniqueID)
            {
                // submission
                if(current.Submitter != null && current.SubmissionID == compared.SubmissionID)
                {
                    return dict["MainColor"];
                }
            }
            return dict["GrayBorderColor"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
