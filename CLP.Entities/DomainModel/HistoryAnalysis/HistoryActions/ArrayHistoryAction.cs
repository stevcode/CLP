﻿using System;
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
        public ArrayHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems, List<string> originalArrayIdentifiers, List<string> newArrayIdentifiers)
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            OriginalArrayIDs = originalArrayIdentifiers;
            NewArrayIDs = newArrayIdentifiers;
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
            }
            else if (arrayDivisionActions.Any())
            {
                ArrayAction = ArrayActions.Divide;
                var arrayDivideAction = arrayDivisionActions.First();
            }
            else if (arrayRotateActions.Any())
            {
                ArrayAction = ArrayActions.Rotate;
                var arrayRotateAction = arrayRotateActions.First();
            }
            else if (arraySnapActions.Any())
            {
                ArrayAction = ArrayActions.Snap;
                
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

        public List<string> OriginalArrayIDs
        {
            get { return GetValue<List<string>>(OriginalArrayIDsProperty); }
            set { SetValue(OriginalArrayIDsProperty, value); }
        }

        public static readonly PropertyData OriginalArrayIDsProperty = RegisterProperty("OriginalArrayIDs", typeof (List<string>));

        public List<string> NewArrayIDs
        {
            get { return GetValue<List<string>>(NewArrayIDsProperty); }
            set { SetValue(NewArrayIDsProperty, value); }
        }

        public static readonly PropertyData NewArrayIDsProperty = RegisterProperty("NewArrayIDs", typeof (List<string>));

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
                       
                        var halfRows1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Rows : halfArrays[0].Rows - halfArrays[1].Rows;
                        var halfColumns1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Columns - halfArrays[1].Columns : halfArrays[0].Columns;
                        var cutDirection = (cutArray.Rows == halfArrays[1].Rows) ? ", v" : "";
                        return string.Format("ARR cut[{0}x{1}{7}: {2}x{3}{8}, {4}x{5}{9}{6}]", cutArray.Rows, cutArray.Columns,
                            halfRows1, halfColumns1, halfArrays[1].Rows, halfArrays[1].Columns, cutDirection,
                            OriginalArrayIDs[0], NewArrayIDs[0], NewArrayIDs[1]);
                    
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
                        var persistingArrayRows = (direction) ? persistingArray.Rows - snappedArray.Rows : persistingArray.Rows;
                        var persistingArrayColumns = (direction) ? snappedArray.Columns : persistingArray.Columns - snappedArray.Columns;
                       
                        return string.Format("ARR snap[{0}x{1}{6}, {2}x{3}{7}:{4}x{5}{8}]", snappedArray.Rows, snappedArray.Columns,
                            persistingArrayRows, persistingArrayColumns, persistingArray.Rows, persistingArray.Columns,
                            OriginalArrayIDs[0], OriginalArrayIDs[1], NewArrayIDs[0]);
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
