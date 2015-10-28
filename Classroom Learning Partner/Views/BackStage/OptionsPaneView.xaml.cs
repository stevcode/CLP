using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for OptionsPaneView.xaml</summary>
    public partial class OptionsPaneView
    {
        public OptionsPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (OptionsPaneViewModel); }
    }
}