using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MarkView.xaml.</summary>
    public partial class MarkView
    {
        /// <summary>Initializes a new instance of the <see cref="MarkView" /> class.</summary>
        public MarkView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (MarkViewModel); }
    }
}