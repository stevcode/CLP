namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels.PageObjects;

    /// <summary>
    /// Interaction logic for CLPStrokePathContainerView.xaml.
    /// </summary>
    public partial class CLPStrokePathContainerView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStrokePathContainerView"/> class.
        /// </summary>
        public CLPStrokePathContainerView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStrokePathContainerViewModel);
        }
    }
}
