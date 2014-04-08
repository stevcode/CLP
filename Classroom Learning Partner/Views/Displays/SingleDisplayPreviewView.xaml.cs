using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SingleDisplayPreviewView.xaml
    /// </summary>
    public partial class SingleDisplayPreviewView
    {
        public SingleDisplayPreviewView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(SingleDisplayViewModel); }
    }
}