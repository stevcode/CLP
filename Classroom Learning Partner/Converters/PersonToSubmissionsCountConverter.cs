using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.Converters
{
    public class PersonToSubmissionsCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Person person = values[0] as Person;
            ObservableCollection<CLPPage> pages = values[1] as ObservableCollection<CLPPage>;

            int submissionsCount = 0;
            foreach(CLPPage page in pages)
            {
                foreach(CLPPage submission in page.Submissions)
                {
                    if(submission.OwnerID == person.ID)
                    {
                        submissionsCount++;
                        break;
                    }
                }
            }
            return ""+submissionsCount;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
