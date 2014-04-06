using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections;
using CLP.Models;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Converters
{
    public class SubmissionsListForPageByStudentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var submissionsforpage = ((IDictionary)values[0])[values[3]];
            var studentslist = (IList)values[1];
            var blankpage = ((CLPNotebook)values[2]).GetNotebookPageByID((string)values[3]);

            ObservableCollection<ICLPPage> submissionswithblanks = new ObservableCollection<ICLPPage>();
            foreach(var student in studentslist)
            {
                bool found = false;
                foreach(var submission in (IList)submissionsforpage)
                {
                    if(((string)((ICLPPage)submission).Submitter.FullName) == ((string)student))
                    {
                        submissionswithblanks.Add((ICLPPage)submission);
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    submissionswithblanks.Add(blankpage);
                }
            }
            return submissionswithblanks;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
