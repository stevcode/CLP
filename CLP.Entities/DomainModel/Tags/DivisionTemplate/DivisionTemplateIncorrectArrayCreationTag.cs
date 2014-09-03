using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateIncorrectArrayCreationTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateIncorrectArrayCreationTag" /> from scratch.</summary>
        public DivisionTemplateIncorrectArrayCreationTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateIncorrectArrayCreationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateIncorrectArrayCreationTag" /> belongs to.</param>
        public DivisionTemplateIncorrectArrayCreationTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateIncorrectArrayCreationTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateIncorrectArrayCreationTag(SerializationInfo info, StreamingContext context)
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

        /// <summary>Number of times an array that had one dimension as the Dividend of the Division Template was created.</summary>
        public int DividendAsDimensionAttempts
        {
            get { return GetValue<int>(DividendAsDimensionAttemptsProperty); }
            set
            {
                SetValue(DividendAsDimensionAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData DividendAsDimensionAttemptsProperty = RegisterProperty("DividendAsDimensionAttempts", typeof (int), 0);

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Incorrect Arrays Created"; }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Incorrect Arrays created for {0} / {1}.\n" + "DivisionTemplate {2} on page.\n" + "Created {3} too large.\n" +
                                  "Created {4} with incorrect dimensions.\n" + "Created {5} with the wrong orientation.\n" +
                                  "Created {6} with Dividend as a dimension.",
                                  Dividend,
                                  Divisor,
                                  IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                  ArrayTooLargeAttempts,
                                  IncorrectDimensionAttempts,
                                  WrongOrientationAttempts,
                                  DividendAsDimensionAttempts);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}