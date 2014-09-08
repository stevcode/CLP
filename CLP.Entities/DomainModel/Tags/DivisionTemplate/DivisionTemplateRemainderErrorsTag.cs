using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateRemainderErrorsTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateRemainderErrorsTag" /> from scratch.</summary>
        public DivisionTemplateRemainderErrorsTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateRemainderErrorsTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateRemainderErrorsTag" /> belongs to.</param>
        public DivisionTemplateRemainderErrorsTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateRemainderErrorsTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateRemainderErrorsTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Dimensions of arrays that were too large to fit the Division Template.</summary>
        public List<string> CreateArrayTooLargeDimensions
        {
            get { return GetValue<List<string>>(CreateArrayTooLargeDimensionsProperty); }
            set { SetValue(CreateArrayTooLargeDimensionsProperty, value); }
        }

        public static readonly PropertyData CreateArrayTooLargeDimensionsProperty = RegisterProperty("CreateArrayTooLargeDimensions",
                                                                                                     typeof(List<string>),
                                                                                                     () => new List<string>());

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        public int CreateArrayTooLargeAttempts
        {
            get { return CreateArrayTooLargeDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that were created with incorrect dimensions for the Division Tempalte.</summary>
        public List<string> CreateIncorrectDimensionDimensions
        {
            get { return GetValue<List<string>>(CreateIncorrectDimensionDimensionsProperty); }
            set { SetValue(CreateIncorrectDimensionDimensionsProperty, value); }
        }

        public static readonly PropertyData CreateIncorrectDimensionDimensionsProperty = RegisterProperty("CreateIncorrectDimensionDimensions",
                                                                                                          typeof(List<string>),
                                                                                                          () => new List<string>());

        /// <summary>Number of times an array that had incorrect dimensions for the Division Template was created.</summary>
        public int CreateIncorrectDimensionAttempts
        {
            get { return CreateIncorrectDimensionDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that were created with the wrong orienation for the Division Template.</summary>
        public List<string> CreateWrongOrientationDimensions
        {
            get { return GetValue<List<string>>(CreateWrongOrientationDimensionsProperty); }
            set { SetValue(CreateWrongOrientationDimensionsProperty, value); }
        }

        public static readonly PropertyData CreateWrongOrientationDimensionsProperty = RegisterProperty("CreateWrongOrientationDimensions",
                                                                                                        typeof(List<string>),
                                                                                                        () => new List<string>());

        /// <summary>Number of times an array that was oriented the wrong way to fit the Division Template was created.</summary>
        public int CreateWrongOrientationAttempts
        {
            get { return CreateWrongOrientationDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that were created with the Dividend of the Division Template as one of its dimensions.</summary>
        public List<string> CreateDividendAsDimensionDimensions
        {
            get { return GetValue<List<string>>(CreateDividendAsDimensionDimensionsProperty); }
            set { SetValue(CreateDividendAsDimensionDimensionsProperty, value); }
        }

        public static readonly PropertyData CreateDividendAsDimensionDimensionsProperty = RegisterProperty("CreateDividendAsDimensionDimensions",
                                                                                                           typeof(List<string>),
                                                                                                           () => new List<string>());

        /// <summary>Number of times an array that had one dimension as the Dividend of the Division Template was created.</summary>
        public int CreateDividendAsDimensionAttempts
        {
            get { return CreateDividendAsDimensionDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that failed to snap into the Division Template because they were too large.</summary>
        public List<string> SnapArrayTooLargeDimensions
        {
            get { return GetValue<List<string>>(SnapArrayTooLargeDimensionsProperty); }
            set { SetValue(SnapArrayTooLargeDimensionsProperty, value); }
        }

        public static readonly PropertyData SnapArrayTooLargeDimensionsProperty = RegisterProperty("SnapArrayTooLargeDimensions",
                                                                                                   typeof(List<string>),
                                                                                                   () => new List<string>());

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        public int SnapArrayTooLargeAttempts
        {
            get { return SnapArrayTooLargeDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that failed to snap into the Division Template with incorrect dimensions.</summary>
        public List<string> SnapIncorrectDimensionDimensions
        {
            get { return GetValue<List<string>>(SnapIncorrectDimensionDimensionsProperty); }
            set { SetValue(SnapIncorrectDimensionDimensionsProperty, value); }
        }

        public static readonly PropertyData SnapIncorrectDimensionDimensionsProperty = RegisterProperty("SnapIncorrectDimensionDimensions",
                                                                                                        typeof(List<string>),
                                                                                                        () => new List<string>());

        /// <summary>Number of times an array that had incorrect dimensions for the Division Template was created.</summary>
        public int SnapIncorrectDimensionAttempts
        {
            get { return SnapIncorrectDimensionDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that failed to snap into the Division Template with wrong orientations.</summary>
        public List<string> SnapWrongOrientationDimensions
        {
            get { return GetValue<List<string>>(SnapWrongOrientationDimensionsProperty); }
            set { SetValue(SnapWrongOrientationDimensionsProperty, value); }
        }

        public static readonly PropertyData SnapWrongOrientationDimensionsProperty = RegisterProperty("SnapWrongOrientationDimensions",
                                                                                                      typeof(List<string>),
                                                                                                      () => new List<string>());

        /// <summary>Number of times an array that was oriented the wrong way to fit the Division Template was created.</summary>
        public int SnapWrongOrientationAttempts
        {
            get { return SnapWrongOrientationDimensions.Count; }
        }

        /// <summary>Dimensions of arrays that change their orientation.</summary>
        public List<string> OrientationChangedDimensions
        {
            get { return GetValue<List<string>>(OrientationChangedDimensionsProperty); }
            set { SetValue(OrientationChangedDimensionsProperty, value); }
        }

        public static readonly PropertyData OrientationChangedDimensionsProperty = RegisterProperty("OrientationChangedDimensions",
                                                                                                    typeof(List<string>),
                                                                                                    () => new List<string>());

        /// <summary>Number of times an array's orientation was changed while attempting to solve the remainder of a Division Template.</summary>
        public int OrientationChangedAttempts
        {
            get { return OrientationChangedDimensions.Count; }
        }

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
            get { return string.Format("Remainder Errors{0}", HadTrouble ? " **Trouble**" : string.Empty); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Errors on {0} / {1} after Division Template Full." + "\nDivisionTemplate {2} on page.{3}{4}{5}{6}{7}{8}{9}{10}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     CreateArrayTooLargeAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nCreated {0} Array(s) too large." + "\nArray(s): {1}",
                                                         CreateArrayTooLargeAttempts,
                                                         string.Join(", ", CreateArrayTooLargeDimensions)),
                                     CreateIncorrectDimensionAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nCreated {0} Array(s) with incorrect dimensions." + "\nArray(s): {1}",
                                                         CreateIncorrectDimensionAttempts,
                                                         string.Join(", ", CreateIncorrectDimensionDimensions)),
                                     CreateWrongOrientationAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nCreated {0} Array(s) with the wrong orientation." + "\nArray(s): {1}",
                                                         CreateWrongOrientationAttempts,
                                                         string.Join(", ", CreateWrongOrientationDimensions)),
                                     CreateDividendAsDimensionAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nCreated {0} Array(s) with Dividend as a dimension." + "\nArray(s): {1}",
                                                         CreateDividendAsDimensionAttempts,
                                                         string.Join(", ", CreateDividendAsDimensionDimensions)),
                                     SnapArrayTooLargeAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nSnapped {0} too large." + "\nArrays: {1}",
                                                         SnapArrayTooLargeAttempts,
                                                         string.Join(", ", SnapArrayTooLargeDimensions)),
                                     SnapIncorrectDimensionAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nSnapped {0} with incorrect dimensions." + "\nArray(s): {1}",
                                                         SnapIncorrectDimensionAttempts,
                                                         string.Join(", ", SnapIncorrectDimensionDimensions)),
                                     SnapWrongOrientationAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nSnapped {0} with the wrong orientation." + "\nArray(s): {1}",
                                                         SnapWrongOrientationAttempts,
                                                         string.Join(", ", SnapWrongOrientationDimensions)),
                                     OrientationChangedAttempts == 0
                                         ? string.Empty
                                         : string.Format("\nPossible Error: Changed Array orientation {0} time(s)." + "\nArray(s): {1}",
                                                         OrientationChangedAttempts,
                                                         string.Join(", ", OrientationChangedDimensions)));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}