using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for UserLoginWorkspaceView.xaml
    /// </summary>
    public partial class UserLoginWorkspaceView
    {
        public UserLoginWorkspaceView() { InitializeComponent(); }
        protected override Type GetViewModelType() { return typeof(UserLoginWorkspaceViewModel); }
    }
}