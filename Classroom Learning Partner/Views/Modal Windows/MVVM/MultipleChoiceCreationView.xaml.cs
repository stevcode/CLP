using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MultipleChoiceCreationView.xaml.</summary>
    public partial class MultipleChoiceCreationView
    {
        /// <summary>Initializes a new instance of the <see cref="MultipleChoiceCreationView" /> class.</summary>
        public MultipleChoiceCreationView(MultipleChoiceCreationViewModel viewModel)
            : base(viewModel) { InitializeComponent(); }
    }
}