using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayDivisionsChanged : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArrayDivisionsChanged(ICLPPage parentPage, string arrayUniqueID, List<CLPArrayDivision> addedDivisions, List<CLPArrayDivision> removedDivisions)
            : base(parentPage)
        {
            ArrayUniqueID = arrayUniqueID;
            AddedDivisions = addedDivisions;
            RemovedDivisions = removedDivisions;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArrayDivisionsChanged(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// UniqueID of the Array whose divisions have been modified
        /// </summary>
        public string ArrayUniqueID
        {
            get { return GetValue<string>(ArrayUniqueIDProperty); }
            set { SetValue(ArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData ArrayUniqueIDProperty = RegisterProperty("ArrayUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// New ArrayDivisions added to Array
        /// </summary>
        public List<CLPArrayDivision> AddedDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(AddedDivisionsProperty); }
            set { SetValue(AddedDivisionsProperty, value); }
        }

        public static readonly PropertyData AddedDivisionsProperty = RegisterProperty("AddedDivisions", typeof(List<CLPArrayDivision>));

        /// <summary>
        /// ArrayDivisions removed from Array
        /// </summary>
        public List<CLPArrayDivision> RemovedDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(RemovedDivisionsProperty); }
            set { SetValue(RemovedDivisionsProperty, value); }
        }

        public static readonly PropertyData RemovedDivisionsProperty = RegisterProperty("RemovedDivisions", typeof(List<CLPArrayDivision>));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
            if(array != null)
            {
                if((AddedDivisions.Any() && AddedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal) ||
                   (RemovedDivisions.Any() && RemovedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal))
                {
                    foreach(var clpArrayDivision in AddedDivisions)
                    {
                        array.HorizontalDivisions.Remove(clpArrayDivision);
                    }
                    foreach(var clpArrayDivision in RemovedDivisions)
                    {
                        array.HorizontalDivisions.Add(clpArrayDivision);
                    }
                }
                else
                {
                    foreach(var clpArrayDivision in AddedDivisions)
                    {
                        array.VerticalDivisions.Remove(clpArrayDivision);
                    }
                    foreach(var clpArrayDivision in RemovedDivisions)
                    {
                        array.VerticalDivisions.Add(clpArrayDivision);
                    }
                }
            }
            else
            {
                Logger.Instance.WriteToLog("Array not found on page for UndoAction");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
            if(array != null)
            {
                if((AddedDivisions.Any() && AddedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal) ||
                   (RemovedDivisions.Any() && RemovedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal))
                {
                    foreach(var clpArrayDivision in AddedDivisions)
                    {
                        array.HorizontalDivisions.Add(clpArrayDivision);
                    }
                    foreach(var clpArrayDivision in RemovedDivisions)
                    {
                        array.HorizontalDivisions.Remove(clpArrayDivision);
                    }
                }
                else
                {
                    foreach(var clpArrayDivision in AddedDivisions)
                    {
                        array.VerticalDivisions.Add(clpArrayDivision);
                    }
                    foreach(var clpArrayDivision in RemovedDivisions)
                    {
                        array.VerticalDivisions.Remove(clpArrayDivision);
                    }
                }
            }
            else
            {
                Logger.Instance.WriteToLog("Array not found on page for RedoAction");
            }
        }

        #endregion //Methods
    }
}
