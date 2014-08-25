using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class DivisionTemplateTroubleWithRemaindersTagViewModel : ViewModelBase
    {
        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public DivisionTemplateTroubleWithRemaindersTagViewModel(DivisionTemplateTroubleWithRemaindersTag troubleWithRemaindersTag)
        {
            TroubleWithRemaindersTag = troubleWithRemaindersTag;
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "DivisionTemplateTroubleWithRemaindersTagVM"; }
        }

        #region Model

        /// <summary>Trouble With Remainders Tag Model.</summary>
        [Model(SupportIEditableObject = true)]
        public DivisionTemplateTroubleWithRemaindersTag TroubleWithRemaindersTag
        {
            get { return GetValue<DivisionTemplateTroubleWithRemaindersTag>(TroubleWithRemaindersTagProperty); }
            set { SetValue(TroubleWithRemaindersTagProperty, value); }
        }

        public static readonly PropertyData TroubleWithRemaindersTagProperty = RegisterProperty("TroubleWithRemaindersTag",
                                                                                                typeof (DivisionTemplateTroubleWithRemaindersTag));

        /// <summary>Dividend of the DivisionTemplate being compared against.</summary>
        [ViewModelToModel("TroubleWithRemaindersTag")]
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (double));

        /// <summary>Divisor of the DivisionTemplate being compared against.</summary>
        [ViewModelToModel("TroubleWithRemaindersTag")]
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double));

        /// <summary>Number of times an array that was too large to fit the Division Template was created.</summary>
        [ViewModelToModel("TroubleWithRemaindersTag")]
        public int ArrayTooLargeAttempts
        {
            get { return GetValue<int>(ArrayTooLargeAttemptsProperty); }
            set { SetValue(ArrayTooLargeAttemptsProperty, value); }
        }

        public static readonly PropertyData ArrayTooLargeAttemptsProperty = RegisterProperty("ArrayTooLargeAttempts", typeof (int), 0);

        /// <summary>Number of times an array failed to snap into the Division Template.</summary>
        [ViewModelToModel("TroubleWithRemaindersTag")]
        public int FailedSnapAttempts
        {
            get { return GetValue<int>(FailedSnapAttemptsProperty); }
            set { SetValue(FailedSnapAttemptsProperty, value); }
        }

        public static readonly PropertyData FailedSnapAttemptsProperty = RegisterProperty("FailedSnapAttempts", typeof (int), 0);

        /// <summary>Number of times an array's orientation was changed while attempting to solve the remainder of a Division Template.</summary>
        [ViewModelToModel("TroubleWithRemaindersTag")]
        public int OrientationChangedAttempts
        {
            get { return GetValue<int>(OrientationChangedAttemptsProperty); }
            set { SetValue(OrientationChangedAttemptsProperty, value); }
        }

        public static readonly PropertyData OrientationChangedAttemptsProperty = RegisterProperty("OrientationChangedAttempts", typeof (int), 0);

        #endregion //Model
    }
}