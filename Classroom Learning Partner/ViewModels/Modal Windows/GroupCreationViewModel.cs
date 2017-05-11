using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GroupCreationViewModel : ViewModelBase
    {
        public enum GroupTypes
        {
            Default,
            Temporary
        }

        #region Nested Class

        public class Group : ViewModelBase
        {
            #region Constructor

            public Group(string label, GroupTypes groupType)
            {
                Label = label;
                SpecificGroupType = groupType;

                var studentNameSort = new SortDescription("FullName", ListSortDirection.Ascending);
                SortedMembers.Source = Members;
                SortedMembers.SortDescriptions.Add(studentNameSort);
            }

            #endregion // Constructor

            #region Properties

            public GroupTypes SpecificGroupType
            {
                get { return GetValue<GroupTypes>(SpecificGroupTypeProperty); }
                set { SetValue(SpecificGroupTypeProperty, value); }
            }

            public static readonly PropertyData SpecificGroupTypeProperty = RegisterProperty("SpecificGroupType", typeof(GroupTypes), GroupTypes.Default);

            public ObservableCollection<Person> Members
            {
                get { return GetValue<ObservableCollection<Person>>(MembersProperty); }
                set { SetValue(MembersProperty, value); }
            }

            public static readonly PropertyData MembersProperty = RegisterProperty("Members", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

            #endregion // Properties

            #region Bindings

            public string Label
            {
                get { return GetValue<string>(LabelProperty); }
                set { SetValue(LabelProperty, value); }
            }

            public static readonly PropertyData LabelProperty = RegisterProperty("Label", typeof(string), string.Empty);

            public CollectionViewSource SortedMembers
            {
                get { return GetValue<CollectionViewSource>(SortedMembersProperty); }
                set { SetValue(SortedMembersProperty, value); }
            }

            public static readonly PropertyData SortedMembersProperty = RegisterProperty("SortedMembers", typeof(CollectionViewSource), () => new CollectionViewSource());

            #endregion // Bindings

            #region Methods

            public void Add(Person student)
            {
                Members.Add(student);
                if (SpecificGroupType == GroupTypes.Default)
                {
                    student.CurrentDifferentiationGroup = Label;
                }
                else
                {
                    student.TemporaryDifferentiationGroup = Label;
                }
            }

            public void Remove(Person student)
            {
                Members.Remove(student);
                if (SpecificGroupType == GroupTypes.Default)
                {
                    student.CurrentDifferentiationGroup = "";
                }
                else
                {
                    student.TemporaryDifferentiationGroup = "";
                }
            }

            #endregion // Methods
        }

        #endregion // Nested Class

        #region Constructor

        public GroupCreationViewModel(ClassRoster classRoster, GroupTypes groupType)
        {
            ClassRoster = classRoster;
            GroupType = groupType;

            InitializeStudentGroups();
            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model]
        public ClassRoster ClassRoster
        {
            get { return GetValue<ClassRoster>(ClassRosterProperty); }
            set { SetValue(ClassRosterProperty, value); }
        }

        public static readonly PropertyData ClassRosterProperty = RegisterProperty("ClassRoster", typeof(ClassRoster));

        /// <summary>Auto-Mapped property of the Roster Model.</summary>
        [ViewModelToModel("ClassRoster")]
        public ObservableCollection<Person> ListOfStudents
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfStudentsProperty); }
            set { SetValue(ListOfStudentsProperty, value); }
        }

        public static readonly PropertyData ListOfStudentsProperty = RegisterProperty("ListOfStudents", typeof(ObservableCollection<Person>));

        #endregion // Model

        #region Bindings

        /// <summary>The group type the panel acts upon.</summary>
        public GroupTypes GroupType
        {
            get { return GetValue<GroupTypes>(GroupTypeProperty); }
            set { SetValue(GroupTypeProperty, value); }
        }

        public static readonly PropertyData GroupTypeProperty = RegisterProperty("GroupType", typeof(GroupTypes), GroupTypes.Default);

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

        public ObservableCollection<Group> Groups
        {
            get { return GetValue<ObservableCollection<Group>>(GroupsProperty); }
            set { SetValue(GroupsProperty, value); }
        }

        public static readonly PropertyData GroupsProperty = RegisterProperty("Groups", typeof(ObservableCollection<Group>), () => new ObservableCollection<Group>());

        #endregion // Bindings

        #region Methods

        private void InitializeStudentGroups()
        {
            StudentsNotInGroup.Clear();
            StudentsNotInGroup.AddRange(ListOfStudents);

            var studentNameSort = new SortDescription("FullName", ListSortDirection.Ascending);
            SortedStudentsNotInGroup.Source = StudentsNotInGroup;
            SortedStudentsNotInGroup.SortDescriptions.Add(studentNameSort);

            foreach (var student in StudentsNotInGroup.ToList())
            {
                if (GetDifferentiationGroup(student) == "")
                {
                    continue;
                }

                var isStudentPartOfExistingGroup = false;
                foreach (var existingGroup in Groups)
                {
                    if (existingGroup.Label != GetDifferentiationGroup(student))
                    {
                        continue;
                    }

                    existingGroup.Add(student);
                    isStudentPartOfExistingGroup = true;
                    break;
                }

                if (!isStudentPartOfExistingGroup)
                {
                    var newGroup = new Group(GetDifferentiationGroup(student), GroupType);
                    newGroup.Add(student);
                    AddGroupInOrder(newGroup);
                }

                StudentsNotInGroup.Remove(student);
            }

            if (Groups.Any())
            {
                return;
            }

            Groups.Add(new Group("A", GroupType));
            Groups.Add(new Group("B", GroupType));
            Groups.Add(new Group("C", GroupType));
            Groups.Add(new Group("D", GroupType));
        }

        private void AddGroupInOrder(Group newGroup)
        {
            for (var i = 0; i < Groups.Count; i++)
            {
                if (string.Compare(Groups[i].Label, newGroup.Label, StringComparison.Ordinal) <= 0)
                {
                    continue;
                }

                Groups.Insert(i, newGroup);
                return;
            }

            //Fall through to here if the group comes after any/all existing groups.
            Groups.Add(newGroup);
        }

        private string GetDifferentiationGroup(Person student)
        {
            return GroupType == GroupTypes.Temporary ? student.TemporaryDifferentiationGroup : student.CurrentDifferentiationGroup;
        }

        #endregion // Methods

        #region Commands

        private void InitializeCommands()
        {
            GroupChangeCommand = new Command<object[]>(OnGroupChangeCommandExecute);
            AddGroupCommand = new Command(OnAddGroupCommandExecute);
            RemoveGroupCommand = new Command<Group>(OnRemoveGroupCommandExecute);
        }

        public Command<object[]> GroupChangeCommand { get; private set; }

        public void OnGroupChangeCommandExecute(object[] parameters)
        {
            var newGroup = parameters[0] as Group;
            var movingPerson = parameters[1] as Person;
            foreach (var group in Groups)
            {
                if (!group.Members.Contains(movingPerson))
                {
                    continue;
                }

                group.Remove(movingPerson);
                if (group != newGroup)
                {
                    continue;
                }

                StudentsNotInGroup.Add(movingPerson);
                return;
            }

            if (StudentsNotInGroup.Contains(movingPerson))
            {
                StudentsNotInGroup.Remove(movingPerson);
            }

            newGroup.Add(movingPerson);
        }

        public Command AddGroupCommand { get; private set; }

        public void OnAddGroupCommandExecute()
        {
            for (var i = 0; i < Groups.Count; i++)
            {
                var expectedLabel = "" + (char)('A' + i);
                if (Groups[i].Label == expectedLabel)
                {
                    continue;
                }

                Groups.Insert(i, new Group(expectedLabel, GroupType));
                return;
            }

            // fall through to here if there are no gaps in the group labels
            var endLabel = "" + (char)('A' + Groups.Count);
            Groups.Add(new Group(endLabel, GroupType));
        }

        public Command<Group> RemoveGroupCommand { get; private set; }

        public void OnRemoveGroupCommandExecute(Group removed)
        {
            foreach (var student in removed.Members)
            {
                StudentsNotInGroup.Add(student);
            }

            Groups.Remove(removed);
        }

        #endregion // Commands
    }
}