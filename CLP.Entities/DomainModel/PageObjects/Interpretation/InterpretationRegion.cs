﻿using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;
using CLP.InkInterpretation;

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
    public class InterpretationRegion : APageObjectBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="InterpretationRegion" /> from scratch.</summary>
        public InterpretationRegion() { }

        /// <summary>Initializes <see cref="InterpretationRegion" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="InterpretationRegion" /> belongs to.</param>
        public InterpretationRegion(CLPPage parentPage)
            : base(parentPage) { }

        /// <summary>Initializes <see cref="InterpretationRegion" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public InterpretationRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        /// <summary>The visual border style around the <see cref="InterpretationRegion" />.</summary>
        public RegionBorderStyles RegionBorderStyle
        {
            get { return GetValue<RegionBorderStyles>(RegionBorderStyleProperty); }
            set { SetValue(RegionBorderStyleProperty, value); }
        }

        public static readonly PropertyData RegionBorderStyleProperty = RegisterProperty("RegionBorderStyle", typeof (RegionBorderStyles), RegionBorderStyles.Hidden);

        /// <summary>List of interpreters to run on the region.</summary>
        public ObservableCollection<Interpreters> Interpreters
        {
            get { return GetValue<ObservableCollection<Interpreters>>(InterpretersProperty); }
            set { SetValue(InterpretersProperty, value); }
        }

        public static readonly PropertyData InterpretersProperty = RegisterProperty("Interpreters", typeof (ObservableCollection<Interpreters>), () => new ObservableCollection<Interpreters>());
        
        

        #endregion // Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Interpretation Region"; }
        }

        public override string CodedName
        {
            get { return Codings.OBJECT_FILL_IN; }
        }

        public override string CodedID
        {
            get { return string.Empty; } // TODO: Make this work with IncrementID
        }

        public override int ZIndex
        {
            get { return 30; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public override IPageObject Duplicate()
        {
            var newRegion = Clone() as InterpretationRegion;
            if (newRegion == null)
            {
                return null;
            }
            newRegion.CreationDate = DateTime.Now;
            newRegion.ID = Guid.NewGuid().ToCompactID();
            newRegion.VersionIndex = 0;
            newRegion.LastVersionIndex = null;
            newRegion.ParentPage = ParentPage;

            return newRegion;
        }

        #endregion // APageObjectBase Overrides

        #region Static Methods

        public static string InterpretHandwriting(InterpretationRegion region, StrokeCollection strokes) { return InkInterpreter.StrokesToBestGuessText(strokes); } 

        #endregion // Static Methods
    }
}