using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GroupCreationPanel.xaml
    /// </summary>
    public partial class GroupCreationPanel
    {
        public GroupCreationPanel()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType() { return typeof(GroupCreationViewModel); }
    }
}
