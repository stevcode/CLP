using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StudentSelectorViewModel : ViewModelBase
    {
        public StudentSelectorViewModel()
        {
            SortedStudents.Source = App.MainWindowViewModel.AvailableUsers;
            SortDescription StudentNameSort = new SortDescription("FullName", ListSortDirection.Ascending);
            SortedStudents.SortDescriptions.Add(StudentNameSort);

            SelectStudentCommand = new Command<Person>(OnSelectStudentCommandExecute);
        }

        public ObservableCollection<Person> SelectedStudents
        {
            get { return GetValue<ObservableCollection<Person>>(SelectedStudentsProperty); }
            set { SetValue(SelectedStudentsProperty, value); }
        }

        public static readonly PropertyData SelectedStudentsProperty = RegisterProperty("SelectedStudents", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());
    
        public CollectionViewSource SortedStudents
        {
            get { return GetValue<CollectionViewSource>(SortedStudentsProperty); }
            set { SetValue(SortedStudentsProperty, value); }
        }

        public static readonly PropertyData SortedStudentsProperty = RegisterProperty("SortedStudents", typeof(CollectionViewSource), () => new CollectionViewSource());

        public Command<Person> SelectStudentCommand
        {
            get;
            private set;
        }

        public void OnSelectStudentCommandExecute(Person student)
        {
            if(SelectedStudents.Contains(student))
            {
                SelectedStudents.Remove(student);
            }
            else
            {
                SelectedStudents.Add(student);
            }
        }
    }
}
