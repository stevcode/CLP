using System;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;

namespace CLP.Models
{
    [Serializable]
    public class Group : DataObjectBase<Group>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public Group()
        {
            GroupID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected Group(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the Group.
        /// </summary>
        public string GroupName
        {
            get { return GetValue<string>(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly PropertyData GroupNameProperty = RegisterProperty("GroupName", typeof(string), "");

        /// <summary>
        /// UniqueID of the Group.
        /// </summary>
        public string GroupID
        {
            get { return GetValue<string>(GroupIDProperty); }
            set { SetValue(GroupIDProperty, value); }
        }

        public static readonly PropertyData GroupIDProperty = RegisterProperty("GroupID", typeof(string), null);

        /// <summary>
        /// List of people in the Group.
        /// </summary>
        public ObservableCollection<Person> GroupMembers
        {
            get { return GetValue<ObservableCollection<Person>>(GroupMembersProperty); }
            set { SetValue(GroupMembersProperty, value); }
        }

        public static readonly PropertyData GroupMembersProperty = RegisterProperty("GroupMembers", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        #endregion //Properties
    }
}
