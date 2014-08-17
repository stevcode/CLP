using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateTroubleWithRemaindersTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" /> from scratch.</summary>
        public DivisionTemplateTroubleWithRemaindersTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateTroubleWithRemaindersTag" /> belongs to.</param>
        public DivisionTemplateTroubleWithRemaindersTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin)
        {
            DivisionTemplateID = divisionTemplateID;
            Dividend = dividend;
            Divisor = divisor;
        }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithRemaindersTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateTroubleWithRemaindersTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>ID of the Division Template against which this tag compares.</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof (string), string.Empty);

        /// <summary>Dividend of the DivisionTemplate being compared against.</summary>
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (double));

        /// <summary>Divisor of the DivisionTemplate being compared against.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double));

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

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

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
                                  "Trouble with {0} / {1}.\n" + "Created {2} Arrays too large.\n" + "Failed to snap {3} Arrays.\n" +
                                  "Changed Array orientation {4} time(s).",
                                  Dividend,
                                  Divisor,
                                  ArrayTooLargeAttempts,
                                  FailedSnapAttempts,
                                  OrientationChangedAttempts);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}