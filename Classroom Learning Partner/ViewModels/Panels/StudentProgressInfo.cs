using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StudentProgressInfo : ViewModelBase
    {
        public StudentProgressInfo(Person student, CLPPage page)
        {
            Student = student;
            Page = page;
        }

        /// <summary>
        /// The student
        /// </summary>
        public Person Student
        {
            get { return GetValue<Person>(StudentProperty); }
            set { SetValue(StudentProperty, value); }
        }

        public static readonly PropertyData StudentProperty = RegisterProperty("Student", typeof(Person));

        /// <summary>
        /// The page
        /// </summary>
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));
    }
}