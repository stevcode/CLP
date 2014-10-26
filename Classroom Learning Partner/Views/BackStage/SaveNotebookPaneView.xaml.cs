using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for SaveNotebookPaneView.xaml</summary>
    public partial class SaveNotebookPaneView
    {
        public SaveNotebookPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (SaveNotebookPaneViewModel); }
    }
}