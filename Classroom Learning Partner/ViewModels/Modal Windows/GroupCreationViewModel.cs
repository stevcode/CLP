using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GroupCreationViewModel : ViewModelBase
    {
        public GroupCreationViewModel()
        {
            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                StudentsNotInGroup = new ObservableCollection<Person>(App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList);
            }
            else
            {
                StudentsNotInGroup = new ObservableCollection<Person>();
                for(int i = 1; i <= 10; i++)
                {
                    StudentsNotInGroup.Add(Person.TestSubmitter);
                }
            }

            Groups = new ObservableCollection<Group>();
            Groups.Add(new Group("A"));
            Groups.Add(new Group("B"));

            GroupChangeCommand = new Command<object[]>(OnGroupChangeCommandExecute);
            AddGroupCommand = new Command(OnAddGroupCommandExecute);
            RemoveGroupCommand = new Command(OnRemoveGroupCommandExecute);
        }

        public ObservableCollection<Group> Groups
        {
            get { return GetValue<ObservableCollection<Group>>(GroupsProperty); }
            set { SetValue(GroupsProperty, value); }
        }

        public static readonly PropertyData GroupsProperty = RegisterProperty("Groups", typeof(ObservableCollection<Group>));

        public ObservableCollection<Person> StudentsNotInGroup
        {
            get { return GetValue<ObservableCollection<Person>>(StudentsNotInGroupProperty); }
            set { SetValue(StudentsNotInGroupProperty, value); }
        }

        public static readonly PropertyData StudentsNotInGroupProperty = RegisterProperty("StudentsNotInGroup", typeof(ObservableCollection<Person>));
    
        public Command<object[]> GroupChangeCommand { get; private set; }

        public void OnGroupChangeCommandExecute(object[] parameters)
        {
            Group newGroup = parameters[0] as Group;
            Person movingPerson = parameters[1] as Person;
            foreach(Group g in Groups)
            {
                if(g.Members.Contains(movingPerson))
                {
                    g.Remove(movingPerson);
                    if(g == newGroup)
                    {
                        StudentsNotInGroup.Add(movingPerson);
                        return;
                    }
                }
            }
            if(StudentsNotInGroup.Contains(movingPerson))
            {
                StudentsNotInGroup.Remove(movingPerson);
            }
            newGroup.Add(movingPerson);
        }

        public Command AddGroupCommand
        {
            get;
            private set;
        }

        public void OnAddGroupCommandExecute()
        {
            string lastLabel = Groups[Groups.Count - 1].Label;
            string nextLabel = "" + (char)(lastLabel[0] + 1);
            Group newGroup = new Group(nextLabel);
            Groups.Add(newGroup);
        }

        public Command RemoveGroupCommand
        {
            get;
            private set;
        }

        public void OnRemoveGroupCommandExecute()
        {
            if(Groups.Count <= 1)
            {
                return;
            }
            Group lastGroup = Groups[Groups.Count - 1];
            foreach(Person student in lastGroup.Members)
            {
                StudentsNotInGroup.Add(student);
            }
            Groups.Remove(lastGroup);
        }
    }
}

public class Group : ViewModelBase
{
    public string Label
    {
        get { return GetValue<string>(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }

    public static readonly PropertyData LabelProperty = RegisterProperty("Label", typeof(string));
  
    public ObservableCollection<Person> Members
    {
        get { return GetValue<ObservableCollection<Person>>(MembersProperty); }
        set { SetValue(MembersProperty, value); }
    }

    public static readonly PropertyData MembersProperty = RegisterProperty("Members", typeof(ObservableCollection<Person>));

    public Group(string label)
    {
        Members = new ObservableCollection<Person>();
        Label = label;
    }

    public void Add(Person student)
    {
        Members.Add(student);
    }

    public void Remove(Person student)
    {
        Members.Remove(student);
    }
}