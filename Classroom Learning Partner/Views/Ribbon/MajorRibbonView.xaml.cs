using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MajorRibbonView.xaml</summary>
    public partial class MajorRibbonView
    {
        public MajorRibbonView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(RibbonViewModel); }
    }
}