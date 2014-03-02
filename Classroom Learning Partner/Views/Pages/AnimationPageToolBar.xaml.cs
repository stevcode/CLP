using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for AnimationPageToolBar.xaml
    /// </summary>
    public partial class AnimationPageToolBar
    {
        public AnimationPageToolBar() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(CLPAnimationPageViewModel); }
    }
}