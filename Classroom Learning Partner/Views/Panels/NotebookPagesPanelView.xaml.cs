using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookPagesPanelView.xaml.
    /// </summary>
    public partial class NotebookPagesPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelView" /> class.
        /// </summary>
        public NotebookPagesPanelView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(NotebookPagesPanelViewModel); }
    }
}