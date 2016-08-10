using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    [Obsolete("Use ObjectsOnPageChangedHistoryItem instead.")]
    public class PageObjectsRemovedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageObjectsRemovedHistoryItem" /> from scratch.
        /// </summary>
        public PageObjectsRemovedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="PageObjectsRemovedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public PageObjectsRemovedHistoryItem(CLPPage parentPage, Person owner, List<IPageObject> removedPageObjects)
            : base(parentPage, owner)
        {
            foreach(var removedPageObject in removedPageObjects)
            {
                PageObjectIDs.Add(removedPageObject.ID);
                ParentPage.History.TrashedPageObjects.Add(removedPageObject);
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PageObjectsRemovedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 400; }
        }

        /// <summary>
        /// List of all the IDs of the <see cref="IPageObject" />s added.
        /// </summary>
        public List<string> PageObjectIDs
        {
            get { return GetValue<List<string>>(PageObjectIDsProperty); }
            set { SetValue(PageObjectIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// List of the pageObjects that were removed from the page as a result of the UndoAction(). Cleared on Redo().
        /// </summary>
        [XmlIgnore]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        public override string FormattedValue
        {
            get
            {
                List<string> PageObjectTypes = new List<string>();
                try
                {
                    foreach(var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByID(pageObjectID)))
                    {
                        PageObjectTypes.Add(pageObject.GetType().ToString());
                    }
                }
                catch(Exception)
                {
                }
                
                string formattedValue = string.Format("Index # {0}, Removed {1} from page.", HistoryIndex, string.Join(", ", PageObjectTypes));
                return formattedValue;
            }
        }
        
        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            try
            {
                foreach (var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetVerifiedPageObjectInTrashByID(pageObjectID)))
                {
                    ParentPage.History.TrashedPageObjects.Remove(pageObject);
                    ParentPage.PageObjects.Add(pageObject);
                    pageObject.OnAdded(true);
                }
            }
            catch(Exception)
            {
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            try
            {
                foreach (var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetVerifiedPageObjectOnPageByID(pageObjectID)))
                {
                    ParentPage.PageObjects.Remove(pageObject);
                    pageObject.OnDeleted();
                    ParentPage.History.TrashedPageObjects.Add(pageObject);
                }
            }
            catch(Exception)
            {
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = this.DeepCopy();
            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        public bool IsUsingTrashedPageObject(string id, bool isUndoItem) { return isUndoItem && PageObjectIDs.Contains(id); }

        #endregion //Methods

        protected override void ConversionUndoAction()
        {

        }
    }
}