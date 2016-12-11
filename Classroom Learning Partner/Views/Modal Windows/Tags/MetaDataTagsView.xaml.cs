using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MetaDataTagsView.xaml</summary>
    public partial class MetaDataTagsView
    {
        public const double WINDOW_WIDTH = 300;

        public MetaDataTagsView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }
    }
}