using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for MirrorDisplayPreviewView.xaml
    /// </summary>
    public partial class MirrorDisplayPreviewView
    {
        public MirrorDisplayPreviewView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(MirrorDisplayViewModel);
        }
    }
}
