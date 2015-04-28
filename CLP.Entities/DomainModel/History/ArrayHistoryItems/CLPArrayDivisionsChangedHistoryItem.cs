using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayDivisionsChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionsChangedHistoryItem" /> from scratch.
        /// </summary>
        public CLPArrayDivisionsChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionsChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayDivisionsChangedHistoryItem(CLPPage parentPage, Person owner, string arrayID, List<CLPArrayDivision> addedDivisions, List<CLPArrayDivision> removedDivisions)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            AddedDivisions = addedDivisions;
            RemovedDivisions = removedDivisions;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayDivisionsChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        /// Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryItem" /> modifies.
        /// </summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string));

        /// <summary>
        /// ArrayDivisions added to Array
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

        public override string FormattedValue
        {
            get
            {
                //TODO: Clean up
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                if (array == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                }
                
                var addHorizontal = 0;
                var addVertical = 0;
                var addHorizontalString = string.Empty;
                var addVerticalString = string.Empty;
                foreach (var addedDivision in AddedDivisions)
                {
                    if (addedDivision.Orientation == ArrayDivisionOrientation.Horizontal)
                    {
                        addHorizontal += 1;
                    }
                    else
                    {
                        addVertical += 1;
                    }
                }

                addHorizontalString = string.Format("horizontally {0} times", addHorizontal);               
                addVerticalString = string.Format("vertically {0} times", addVertical);

                var dividedArray = string.Empty;
                if(addHorizontal > 0 || addVertical > 0)
                {
                    dividedArray = string.Format("Divided array ({0} by {1}) {2}, {3}.",
                        array.Rows, array.Columns, addHorizontalString, addVerticalString);
                }

                var removeHorizontal = 0;
                var removeVertical = 0;
                var removeHorizontalString = string.Empty;
                var removeVerticalString = string.Empty;
                foreach (CLPArrayDivision removedDivision in RemovedDivisions)
                {
                    if (removedDivision.Orientation == ArrayDivisionOrientation.Horizontal)
                    {
                        removeHorizontal += 1;
                    }
                    else
                    {
                        removeVertical += 1;
                    }
                }


                removeHorizontalString = string.Format("{0} horizontal divisions", removeHorizontal);
                removeVerticalString = string.Format("{0} vertical divisions", removeVertical);

                var removedDivisions = string.Empty;
                if(removeHorizontal > 0 || removeVertical > 0)
                {
                    removedDivisions = string.Format("Removed {0}, {1}.",
                        removeHorizontalString, removeVerticalString);
                }


                var formattedValue = string.Format("Index #{0}, {1}{2}", 
                    HistoryIndex, dividedArray, removedDivisions);
                return formattedValue;
            }
        }
        
        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            UndoAction(false);
        }

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if(array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                return;
            }

            if (!AddedDivisions.Any() && 
                !RemovedDivisions.Any())
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing AddedDivisions and RemovedDivisions.", HistoryIndex);
                return;
            }

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

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if(array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                return;
            }

            if (!AddedDivisions.Any() &&
                !RemovedDivisions.Any())
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing AddedDivisions and RemovedDivisions.", HistoryIndex);
                return;
            }

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

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayDivisionsChangedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}