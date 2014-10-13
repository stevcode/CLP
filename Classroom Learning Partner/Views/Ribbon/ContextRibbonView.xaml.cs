using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ContextRibbonView.xaml</summary>
    public partial class ContextRibbonView
    {
        public ContextRibbonView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (ContextRibbonViewModel); }
    }
}