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




        #endregion //Model
    }
}