using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionsListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = (CLPPage)value;
            var submissions = new ObservableCollection<StudentProgressInfo>();
            var studentList = new ObservableCollection<Person>();
            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                studentList = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                Person dummy = new Person();
                dummy.FullName = "Test";
                studentList.Add(dummy);
            }
            foreach (Person student in studentList) {
                CLPPage foundSubmission = null;
                foreach(CLPPage submission in page.Submissions)
                {
                    // TODO check same person :)
                    foundSubmission = submission;
                    break;
                }
                submissions.Add(new StudentProgressInfo(student, foundSubmission));
            }
            return submissions;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
