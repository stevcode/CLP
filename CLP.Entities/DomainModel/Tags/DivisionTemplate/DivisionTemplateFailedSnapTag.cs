using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateFailedSnapTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateFailedSnapTag" /> from scratch.</summary>
        public DivisionTemplateFailedSnapTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateFailedSnapTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateFailedSnapTag" /> belongs to.</param>
        public DivisionTemplateFailedSnapTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateFailedSnapTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateFailedSnapTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        public int ArrayTooLargeAttempts
        {
            get { return GetValue<int>(ArrayTooLargeAttemptsProperty); }
            set
            {
                SetValue(ArrayTooLargeAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData ArrayTooLargeAttemptsProperty = RegisterProperty("ArrayTooLargeAttempts", typeof (int), 0);

        /// <summary>Number of times an array that had incorrect dimensions for the Division Template was created.</summary>
        public int IncorrectDimensionAttempts
        {
            get { return GetValue<int>(IncorrectDimensionAttemptsProperty); }
            set
            {
                SetValue(IncorrectDimensionAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData IncorrectDimensionAttemptsProperty = RegisterProperty("IncorrectDimensionAttempts", typeof (int), 0);

        /// <summary>Number of times an array that was oriented the wrong way to fit the Division Template was created.</summary>
        public int WrongOrientationAttempts
        {
            get { return GetValue<int>(WrongOrientationAttemptsProperty); }
            set
            {
                SetValue(WrongOrientationAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData WrongOrientationAttemptsProperty = RegisterProperty("WrongOrientationAttempts", typeof (int), 0);

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Failed Array Snaps"; }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Failed to Snap Arrays into {0} / {1}.\n" + "DivisionTemplate {2} on page.\n" + "Snapped {3} too large.\n" +
                                  "Snapped {4} with incorrect dimensions.\n" + "Snapped {5} with the wrong orientation.",
                                  Dividend,
                                  Divisor,
                                  IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                  ArrayTooLargeAttempts,
                                  IncorrectDimensionAttempts,
                                  WrongOrientationAttempts);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}