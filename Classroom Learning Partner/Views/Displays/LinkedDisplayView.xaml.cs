using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for LinkedDisplayView.xaml
    /// </summary>
    public partial class LinkedDisplayView : Catel.Windows.Controls.UserControl
    {
        public LinkedDisplayView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(LinkedDisplayViewModel);
        }

        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            double pageAspectRatio = (ViewModel as LinkedDisplayViewModel).DisplayedPage.PageAspectRatio;

            double borderWidth = ActualWidth - 20;
            double borderHeight = borderWidth / pageAspectRatio;

            if(borderHeight > ActualHeight - 20)
            {
                borderHeight = ActualHeight - 20;
                borderWidth = borderHeight * pageAspectRatio;
            }

            PageBorder.Height = borderHeight;
            PageBorder.Width = borderWidth;

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
