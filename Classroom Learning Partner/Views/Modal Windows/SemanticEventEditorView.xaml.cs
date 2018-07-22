using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for SemanticEventEditorView.xaml</summary>
    public partial class SemanticEventEditorView
    {
        public const double WINDOW_WIDTH = 600;

        public SemanticEventEditorView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }
    }
}
