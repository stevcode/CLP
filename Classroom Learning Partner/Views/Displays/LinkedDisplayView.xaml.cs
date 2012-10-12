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

        //ar = w / h
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            ResizePage();

            base.OnRenderSizeChanged(sizeInfo);
        }

        private void ResizePage()
        {
            double pageAspectRatio = (ViewModel as LinkedDisplayViewModel).DisplayedPage.PageAspectRatio;
            double pageHeight = (ViewModel as LinkedDisplayViewModel).DisplayedPage.PageHeight;
            double pageWidth = (ViewModel as LinkedDisplayViewModel).DisplayedPage.PageWidth;
            double scrolledAspectRatio = pageWidth / pageHeight;

            double borderWidth = ActualWidth - 20;
            double borderHeight = borderWidth / pageAspectRatio;

            if(borderHeight > ActualHeight - 20)
            {
                borderHeight = ActualHeight - 20;
                borderWidth = borderHeight * pageAspectRatio;
            }

            PageBorder.Height = borderHeight;
            PageBorder.Width = borderWidth;

            DimensionBorder.Width = borderWidth - 2;
            DimensionBorder.Height = DimensionBorder.Width / scrolledAspectRatio;
        }

        private void CLPPageView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ResizePage();
        }
    }
}
