using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for BlankWorkspaceView.xaml
    /// </summary>
    public partial class BlankWorkspaceView
    {
        public BlankWorkspaceView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(BlankWorkspaceViewModel); }
    }
}