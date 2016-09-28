using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for PersonView.xaml</summary>
    public partial class PersonView
    {
        public const double WINDOW_WIDTH = 300;

        public PersonView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (WINDOW_WIDTH / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (WINDOW_WIDTH / 2);
        }
    }
}