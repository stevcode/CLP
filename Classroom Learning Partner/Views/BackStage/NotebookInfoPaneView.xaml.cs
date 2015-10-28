using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NotebookInfoPaneView.xaml</summary>
    public partial class NotebookInfoPaneView
    {
        public NotebookInfoPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (NotebookInfoPaneViewModel); }
    }
}