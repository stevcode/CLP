using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookSelectorView.xaml
    /// </summary>
    public partial class NotebookSelectorView : Catel.Windows.Controls.UserControl
    {
        public NotebookSelectorView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookSelectorViewModel);
        }
    }
}
