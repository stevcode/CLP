using System;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPageThumbnailView
    {
        public CLPPageThumbnailView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(CLPPageViewModel); }
    }
}
