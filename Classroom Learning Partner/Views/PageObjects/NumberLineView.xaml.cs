using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NumberLineView.xaml</summary>
    public partial class NumberLineView
    {
        public NumberLineView() { InitializeComponent(); }
        protected override Type GetViewModelType() { return typeof (NumberLineViewModel); }
    }
}