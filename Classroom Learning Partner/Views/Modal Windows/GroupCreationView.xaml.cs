using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GroupCreationView.xaml
    /// </summary>
    public partial class GroupCreationView
    {
        public GroupCreationView(GroupCreationViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}