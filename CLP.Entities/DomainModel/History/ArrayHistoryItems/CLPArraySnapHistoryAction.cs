using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArraySnapHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArraySnapHistoryAction" /> from scratch.</summary>
        public CLPArraySnapHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArraySnapHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public CLPArraySnapHistoryAction(CLPPage parentPage, Person owner, ACLPArrayBase persistingArray, ACLPArrayBase snappedArray, bool isHorizontal)
            : base(parentPage, owner)
        {
            IsHorizontal = isHorizontal;
            SnappedArrayID = snappedArray.ID;
            SnappedArraySquareSize = snappedArray.GridSquareSize;
            parentPage.History.TrashedPageObjects.Add(snappedArray);

            PersistingArrayID = persistingArray.ID;
            PersistingArrayDivisionBehavior = persistingArray.IsDivisionBehaviorOn;
            PersistingArrayHorizontalDivisions =
                persistingArray.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            PersistingArrayVerticalDivisions =
                persistingArray.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            PersistingArrayRowsOrColumns = isHorizontal ? persistingArray.Rows : persistingArray.Columns;
            PersistingArrayXOrYPosition = isHorizontal ? persistingArray.YPosition : persistingArray.XPosition;
        }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>True if the arrays snap together along a horizontal edge, false if along a vertical one</summary>
        public bool IsHorizontal
        {
            get { return GetValue<bool>(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }

        public static readonly PropertyData IsHorizontalProperty = RegisterProperty("IsHorizontal", typeof (bool));

        /// <summary>UniqueID of the array that is snapped onto and then deleted.</summary>
        public string SnappedArrayID
        {
            get { return GetValue<string>(SnappedArrayIDProperty); }
            set { SetValue(SnappedArrayIDProperty, value); }
        }

        public static readonly PropertyData SnappedArrayIDProperty = RegisterProperty("SnappedArrayID", typeof (string), string.Empty);

        /// <summary>Original GridSquareSize of the array that is snapped onto and then deleted.</summary>
        public double SnappedArraySquareSize
        {
            get { return GetValue<double>(SnappedArraySquareSizeProperty); }
            set { SetValue(SnappedArraySquareSizeProperty, value); }
        }

        public static readonly PropertyData SnappedArraySquareSizeProperty = RegisterProperty("SnappedArraySquareSize", typeof (double));

        /// <summary>UniqueID of the array that snaps on and continues to exist.</summary>
        public string PersistingArrayID
        {
            get { return GetValue<string>(PersistingArrayIDProperty); }
            set { SetValue(PersistingArrayIDProperty, value); }
        }

        public static readonly PropertyData PersistingArrayIDProperty = RegisterProperty("PersistingArrayID", typeof (string), string.Empty);

        /// <summary>Horizontal divisions that the persisting array should be set to have when this history event fires (undoes or redoes, whichever comes next).</summary>
        public List<CLPArrayDivision> PersistingArrayHorizontalDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(PersistingArrayHorizontalDivisionsProperty); }
            set { SetValue(PersistingArrayHorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayHorizontalDivisionsProperty = RegisterProperty("PersistingArrayHorizontalDivisions",
                                                                                                          typeof (List<CLPArrayDivision>),
                                                                                                          () => new List<CLPArrayDivision>());

        /// <summary>Vertical divisions that the persisting array should be set to have when this history event fires (undoes or redoes, whichever comes next).</summary>
        public List<CLPArrayDivision> PersistingArrayVerticalDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(PersistingArrayVerticalDivisionsProperty); }
            set { SetValue(PersistingArrayVerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayVerticalDivisionsProperty = RegisterProperty("PersistingArrayVerticalDivisions",
                                                                                                        typeof (List<CLPArrayDivision>),
                                                                                                        () => new List<CLPArrayDivision>());

        /// <summary>Value of IsDivisionBehaviorOn prior to the history event (which sets it true)</summary>
        public bool PersistingArrayDivisionBehavior
        {
            get { return GetValue<bool>(PersistingArrayDivisionBehaviorProperty); }
            set { SetValue(PersistingArrayDivisionBehaviorProperty, value); }
        }

        public static readonly PropertyData PersistingArrayDivisionBehaviorProperty = RegisterProperty("PersistingArrayDivisionBehavior", typeof (bool));

        /// <summary>Rows or columns that the persisting array should be set to have when this history event fires (undoes or redoes, whichever comes next).</summary>
        public int PersistingArrayRowsOrColumns
        {
            get { return GetValue<int>(PersistingArrayRowsOrColumnsProperty); }
            set { SetValue(PersistingArrayRowsOrColumnsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayRowsOrColumnsProperty = RegisterProperty("PersistingArrayRowsOrColumns", typeof (int));

        /// <summary>Rows or columns that the persisting array should be set to have when this history event fires (undoes or redoes, whichever comes next).</summary>
        public double PersistingArrayXOrYPosition
        {
            get { return GetValue<double>(PersistingArrayXOrYPositionProperty); }
            set { SetValue(PersistingArrayXOrYPositionProperty, value); }
        }

        public static readonly PropertyData PersistingArrayXOrYPositionProperty = RegisterProperty("PersistingArrayXOrYPosition", typeof (double));

        public override string FormattedValue
        {
            get
            {
                var persistingArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(PersistingArrayID) as CLPArray;
                if (persistingArray == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Persisting Array not found on page or in history.", HistoryIndex);
                }

                var snappedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(SnappedArrayID) as CLPArray;
                if (snappedArray == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Snapped Array not found on page or in history.", HistoryIndex);
                }

                var direction = IsHorizontal ? "horizontally" : "vertically";
                var persistingArrayRows = IsHorizontal ? persistingArray.Rows - snappedArray.Rows : persistingArray.Rows;
                var persistingArrayColumns = IsHorizontal ? snappedArray.Columns : persistingArray.Columns - snappedArray.Columns;

                string persisitingArrayType;
                switch (persistingArray.ArrayType)
                {
                    case ArrayTypes.Array:
                        persisitingArrayType = "Array";
                        break;
                    case ArrayTypes.ArrayCard:
                        persisitingArrayType = "Array Card";
                        break;
                    case ArrayTypes.FactorCard:
                        persisitingArrayType = "Factor Card";
                        break;
                    case ArrayTypes.TenByTen:
                        persisitingArrayType = "Array (Static)";
                        break;
                    case ArrayTypes.ObscurableArray:
                        persisitingArrayType = "Obscurable Array";
                        break;
                    default:
                        persisitingArrayType = "Default Array";
                        break;
                }

                var presnapPersisitingArrayFormatedName = string.Format("{0}x{1} {2}", persistingArrayRows, persistingArrayColumns, persisitingArrayType);

                return string.Format("Index #{0}, Snapped {1} {2} onto {3} to create {4}.",
                                     HistoryIndex,
                                     snappedArray.FormattedName,
                                     direction,
                                     presnapPersisitingArrayFormatedName,
                                     persistingArray.FormattedName);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            PersistingArrayHorizontalDivisions = PersistingArrayHorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            PersistingArrayVerticalDivisions = PersistingArrayVerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            UndoAction(false);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if (persistingArray == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Persisting Array not found on page or in history.", HistoryIndex);
                return;
            }

            var snappedArray = ParentPage.GetVerifiedPageObjectInTrashByID(SnappedArrayID) as CLPArray;
            if (snappedArray == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Snapped Array not found on page or in history.", HistoryIndex);
                return;
            }

            snappedArray.SizeArrayToGridLevel(SnappedArraySquareSize);
            snappedArray.ParentPage = ParentPage;
            ParentPage.PageObjects.Add(snappedArray);
            ParentPage.History.TrashedPageObjects.Remove(snappedArray);

            var persistingArrayGridSquareSize = persistingArray.GridSquareSize;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = PersistingArrayDivisionBehavior;
            persistingArray.SizeArrayToGridLevel(persistingArrayGridSquareSize, false);

            var oldPageObjects = new List<IPageObject>
                                 {
                                     persistingArray
                                 };
            var newPageObjects = new List<IPageObject>
                                 {
                                     persistingArray,
                                     snappedArray
                                 };

            AStrokeAccepter.SplitAcceptedStrokes(oldPageObjects, newPageObjects);
            APageObjectAccepter.SplitAcceptedPageObjects(oldPageObjects, newPageObjects);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if (persistingArray == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Persisting Array not found on page or in history.", HistoryIndex);
                return;
            }

            var snappedArray = ParentPage.GetVerifiedPageObjectOnPageByID(SnappedArrayID) as CLPArray;
            if (snappedArray == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Snapped Array not found on page or in history.", HistoryIndex);
                return;
            }

            ParentPage.History.TrashedPageObjects.Add(snappedArray);
            ParentPage.PageObjects.Remove(snappedArray);

            var persistingArrayGridSquareSize = persistingArray.GridSquareSize;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = true;
            persistingArray.SizeArrayToGridLevel(persistingArrayGridSquareSize, false);

            var oldPageObjects = new List<IPageObject>
                                 {
                                     snappedArray
                                 };
            var newPageObjects = new List<IPageObject>
                                 {
                                     persistingArray
                                 };

            AStrokeAccepter.SplitAcceptedStrokes(oldPageObjects, newPageObjects);
            APageObjectAccepter.SplitAcceptedPageObjects(oldPageObjects, newPageObjects);
        }

        private void RestoreDivisions(CLPArray persistingArray)
        {
            var tempHorizontalDivisions = persistingArray.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            persistingArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(PersistingArrayHorizontalDivisions);
            PersistingArrayHorizontalDivisions = tempHorizontalDivisions;
            persistingArray.IsSideLabelVisible = persistingArray.HorizontalDivisions.All(d => !d.IsObscured);
            // BUG: doens't take into account if Both labels were hidden at the same time.

            var tempVerticalDivisions = persistingArray.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            persistingArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(PersistingArrayVerticalDivisions);
            PersistingArrayVerticalDivisions = tempVerticalDivisions;
            persistingArray.IsTopLabelVisible = persistingArray.VerticalDivisions.All(d => !d.IsObscured);
        }

        private void RestoreDimensionsAndPosition(CLPArray persistingArray)
        {
            if (IsHorizontal)
            {
                var tempRows = persistingArray.Rows;
                persistingArray.Rows = PersistingArrayRowsOrColumns;
                PersistingArrayRowsOrColumns = tempRows;

                var tempPosition = persistingArray.YPosition;
                persistingArray.YPosition = PersistingArrayXOrYPosition;
                PersistingArrayXOrYPosition = tempPosition;
            }
            else
            {
                var tempColumns = persistingArray.Columns;
                persistingArray.Columns = PersistingArrayRowsOrColumns;
                PersistingArrayRowsOrColumns = tempColumns;

                var tempPosition = persistingArray.XPosition;
                persistingArray.XPosition = PersistingArrayXOrYPosition;
                PersistingArrayXOrYPosition = tempPosition;
            }
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = this.DeepCopy();
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if (clonedHistoryItem == null ||
                persistingArray == null)
            {
                return null;
            }

            clonedHistoryItem.PersistingArrayHorizontalDivisions = persistingArray.HorizontalDivisions.ToList();
            clonedHistoryItem.PersistingArrayVerticalDivisions = persistingArray.VerticalDivisions.ToList();

            if (IsHorizontal)
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Rows;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.YPosition;
            }
            else
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Columns;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.XPosition;
            }

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return SnappedArrayID == id || PersistingArrayID == id; }

        #endregion //Methods
    }
}