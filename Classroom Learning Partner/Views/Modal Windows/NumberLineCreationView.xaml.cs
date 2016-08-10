using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NumberLineCreationView.xaml</summary>
    public partial class NumberLineCreationView
    {
        public NumberLineCreationView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - 150;
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - 150;
        }
    }
}