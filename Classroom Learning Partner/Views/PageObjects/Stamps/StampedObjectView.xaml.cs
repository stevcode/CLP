using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StampedObjectView.xaml
    /// </summary>
    public partial class StampedObjectView
    {
        public StampedObjectView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(StampedObjectViewModel); }
    }
}