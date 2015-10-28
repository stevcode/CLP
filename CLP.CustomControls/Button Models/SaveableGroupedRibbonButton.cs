using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.CustomControls
{
    [Serializable]
    public class SaveableGroupedRibbonButton : ASavableButtonBase
    {
         #region Constructor

        public SaveableGroupedRibbonButton() { }

        /// <summary>Initializes a new object from scratch.</summary>
        public SaveableGroupedRibbonButton(string text, string packUri, string groupName)
            : base(text, packUri) { GroupName = groupName; }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        private SaveableGroupedRibbonButton(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Which Group the Button belongs to.
        /// </summary>
        public string GroupName
        {
            get { return GetValue<string>(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly PropertyData GroupNameProperty = RegisterProperty("GroupName", typeof (string)); 

        #endregion //Properties
    }
}