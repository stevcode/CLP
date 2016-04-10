using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for TemporaryGridView.xaml</summary>
    public partial class TemporaryGridView
    {
        public TemporaryGridView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(TemporaryGridViewModel); }
    }
}