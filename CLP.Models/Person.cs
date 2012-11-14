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
    public class Person : DataObjectBase<Person>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public Person() { }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected Person(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        /// <summary>
        /// Register the FullName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        /// <summary>
        /// Register the Handedness property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<byte> HeadShotByteSource
        {
            get { return GetValue<List<byte>>(HeadShotByteSourceProperty); }
            set { SetValue(HeadShotByteSourceProperty, value); }
        }

        /// <summary>
        /// Register the HeadShotByteSource property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HeadShotByteSourceProperty = RegisterProperty("HeadShotByteSource", typeof(List<byte>), () => new List<byte>());

        #endregion

    }
}
