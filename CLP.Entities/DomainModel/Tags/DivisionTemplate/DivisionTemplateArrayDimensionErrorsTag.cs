using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateArrayDimensionErrorsTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateArrayDimensionErrorsTag" /> from scratch.</summary>
        public DivisionTemplateArrayDimensionErrorsTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateArrayDimensionErrorsTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateArrayDimensionErrorsTag" /> belongs to.</param>
        public DivisionTemplateArrayDimensionErrorsTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateArrayDimensionErrorsTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateArrayDimensionErrorsTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        public int CreateArrayTooLargeAttempts
        {
            get { return GetValue<int>(CreateArrayTooLargeAttemptsProperty); }
            set
            {
                SetValue(CreateArrayTooLargeAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData CreateArrayTooLargeAttemptsProperty = RegisterProperty("CreateArrayTooLargeAttempts", typeof (int), 0);

        /// <summary>Number of times an array that had incorrect dimensions for the Division Template was created.</summary>
        public int CreateIncorrectDimensionAttempts
        {
            get { return GetValue<int>(CreateIncorrectDimensionAttemptsProperty); }
            set
            {
                SetValue(CreateIncorrectDimensionAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData CreateIncorrectDimensionAttemptsProperty = RegisterProperty("CreateIncorrectDimensionAttempts",
                                                                                                        typeof (int),
                                                                                                        0);

        /// <summary>Number of times an array that was oriented the wrong way to fit the Division Template was created.</summary>
        public int CreateWrongOrientationAttempts
        {
            get { return GetValue<int>(CreateWrongOrientationAttemptsProperty); }
            set
            {
                SetValue(CreateWrongOrientationAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData CreateWrongOrientationAttemptsProperty = RegisterProperty("CreateWrongOrientationAttempts",
                                                                                                      typeof (int),
                                                                                                      0);

        /// <summary>Number of times an array that had one dimension as the Dividend of the Division Template was created.</summary>
        public int CreateDividendAsDimensionAttempts
        {
            get { return GetValue<int>(CreateDividendAsDimensionAttemptsProperty); }
            set
            {
                SetValue(CreateDividendAsDimensionAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData CreateDividendAsDimensionAttemptsProperty = RegisterProperty("CreateDividendAsDimensionAttempts",
                                                                                                         typeof (int),
                                                                                                         0);

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        public int SnapArrayTooLargeAttempts
        {
            get { return GetValue<int>(SnapArrayTooLargeAttemptsProperty); }
            set
            {
                SetValue(SnapArrayTooLargeAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData SnapArrayTooLargeAttemptsProperty = RegisterProperty("SnapArrayTooLargeAttempts", typeof (int), 0);

        /// <summary>Number of times an array that had incorrect dimensions for the Division Template was created.</summary>
        public int SnapIncorrectDimensionAttempts
        {
            get { return GetValue<int>(SnapIncorrectDimensionAttemptsProperty); }
            set
            {
                SetValue(SnapIncorrectDimensionAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData SnapIncorrectDimensionAttemptsProperty = RegisterProperty("SnapIncorrectDimensionAttempts",
                                                                                                      typeof (int),
                                                                                                      0);

        /// <summary>Number of times an array that was oriented the wrong way to fit the Division Template was created.</summary>
        public int SnapWrongOrientationAttempts
        {
            get { return GetValue<int>(SnapWrongOrientationAttemptsProperty); }
            set
            {
                SetValue(SnapWrongOrientationAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData SnapWrongOrientationAttemptsProperty = RegisterProperty("SnapWrongOrientationAttempts", typeof (int), 0);

        /// <summary>Number of times an array's orientation was changed while attempting to solve the remainder of a Division Template.</summary>
        public int OrientationChangedAttempts
        {
            get { return GetValue<int>(OrientationChangedAttemptsProperty); }
            set
            {
                SetValue(OrientationChangedAttemptsProperty, value);
                RaisePropertyChanged("FormattedName");
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData OrientationChangedAttemptsProperty = RegisterProperty("OrientationChangedAttempts", typeof (int), 0);

        #region Calculated Properties

        private const int TROUBLE_TOLERANCE = 4;

        public bool HadTrouble
        {
            get { return ErrorAtemptsSum > TROUBLE_TOLERANCE; }
        }

        public int ErrorAtemptsSum
        {
            get
            {
                return CreateArrayTooLargeAttempts + CreateIncorrectDimensionAttempts + CreateWrongOrientationAttempts +
                       CreateDividendAsDimensionAttempts + SnapArrayTooLargeAttempts + SnapIncorrectDimensionAttempts + SnapWrongOrientationAttempts +
                       OrientationChangedAttempts;
            }
        }

        #endregion //Calculated Properties

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Array Dimension Errors{0}", HadTrouble ? " **Trouble**" : string.Empty); }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Errors on {0} / {1}.\n" + "DivisionTemplate {2} on page.\n" + "Created {3} Arrays too large.\n" +
                                  "Created {4} Arrays with incorrect dimensions.\n" + "Created {5} Arrays with the wrong orientation.\n" +
                                  "Created {6} Arrays with Dividend as a dimension.\n" + "Snapped {7} too large.\n" +
                                  "Snapped {8} with incorrect dimensions.\n" + "Snapped {9} with the wrong orientation." +
                                  "Changed Array orientation {10} time(s).",
                                  Dividend,
                                  Divisor,
                                  IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                  CreateArrayTooLargeAttempts,
                                  CreateIncorrectDimensionAttempts,
                                  CreateWrongOrientationAttempts,
                                  CreateDividendAsDimensionAttempts,
                                  SnapArrayTooLargeAttempts,
                                  SnapIncorrectDimensionAttempts,
                                  SnapWrongOrientationAttempts,
                                  OrientationChangedAttempts);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}