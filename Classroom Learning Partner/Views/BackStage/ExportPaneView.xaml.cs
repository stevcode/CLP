using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ExportPaneView.xaml</summary>
    public partial class ExportPaneView
    {
        public ExportPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (ExportPaneViewModel); }
    }
}