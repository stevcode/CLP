using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateTroubleWithRemaindersTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" /> from scratch.</summary>
        public DivisionTemplateTroubleWithRemaindersTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateTroubleWithRemaindersTag" /> belongs to.</param>
        public DivisionTemplateTroubleWithRemaindersTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateTroubleWithRemaindersTag(SerializationInfo info, StreamingContext context)
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

        /// <summary>Number of times an array failed to snap into the Division Template.</summary>
        public int FailedSnapAttempts
        {
            get { return GetValue<int>(FailedSnapAttemptsProperty); }
            set
            {
                SetValue(FailedSnapAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData FailedSnapAttemptsProperty = RegisterProperty("FailedSnapAttempts", typeof (int), 0);

        /// <summary>Number of times an array's orientation was changed while attempting to solve the remainder of a Division Template.</summary>
        public int OrientationChangedAttempts
        {
            get { return GetValue<int>(OrientationChangedAttemptsProperty); }
            set
            {
                SetValue(OrientationChangedAttemptsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData OrientationChangedAttemptsProperty = RegisterProperty("OrientationChangedAttempts", typeof (int), 0);

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Trouble With Remainders"; }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Trouble with {0} / {1}.\n" + "DivisionTemplate {2} on page.\n" + "Created {3} Arrays too large.\n" +
                                  "Failed to snap {4} Arrays.\n" + "Changed Array orientation {5} time(s).",
                                  Dividend,
                                  Divisor,
                                  IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                  ArrayTooLargeAttempts,
                                  FailedSnapAttempts,
                                  OrientationChangedAttempts);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}