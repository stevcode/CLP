using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public enum Handedness
    {
        Left,
        Right
    }

    /// <summary>
    /// Person Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class Person : ModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public Person()
        {
            CurrentMachineName = Environment.MachineName;
            UniqueID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected Person(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region Persistent

        /// <summary>
        /// UniqueID associated with the Person.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), null);

        /// <summary>
        /// Full Name of the Person, delimited by spaces.
        /// </summary>
        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), "NoName");

        /// <summary>
        /// Left or Right Handed.
        /// </summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>
        /// Picture of the Person.
        /// </summary>
        public List<byte> HeadShotByteSource
        {
            get { return GetValue<List<byte>>(HeadShotByteSourceProperty); }
            set { SetValue(HeadShotByteSourceProperty, value); }
        }

        public static readonly PropertyData HeadShotByteSourceProperty = RegisterProperty("HeadShotByteSource", typeof(List<byte>), () => new List<byte>());

        #endregion //Persistent

        #region Change From Session to Session

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
        /// FriendlyName of the Machine the Person is currently using.
        /// </summary>
        public string CurrentMachineName
        {
            get { return GetValue<string>(CurrentMachineNameProperty); }
            set { SetValue(CurrentMachineNameProperty, value); }
        }

        public static readonly PropertyData CurrentMachineNameProperty = RegisterProperty("CurrentMachineName", typeof(string), null);

        /// <summary>
        /// Stringified URI address of the endpoint of this student's StudentService.
        /// </summary>
        public string CurrentMachineAddress
        {
            get { return GetValue<string>(CurrentMachineAddressProperty); }
            set { SetValue(CurrentMachineAddressProperty, value); }
        }

        public static readonly PropertyData CurrentMachineAddressProperty = RegisterProperty("CurrentMachineAddress", typeof(string), null);

        #endregion //Change From Session to Session

        #endregion //Properties

    }
}
