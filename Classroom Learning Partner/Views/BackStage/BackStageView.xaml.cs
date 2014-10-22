using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for BackStageView.xaml</summary>
    public partial class BackStageView
    {
        public BackStageView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(BackStageViewModel); }
    }
}