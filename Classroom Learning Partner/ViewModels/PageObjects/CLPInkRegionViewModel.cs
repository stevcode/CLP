namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using System.Windows.Ink;
    using Catel.Data;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPInkRegionViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPInkRegionViewModel"/> class.
        /// </summary>
        public CLPInkRegionViewModel(CLPInkRegion inkRegion)
            : base()
        {
            PageObject = inkRegion;
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "InkRegionVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string StoredAnswer
        {
            get { return GetValue<string>(StoredAnswerProperty); }
            set { SetValue(StoredAnswerProperty, value); }
        }

        /// <summary>
        /// Register the StoredAnswer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StoredAnswerProperty = RegisterProperty("StoredAnswer", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string CorrectAnswer
        {
            get { return GetValue<string>(CorrectAnswerProperty); }
            set { SetValue(CorrectAnswerProperty, value); }
        }

        /// <summary>
        /// Register the CorrectAnswer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CorrectAnswerProperty = RegisterProperty("CorrectAnswer", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int AnalysisType
        {
            get { return GetValue<int>(AnalysisTypeProperty); }
            set { SetValue(AnalysisTypeProperty, value); }
        }

        /// <summary>
        /// Register the AnalysisType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(int));

        #endregion //Model

    }
}
