using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class ArrayHistoryAction : AHistoryActionBase
    {
        public enum ArrayActions
        {
            Cut,
            Divide,
            InkDivide,
            Rotate,
            Snap
        }
        
        #region Constructors
        public ArrayHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            var arrayCutActions = ArrayCutActions;
            var arrayDivisionActions = ArrayDivisionActions;
            var arrayRotateActions = ArrayRotateActions;
            var arraySnapActions = ArraySnapActions;

            if (arrayCutActions.Count + arrayDivisionActions.Count + arrayRotateActions.Count + arraySnapActions.Count != 1)
            {
                //throw error
            }

            if (arrayCutActions.Any())
            {
                ArrayAction = ArrayActions.Cut;
                var arrayCutAction = arrayCutActions.First();
                ArrayDimensions = new List<int>{};
                ChangedArrayDimensions = new List<int>{ };
            }
            else if (arrayDivisionActions.Any())
            {
                ArrayAction = ArrayActions.Divide;
                var arrayDivideAction = arrayDivisionActions.First();
                ArrayDimensions = new List<int> { };
                ChangedArrayDimensions = new List<int> { };
            }
            else if (arrayRotateActions.Any())
            {
                ArrayAction = ArrayActions.Rotate;
                var arrayRotateAction = arrayRotateActions.First();
                ArrayDimensions = new List<int> { };
                ChangedArrayDimensions = new List<int> { };
            }
            else if (arraySnapActions.Any())
            {
                ArrayAction = ArrayActions.Snap;
                

                ArrayDimensions = new List<int> { };
                ChangedArrayDimensions = new List<int> { };
            }
        }

        /// <summary>Initializes <see cref="ArrayHistoryAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayHistoryAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion //Constructors

        #region Properties
        /// <summary>
        /// The type of Array Action this HistoryAction represents.
        /// </summary>
        public ArrayActions ArrayAction
        {
            get { return GetValue<ArrayActions>(ArrayActionProperty); }
            set { SetValue(ArrayActionProperty, value); }
        }

        public static readonly PropertyData ArrayActionProperty = RegisterProperty("ArrayAction", typeof(ArrayActions));

        public List<int> ArrayDimensions
        {
            get { return GetValue<List<int>>(ArrayDimensionsProperty); }
            set { SetValue(ArrayDimensionsProperty, value);}
        }

        public static readonly PropertyData ArrayDimensionsProperty = RegisterProperty("ArrayDimensions", typeof(List<int>));

        public List<int> ChangedArrayDimensions
        {
            get { return GetValue<List<int>>(ChangedArrayDimensionsProperty); }
            set { SetValue(ChangedArrayDimensionsProperty, value); }
        }

        public static readonly PropertyData ChangedArrayDimensionsProperty = RegisterProperty("ChangedArrayDimensions", typeof(List<int>));

        public override string CodedValue
        {
            get 
            {
                switch (ArrayAction)
                {
                    case ArrayActions.Cut:
                        var arrayCutAction = ArrayCutActions.First();
                        var cutArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(arrayCutAction.CutPageObjectID) as CLPArray;
                        var halfArrays = arrayCutAction.HalvedPageObjectIDs.Select(h => ParentPage.GetPageObjectByIDOnPageOrInHistory(h) as CLPArray).ToList();
                        var cutDirection = (cutArray.Rows == halfArrays[0].Rows) ? " v" : "";
                        return string.Format("ARR cut[{0}x{1}: {2}x{3}, {4}x{5}{6}]", cutArray.Rows, cutArray.Columns,
                            halfArrays[0].Rows, halfArrays[0].Columns, halfArrays[1].Rows, halfArrays[1].Columns, cutDirection);
                    
                    case ArrayActions.Divide:
                        return string.Format("ARR divide[{0}x{1}: {2}x{3}, {4}x{5}]", ArrayDimensions[0], ArrayDimensions[1],
                            ChangedArrayDimensions[0], ChangedArrayDimensions[1], ChangedArrayDimensions[2], ChangedArrayDimensions[3]);
                    
                    case ArrayActions.InkDivide:
                        return string.Format("ARR ink divide[{0}x{1}: {2}x{3}, {4}x{5}]", ArrayDimensions[0], ArrayDimensions[1],
                            ChangedArrayDimensions[0], ChangedArrayDimensions[1], ChangedArrayDimensions[2], ChangedArrayDimensions[3]);
                    
                    case ArrayActions.Rotate:
                        var arrayRotateAction = ArrayRotateActions.First();

                        return string.Format("ARR rotate[{0}x{1}:{2}x{3}", ArrayDimensions[0], ArrayDimensions[1], ChangedArrayDimensions[0], ChangedArrayDimensions[1]);
                    
                    case ArrayActions.Snap:
                        var arraySnapAction = ArraySnapActions.First();
                        var direction = arraySnapAction.IsHorizontal;
                        var persistingArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(arraySnapAction.PersistingArrayID) as CLPArray;
                        var snappedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(arraySnapAction.SnappedArrayID) as CLPArray;
                        var persistingArrayRows = persistingArray.Rows - snappedArray.Rows;
                        var persistingArrayColumns = snappedArray.Columns;
                        if (!direction)
                        {
                            persistingArrayRows = persistingArray.Rows;
                            persistingArrayColumns = persistingArray.Columns - snappedArray.Columns;
                        }
                        return string.Format("ARR snap[{0}x{1}, {2}x{3}:{4}x{5}]", snappedArray.Rows, snappedArray.Columns,
                            persistingArrayRows, persistingArrayColumns, persistingArray.Rows, persistingArray.Columns);
                    default:
                        return "ARR modified";
                }
            }
        }

        #endregion //Properties

        #region Calculated Properties

        public List<PageObjectCutHistoryItem> ArrayCutActions
        {
            get { return HistoryItems.OfType<PageObjectCutHistoryItem>().ToList(); }
        }

        public List<CLPArrayDivisionsChangedHistoryItem> ArrayDivisionActions
        {
            get { return HistoryItems.OfType<CLPArrayDivisionsChangedHistoryItem>().ToList();}
        }
        public List<CLPArrayRotateHistoryItem> ArrayRotateActions
        {
            get { return HistoryItems.OfType<CLPArrayRotateHistoryItem>().ToList(); }
        }

        public List<CLPArraySnapHistoryItem> ArraySnapActions
        {
            get { return HistoryItems.OfType<CLPArraySnapHistoryItem>().ToList(); }

        }

        #endregion //Calculated Properties
    }
}
