namespace Classroom_Learning_Partner.Views
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels;

    /// <summary>
    /// Interaction logic for DisplayListPanelView.xaml.
    /// </summary>
    public partial class DisplayListPanelView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayListPanelView"/> class.
        /// </summary>
        public DisplayListPanelView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(DisplayListPanelViewModel);
        }
    }
}
