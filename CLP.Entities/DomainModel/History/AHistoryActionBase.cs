﻿using System;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public abstract class AHistoryActionBase : ASerializableBase, IHistoryAction
    {
        #region Constructors

        /// <summary>Initializes <see cref="AHistoryActionBase" /> from scratch.</summary>
        protected AHistoryActionBase()
        {
            ID = Guid.NewGuid().ToCompactID();
            CreationTime = DateTime.Now;
        }

        /// <summary>Initializes <see cref="APageObjectBase" /> using <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> belongs to.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        protected AHistoryActionBase(CLPPage parentPage, Person owner)
            : this()
        {
            ParentPage = parentPage;
            OwnerID = owner.ID;
        }

        #endregion //Constructors

        #region Properties

        #region ID Properties

        /// <summary>Unique Identifier for the <see cref="AHistoryActionBase" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

        /// <summary>Location of the <see cref="IHistoryAction" /> in the entirety of history, including UndoActions and RedoActions.</summary>
        public int HistoryActionIndex
        {
            get { return GetValue<int>(HistoryIndexProperty); }
            set { SetValue(HistoryIndexProperty, value); }
        }

        public static readonly PropertyData HistoryIndexProperty = RegisterProperty("HistoryActionIndex", typeof(int), -1);

        /// <summary>Cached value of FormattedValue with correct page state.</summary>
        public string CachedFormattedValue
        {
            get { return GetValue<string>(CachedFormattedValueProperty); }
            set { SetValue(CachedFormattedValueProperty, value); }
        }

        public static readonly PropertyData CachedFormattedValueProperty = RegisterProperty("CachedFormattedValue", typeof(string), string.Empty);

        #endregion // ID Properties

        #region Backing

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="AHistoryActionBase" />.</summary>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>Creation Time of the HistoryAction.</summary>
        public DateTime CreationTime
        {
            get { return GetValue<DateTime>(CreationTimeProperty); }
            set { SetValue(CreationTimeProperty, value); }
        }

        public static readonly PropertyData CreationTimeProperty = RegisterProperty("CreationTime", typeof(DateTime));

        /// <summary>The <see cref="AHistoryActionBase" />'s parent <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public CLPPage ParentPage    // TODO: Have to set after Deserialization, otherwise null
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #endregion // Backing

        #region Calculated Properties

        public virtual int AnimationDelay => 50;

        public string FormattedValue
        {
            get
            {
                var formattedValue = $"#{HistoryActionIndex}, {FormattedReport}";
                if (!formattedValue.Equals(CachedFormattedValue))
                {
                    CachedFormattedValue = formattedValue;
                }

                return formattedValue;
            }
        }

        protected abstract string FormattedReport { get; }

        #endregion // Calculated Properties

        #endregion //Properties

        #region Methods

        public void Undo(bool isAnimationUndo = false)
        {
            if (ParentPage == null)
            {
                return;
            }

            ParentPage.IsTagAddPrevented = true;
            UndoAction(isAnimationUndo);
            ParentPage.IsTagAddPrevented = false;
        }

        protected abstract void UndoAction(bool isAnimationUndo);

        public void Redo(bool isAnimationRedo = false)
        {
            if (ParentPage == null)
            {
                return;
            }
            ParentPage.IsTagAddPrevented = true;
            RedoAction(isAnimationRedo);
            CachedFormattedValue = FormattedValue;
            ParentPage.IsTagAddPrevented = false;
        }

        protected abstract void RedoAction(bool isAnimationRedo);

        public abstract IHistoryAction CreatePackagedHistoryAction();

        public abstract void UnpackHistoryAction();

        public virtual bool IsUsingTrashedPageObject(string id)
        {
            return false;
        }

        public virtual bool IsUsingTrashedInkStroke(string id)
        {
            return false;
        }

        #endregion //Methods
    }
}