using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    public partial class CLPAudioView : Catel.Windows.Controls.UserControl
    {
        public CLPAudioView()
        {
            InitializeComponent();
        }
        protected override System.Type GetViewModelType()
        {
            return typeof(CLPAudioViewModel);
        }
    }
}
