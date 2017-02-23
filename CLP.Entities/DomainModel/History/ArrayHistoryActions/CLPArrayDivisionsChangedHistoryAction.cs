using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayDivisionsChangedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionsChangedHistoryAction" /> from scratch.</summary>
        public CLPArrayDivisionsChangedHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionsChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public CLPArrayDivisionsChangedHistoryAction(CLPPage parentPage, Person owner, string arrayID, List<CLPArrayDivision> oldRegions, List<CLPArrayDivision> newRegions)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            OldRegions = oldRegions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            NewRegions = newRegions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryAction" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string), string.Empty);

        /// <summary>Old regions, either RowRegions or ColumnRegions; HorizontalDivisions and VerticalDivisions respectively.</summary>
        public List<CLPArrayDivision> OldRegions
        {
            get { return GetValue<List<CLPArrayDivision>>(OldRegionsProperty); }
            set { SetValue(OldRegionsProperty, value); }
        }

        public static readonly PropertyData OldRegionsProperty = RegisterProperty("OldRegions", typeof(List<CLPArrayDivision>), () => new List<CLPArrayDivision>());

        /// <summary>New regions, either RowRegions or ColumnRegions; HorizontalDivisions and VerticalDivisions respectively.</summary>
        public List<CLPArrayDivision> NewRegions
        {
            get { return GetValue<List<CLPArrayDivision>>(NewRegionsProperty); }
            set { SetValue(NewRegionsProperty, value); }
        }

        public static readonly PropertyData NewRegionsProperty = RegisterProperty("NewRegions", typeof(List<CLPArrayDivision>), () => new List<CLPArrayDivision>());

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

                Debug.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryActionIndex);
                return null;
            }
        }

        #endregion // Properties

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                if (array == null)
                {
                    return "[ERROR] Array for Divisions Changed not found on page or in history.";
                }

                if (IsColumnRegionsChange == null)
                {
                    return "[ERROR] Array Divisions Changed is missing Old and New Regions.";
                }

                var orientation = (bool)IsColumnRegionsChange ? "vertical" : "horizontal";
                var oldRegion = OldRegions.Any() ? string.Join(",", OldRegions.Select(d => d.Value)) : "none";
                var newRegion = NewRegions.Any() ? string.Join(",", NewRegions.Select(d => d.Value)) : "none";

                //TODO: Created FormattedValue for situation where IsObscuring is being toggled.
                return $"Changed {orientation} divisions on {array.FormattedName} from {oldRegion} to {newRegion}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            if (IsColumnRegionsChange == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryActionIndex);
                return;
            }

            if ((bool)IsColumnRegionsChange)
            {
                array.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(OldRegions);
                array.IsTopLabelVisible = array.VerticalDivisions.All(d => !d.IsObscured);
            }
            else
            {
                array.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(OldRegions);
                array.IsSideLabelVisible = array.HorizontalDivisions.All(d => !d.IsObscured);
            }
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array for Divisions Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            if (IsColumnRegionsChange == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array Divisions Changed is missing Old and New Regions.", HistoryActionIndex);
                return;
            }

            if ((bool)IsColumnRegionsChange)
            {
                array.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(NewRegions);
                array.IsTopLabelVisible = array.VerticalDivisions.All(d => !d.IsObscured);
            }
            else
            {
                array.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(NewRegions);
                array.IsSideLabelVisible = array.HorizontalDivisions.All(d => !d.IsObscured);
            }
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return ArrayID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}