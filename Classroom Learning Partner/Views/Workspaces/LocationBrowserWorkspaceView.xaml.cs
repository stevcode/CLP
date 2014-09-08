using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for LocationBrowserWorkspaceView.xaml</summary>
    public partial class LocationBrowserWorkspaceView
    {
        public LocationBrowserWorkspaceView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (LocationBrowserWorkspaceViewModel); }
    }
}