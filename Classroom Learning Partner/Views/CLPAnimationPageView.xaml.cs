using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPProofPageView.xaml
    /// </summary>
    public partial class CLPAnimationPageView
    {
        public CLPAnimationPageView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPAnimationPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPAnimationPageViewModel)
            {
                (ViewModel as CLPAnimationPageViewModel).TopCanvas = TopCanvas;
                (ViewModel as CLPAnimationPageViewModel).IsPagePreview = false;
            }
            
            base.OnViewModelChanged();
        }

        private void Slider_ValueChanged_1(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if(ViewModel!=null)
            {
                (ViewModel as CLPAnimationPageViewModel).Slider_ValueChanged_1b(sender, e);
            }
        }
    }
}
