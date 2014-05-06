using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionsListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pageSubmissions = values[0] as ObservableCollection<CLPPage>;
            var submissionsWithBlanks = new ObservableCollection<StudentProgressInfo>();
            var studentList = new ObservableCollection<Person>();
            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                studentList = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                for(int i = 1; i <= 10; i++)
                {
                    studentList.Add(Person.TestSubmitter);
                }
            }
            foreach (Person student in studentList) {
                CLPPage foundSubmission = null;
                if(pageSubmissions != null)
                {
                    foreach(CLPPage submission in pageSubmissions)
                    {
                        if(submission.OwnerID == student.ID)
                        {
                            foundSubmission = submission;
                            break;
                        }
                    }
                }
                submissionsWithBlanks.Add(new StudentProgressInfo(student, foundSubmission));
            }
            return submissionsWithBlanks;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
