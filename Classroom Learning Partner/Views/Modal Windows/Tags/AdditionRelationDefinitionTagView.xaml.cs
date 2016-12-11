using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for AdditionRelationDefinitionTagView.xaml.</summary>
    public partial class AdditionRelationDefinitionTagView
    {
        public AdditionRelationDefinitionTagView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }
    }
}