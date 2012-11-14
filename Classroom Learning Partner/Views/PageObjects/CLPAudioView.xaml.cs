using System.Windows;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
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

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CLPAudioViewModel audio = (this.DataContext as CLPAudioViewModel);
            CLPServiceAgent.Instance.RemovePageObjectFromPage(audio.PageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPAudioViewModel audio = (this.DataContext as CLPAudioViewModel);

            double x = audio.XPosition + e.HorizontalChange;
            double y = audio.YPosition + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 1056 - audio.Width)
            {
                x = 1056 - audio.Width;
            }
            if (y > 816 - audio.Height)
            {
                y = 816 - audio.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(audio.PageObject, pt);
        }
    }
}
