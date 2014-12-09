using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for AnimationControlRibbonView.xaml</summary>
    public partial class AnimationControlRibbonView
    {
        public AnimationControlRibbonView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(AnimationControlRibbonViewModel); }
    }
}