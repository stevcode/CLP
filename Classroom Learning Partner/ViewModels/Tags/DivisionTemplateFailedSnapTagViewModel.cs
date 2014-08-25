using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class DivisionTemplateFailedSnapTagViewModel : ViewModelBase
    {
        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public DivisionTemplateFailedSnapTagViewModel(DivisionTemplateFailedSnapTag failedSnapTag) { FailedSnapTag = failedSnapTag; }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "DivisionTemplateFailedSnapTagVM"; }
        }

        #region Model

        /// <summary>Model.</summary>
        [Model(SupportIEditableObject = true)]
        public DivisionTemplateFailedSnapTag FailedSnapTag
        {
            get { return GetValue<DivisionTemplateFailedSnapTag>(FailedSnapTagProperty); }
            set { SetValue(FailedSnapTagProperty, value); }
        }

        public static readonly PropertyData FailedSnapTagProperty = RegisterProperty("FailedSnapTag", typeof (DivisionTemplateFailedSnapTag));

        /// <summary>Value of the Tag.</summary>
        [ViewModelToModel("FailedSnapTag")]
        public DivisionTemplateFailedSnapTag.AcceptedValues Value
        {
            get { return GetValue<DivisionTemplateFailedSnapTag.AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof (DivisionTemplateFailedSnapTag.AcceptedValues));

        /// <summary>Number of times a type of failed snap was attempted.</summary>
        [ViewModelToModel("FailedSnapTag")]
        public int NumberOfAttempts
        {
            get { return GetValue<int>(NumberOfAttemptsProperty); }
            set { SetValue(NumberOfAttemptsProperty, value); }
        }

        public static readonly PropertyData NumberOfAttemptsProperty = RegisterProperty("NumberOfAttempts", typeof (int), 0);

        #endregion //Model
    }
}