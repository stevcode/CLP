using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryRemoveObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryRemoveObject(ICLPPageObject item) : base(HistoryItemType.RemoveObject)
        {
            try
            {
                PageObject = item;
                ObjectId = item.UniqueID;
            }catch(Exception e){
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryRemoveObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Page object removed in this historical event; null if this event is in the Future
        /// </summary>
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = 
            RegisterProperty("PageObject", typeof(ICLPPageObject), null);

        /// <summary>
        /// Unique ID of page object removed in this historical event
        /// </summary>
        public String ObjectId
        {
            get { return GetValue<String>(ObjectIdProperty); }
            set { SetValue(ObjectIdProperty, value); }
        }

        public static readonly PropertyData ObjectIdProperty =
            RegisterProperty("ObjectId", typeof(String), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryAddObject(ObjectId);
        }

        public override CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryRemoveObject(PageObject);
        }

        override public void Undo(CLPPage page)
        {
            //var metaPast = page.PageHistory.MetaPast;
            //var future = page.PageHistory.Future;
            
            if(PageObject == null)
            {
               /* if(metaPast.Count>0){
                    var last = metaPast.Pop();
                    last.Undo(page);
                    future.Push(last);
                }*/
                Console.WriteLine("RemoveObject Undo Failure: No object to add.");
                return;
            }
            page.PageObjects.Add(PageObject);
            /////////////////////////////////
            /*if(metaPast.Count > 0)
            {
                CLPHistoryItem prev = metaPast.Peek();
                if(this.singleCut == true && prev.singleCut == true)
                {
                    metaPast.Pop();
                    prev.Undo(page);
                    future.Push(prev);
                }

            }*/
            /////////////////////////////////
            PageObject = null; //forget it because we don't need it anymore
        }

        
        override public void Redo(CLPPage page)
        {
            PageObject = GetPageObjectByUniqueID(page, ObjectId);  //remember in case we need to add it back later
            //var future = page.PageHistory.Future;
            //var metaPast = page.PageHistory.MetaPast;
            
            if(PageObject == null)
            {
               /* if(future.Count > 0)
                {
                    var next = future.Pop();
                    next.Redo(page);
                    metaPast.Push(next);
                }*/
                Console.WriteLine("RemoveObject Redo Failure: No object to remove.");
                return;
            }
            page.PageObjects.Remove(PageObject);
            /////////////////////////////////
            /*if(future.Count > 0)
            {
                CLPHistoryItem next = future.Peek();
                if(this.singleCut == true && next.singleCut == true)
                {
                    future.Pop();
                    next.Redo(page);
                    metaPast.Push(next);
                }
            }*/
            /////////////////////////////////
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryRemoveObject) ||
                (obj as CLPHistoryRemoveObject).ObjectId != ObjectId)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
