using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayDivisionsChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionsChangedHistoryItem" /> from scratch.</summary>
        public CLPArrayDivisionsChangedHistoryItem() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionsChangedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayDivisionsChangedHistoryItem(CLPPage parentPage, Person owner, string arrayID, List<CLPArrayDivision> oldRegions, List<CLPArrayDivision> newRegions)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            OldRegions = oldRegions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            NewRegions = newRegions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
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

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryItem" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof (string));

        /// <summary>ArrayDivisions added to Array</summary>
        [Obsolete("Too error prone. Now keeps track of all old and new Column or Row Regions.")]
        public List<CLPArrayDivision> AddedDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(AddedDivisionsProperty); }
            set { SetValue(AddedDivisionsProperty, value); }
        }

        public static readonly PropertyData AddedDivisionsProperty = RegisterProperty("AddedDivisions", typeof (List<CLPArrayDivision>));

        /// <summary>ArrayDivisions removed from Array</summary>
        [Obsolete("Too error prone. Now keeps track of all old and new Column or Row Regions.")]
        public List<CLPArrayDivision> RemovedDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(RemovedDivisionsProperty); }
            set { SetValue(RemovedDivisionsProperty, value); }
        }

        public static readonly PropertyData RemovedDivisionsProperty = RegisterProperty("RemovedDivisions", typeof (List<CLPArrayDivision>));

        /// <summary>Old regions, either RowRegions or ColumnRegions; HorizontalDivisions and VerticalDivisions respectively.</summary>
        public List<CLPArrayDivision> OldRegions
        {
            get { return GetValue<List<CLPArrayDivision>>(OldRegionsProperty); }
            set { SetValue(OldRegionsProperty, value); }
        }

        public static readonly PropertyData OldRegionsProperty = RegisterProperty("OldRegions", typeof (List<CLPArrayDivision>), () => new List<CLPArrayDivision>());

        /// <summary>New regions, either RowRegions or ColumnRegions; HorizontalDivisions and VerticalDivisions respectively.</summary>
        public List<CLPArrayDivision> NewRegions
        {
            get { return GetValue<List<CLPArrayDivision>>(NewRegionsProperty); }
            set { SetValue(NewRegionsProperty, value); }
        }

        public static readonly PropertyData NewRegionsProperty = RegisterProperty("NewRegions", typeof (List<CLPArrayDivision>), () => new List<CLPArrayDivision>());

        public bool? IsColumnRegionsChange
        {
            get
            {
                if (OldRegions.Any() ||
                    NewRegions.Any())
                {
                    return (OldRegions.Any() && OldRegions.First().Orientation == ArrayDivisionOrientation.Vertical) ||
                           (NewRegions.Any() && NewRegions.First().Orientation == ArrayDivisionOrientation.Vertical);
                }

                Console.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryIndex);
                return null;
            }
        }

        public override string FormattedValue
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                if (array == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                }

                if (IsColumnRegionsChange == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryIndex);
                }

                var orientation = (bool)IsColumnRegionsChange ? "vertical" : "horizontal";
                var oldRegion = OldRegions.Any() ? string.Join(",", OldRegions.Select(d => d.Value)) : "none";
                var newRegion = NewRegions.Any() ? string.Join(",", NewRegions.Select(d => d.Value)) : "none";

                return string.Format("Index #{0}, Changed {1} divisions on {2} from {3} to {4}.", HistoryIndex, orientation, array.FormattedName, oldRegion, newRegion);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                return;
            }

            if ((AddedDivisions.Any() && AddedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal) ||
                (RemovedDivisions.Any() && RemovedDivisions[0].Orientation == ArrayDivisionOrientation.Horizontal))
            {
                NewRegions = array.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();

                foreach (var clpArrayDivision in AddedDivisions)
                {
                    array.HorizontalDivisions.Remove(clpArrayDivision);
                }
                foreach (var clpArrayDivision in RemovedDivisions)
                {
                    array.HorizontalDivisions.Add(clpArrayDivision);
                }

                OldRegions = array.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            }
            else
            {
                NewRegions = array.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();

                foreach (var clpArrayDivision in AddedDivisions)
                {
                    array.VerticalDivisions.Remove(clpArrayDivision);
                }
                foreach (var clpArrayDivision in RemovedDivisions)
                {
                    array.VerticalDivisions.Add(clpArrayDivision);
                }

                OldRegions = array.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            }

            AddedDivisions.Clear();
            RemovedDivisions.Clear();
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                return;
            }

            if (IsColumnRegionsChange == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryIndex);
                return;
            }

            if ((bool)IsColumnRegionsChange)
            {
                array.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(OldRegions);
            }
            else
            {
                array.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(OldRegions);
            }
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryIndex);
                return;
            }

            if (IsColumnRegionsChange == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryIndex);
                return;
            }

            if ((bool)IsColumnRegionsChange)
            {
                array.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(NewRegions);
            }
            else
            {
                array.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(NewRegions);
            }
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayDivisionsChangedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return ArrayID == id; }

        #endregion //Methods
    }
}