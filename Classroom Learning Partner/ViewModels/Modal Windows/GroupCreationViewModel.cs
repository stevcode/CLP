using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GroupCreationViewModel : ViewModelBase
    {
        public string GroupType;

        public GroupCreationViewModel(string groupType)
        {
            GroupType = groupType;
            init();
        }

        public GroupCreationViewModel()
        {
            init();
        }

        private void init()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            //if (dataService.CurrentClassPeriod != null)
            //{
            //    StudentsNotInGroup = new ObservableCollection<Person>(dataService.CurrentClassPeriod.ClassInformation.StudentList);
            //}

            SortedStudentsNotInGroup.Source = StudentsNotInGroup;
            SortDescription StudentNameSort = new SortDescription("FullName", ListSortDirection.Ascending);
            SortedStudentsNotInGroup.SortDescriptions.Add(StudentNameSort);

            Groups = new ObservableCollection<Group>();

            foreach(Person student in new ObservableCollection<Person>(StudentsNotInGroup))
            {
                if(GetDifferentiationGroup(student) != "")
                {
                    bool added = false;
                    foreach(Group existingGroup in Groups)
                    {
                        if(existingGroup.Label == GetDifferentiationGroup(student))
                        {
                            existingGroup.Add(student);
                            added = true;
                            break;
                        }
                    }
                    if(!added)
                    {
                        Group newGroup = new Group(GetDifferentiationGroup(student));
                        newGroup.Add(student);
                        AddGroupInOrder(newGroup);
                    }
                    StudentsNotInGroup.Remove(student);
                }
            }

            if(Groups.Count == 0)
            {
                Groups.Add(new Group("A"));
                Groups.Add(new Group("B"));
                Groups.Add(new Group("C"));
                Groups.Add(new Group("D"));
            }

            GroupChangeCommand = new Command<object[]>(OnGroupChangeCommandExecute);
            AddGroupCommand = new Command(OnAddGroupCommandExecute);
            RemoveGroupCommand = new Command<Group>(OnRemoveGroupCommandExecute);
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

        public static readonly PropertyData StudentsNotInGroupProperty = RegisterProperty("StudentsNotInGroup", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());
    
        public CollectionViewSource SortedStudentsNotInGroup
        {
            get { return GetValue<CollectionViewSource>(SortedStudentsNotInGroupProperty); }
            set { SetValue(SortedStudentsNotInGroupProperty, value); }
        }

        public static readonly PropertyData SortedStudentsNotInGroupProperty = RegisterProperty("SortedStudentsNotInGroup", typeof(CollectionViewSource), () => new CollectionViewSource());

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
            for(int i = 0; i < Groups.Count; i++)
            {
                string expectedLabel = "" + (char)('A' + i);
                if(Groups[i].Label != expectedLabel)
                {
                    Groups.Insert(i, new Group(expectedLabel));
                    return;
                }
            }

            // fall through to here if there are no gaps in the group labels
            string endLabel = "" + (char)('A' + Groups.Count);
            Groups.Add(new Group(endLabel));
        }

        public Command<Group> RemoveGroupCommand
        {
            get;
            private set;
        }

        public void OnRemoveGroupCommandExecute(Group removed)
        {
            foreach(Person student in removed.Members)
            {
                StudentsNotInGroup.Add(student);
            }
            Groups.Remove(removed);
        }

        private void AddGroupInOrder(Group newGroup)
        {
            for(int i = 0; i < Groups.Count; i++)
            {
                if(Groups[i].Label.CompareTo(newGroup.Label) > 0)
                {
                    Groups.Insert(i, newGroup);
                    return;
                }
            }
            //Fall through to here if the group comes after any/all existing groups.
            Groups.Add(newGroup);
        }

        private string GetDifferentiationGroup(Person student)
        {
            if(GroupType == "Temp")
            {
                return student.TempDifferentiationGroup;
            }
            else
            {
                return student.CurrentDifferentiationGroup;
            }
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

    public CollectionViewSource SortedMembers
    {
        get { return GetValue<CollectionViewSource>(SortedMembersProperty); }
        set { SetValue(SortedMembersProperty, value); }
    }

    public static readonly PropertyData SortedMembersProperty = RegisterProperty("SortedMembers", typeof(CollectionViewSource), () => new CollectionViewSource());

    public Group(string label)
    {
        Members = new ObservableCollection<Person>();
        SortedMembers.Source = Members;
        SortDescription StudentNameSort = new SortDescription("FullName", ListSortDirection.Ascending);
        SortedMembers.SortDescriptions.Add(StudentNameSort);
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