using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>Interaction logic for MultiplicationRelationDefinitionTagView.xaml.</summary>
    public partial class MultiplicationRelationDefinitionTagView : DataWindow
    {
        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagView" /> class.</summary>
        public MultiplicationRelationDefinitionTagView(MultiplicationRelationDefinitionTagViewModel viewModel)
            : base(viewModel) { InitializeComponent(); }
    }
}