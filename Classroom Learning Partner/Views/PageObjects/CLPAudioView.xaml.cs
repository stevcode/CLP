using Classroom_Learning_Partner.ViewModels.PageObjects;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    public partial class CLPAudioView : Catel.Windows.Controls.UserControl
    {
        public CLPAudioView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }
        protected override System.Type GetViewModelType()
        {
            return typeof(CLPAudioViewModel);
        }
    }
}
