﻿using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    [AllowNonSerializableMembers]
    public class CLPGridDisplay : ACLPDisplayBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPGridDisplay()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGridDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Pages on the GridDisplay.
        /// </summary>
        public ObservableCollection<ICLPPage> Pages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<ICLPPage>), () => new ObservableCollection<ICLPPage>(), includeInSerialization:false);

        #endregion //Properties

        #region Methods

        public override void AddPageToDisplay(ICLPPage page)
        {
            DisplayPageIDs.Add(page.UniqueID);
            Pages.Add(page);
        }

        public override void RemovePageFromDisplay(ICLPPage page)
        {
            DisplayPageIDs.Remove(page.UniqueID);
            Pages.Remove(page);
        }

        #endregion //Methods
    }
}
