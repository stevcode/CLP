using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Catel.Data;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    abstract public class  CLPStampBase : CLPPageObjectBase
    {
        protected CLPStampBase() : base()
        {
            IsAnchored = true;
            Parts = 0;
            base.Position = new Point(10, 10);
            Width = 150;
        }

        #region Properties

        //returns true if stamp is anchor/placed by teacher
        //returns false if stamp is a copy of the anchor; moved by the student
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsAnchored
        {
            get { return GetValue<bool>(IsAnchoredProperty); }
            set { SetValue(IsAnchoredProperty, value); }
        }

        /// <summary>
        /// Register the IsAnchored property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAnchoredProperty = RegisterProperty("IsAnchored", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        /// <summary>
        /// Register the Parts property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 0);

       

        #endregion //Properties
    }
}