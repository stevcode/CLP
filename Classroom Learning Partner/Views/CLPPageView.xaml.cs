using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView
    {
        public CLPPageView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPPageViewModel)
            {
                (ViewModel as CLPPageViewModel).TopCanvas = TopCanvas;
                (ViewModel as CLPPageViewModel).IsPagePreview = false;
            }
            
            base.OnViewModelChanged();
        }
    }
}
