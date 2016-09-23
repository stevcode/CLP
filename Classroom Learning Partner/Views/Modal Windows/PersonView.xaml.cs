using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for PersonView.xaml</summary>
    public partial class PersonView
    {
        public PersonView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - 150;
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - 150;
        }
    }
}