using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PersonCreationView.xaml
    /// </summary>
    public partial class PersonCreationView
    {
        public PersonCreationView(PersonCreationViewModel viewModel)
            : base(viewModel) { InitializeComponent(); }
    }
}