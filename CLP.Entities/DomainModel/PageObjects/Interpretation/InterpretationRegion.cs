using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    public enum RegionBorderStyles
    {
        SolidLine,
        DashedLine,
        Hidden,
        Underline
    }

    public enum Interpreters
    {
        Handwriting
    }

    [Serializable]
    public class InterpretationRegion : AStrokeAccepter
    {
        #region Constructors

        /// <summary>Initializes <see cref="InterpretationRegion" /> from scratch.</summary>
        public InterpretationRegion() { }

        /// <summary>Initializes <see cref="InterpretationRegion" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="InterpretationRegion" /> belongs to.</param>
        public InterpretationRegion(CLPPage parentPage)
            : base(parentPage) { }

        #endregion // Constructors

        #region Properties

        /// <summary>The visual border style around the <see cref="InterpretationRegion" />.</summary>
        public RegionBorderStyles RegionBorderStyle
        {
            get { return GetValue<RegionBorderStyles>(RegionBorderStyleProperty); }
            set { SetValue(RegionBorderStyleProperty, value); }
        }

        public static readonly PropertyData RegionBorderStyleProperty = RegisterProperty("RegionBorderStyle", typeof(RegionBorderStyles), RegionBorderStyles.Hidden);

        /// <summary>List of interpreters to run on the region.</summary>
        public ObservableCollection<Interpreters> Interpreters
        {
            get { return GetValue<ObservableCollection<Interpreters>>(InterpretersProperty); }
            set { SetValue(InterpretersProperty, value); }
        }

        public static readonly PropertyData InterpretersProperty = RegisterProperty("Interpreters", typeof(ObservableCollection<Interpreters>), () => new ObservableCollection<Interpreters>());

        #endregion // Properties

        #region APageObjectBase Overrides

        public override string FormattedName => "Interpretation Region";

        public override string CodedName => Codings.OBJECT_FILL_IN;

        public override string CodedID => string.Empty; // TODO: Make this work with IncrementID

        public override int ZIndex => 30;

        public override bool IsBackgroundInteractable => false;

        #endregion // APageObjectBase Overrides
    }
}