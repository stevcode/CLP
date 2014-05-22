using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

            var latestSubmissions = new List<CLPPage>();
            if(pageSubmissions == null)
            {
                return submissionsWithBlanks;
            }

            foreach(var pageSubmission in pageSubmissions)
            {
                var submissionFromSameStudent = latestSubmissions.FirstOrDefault(latestSubmission => pageSubmission.OwnerID == latestSubmission.OwnerID && 
                                                                                                     pageSubmission.VersionIndex > latestSubmission.VersionIndex);

                if(submissionFromSameStudent != null)
                {
                    latestSubmissions.Remove(submissionFromSameStudent);
                }
                
                latestSubmissions.Add(pageSubmission);
            }

            foreach(var student in studentList)
            {
                var foundSubmission = latestSubmissions.FirstOrDefault(pageSubmission => pageSubmission.OwnerID == student.ID);
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
