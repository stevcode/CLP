using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    [AllowNonSerializableMembers]
    public class CLPMirrorDisplay : ACLPDisplayBase
    {
        #region Constructors
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPMirrorDisplay()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPMirrorDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// The page currently displayed on the MirrorDisplay.
        /// </summary>
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set
            {
                SetValue(CurrentPageProperty, value);
                DisplayPageIDs.Clear();
                DisplayPageIDs.Add(value.UniqueID);
            }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage), includeInSerialization:false);

        #endregion //Properties

    }
}
