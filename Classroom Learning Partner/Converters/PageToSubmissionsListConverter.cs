using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Catel.IoC;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToSubmissionsListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            

            var pageSubmissions = values[0] as ObservableCollection<CLPPage>;
            var submissionsWithBlanks = new ObservableCollection<StudentProgressInfo>();
            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            if (notebookService == null)
            {
                return submissionsWithBlanks;
            }

            var studentList = new ObservableCollection<Person>();
            if (notebookService.CurrentClassPeriod != null)
            {
                studentList = notebookService.CurrentClassPeriod.ClassSubject.StudentList;
            }

            var allSubmissions = new List<CLPPage>();
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

                allSubmissions.Add(pageSubmission);
                latestSubmissions.Add(pageSubmission);
            }

            foreach(var student in studentList)
            {
                var thisStudentLatestSubmission = latestSubmissions.FirstOrDefault(pageSubmission => pageSubmission.OwnerID == student.ID);
                var thisStudentAllSubmissions = allSubmissions.FindAll(pageSubmission => pageSubmission.OwnerID == student.ID);
                StudentProgressInfo studentData = new StudentProgressInfo(student, thisStudentLatestSubmission);
                studentData.AllPages = thisStudentAllSubmissions;
                submissionsWithBlanks.Add(studentData);
            }

            return submissionsWithBlanks;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
