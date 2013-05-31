using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStrokePathContainerView.xaml
    /// </summary>
    public partial class CLPStrokePathContainerView
    {
        public CLPStrokePathContainerView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPStrokePathContainerViewModel);
        }
    }
}
