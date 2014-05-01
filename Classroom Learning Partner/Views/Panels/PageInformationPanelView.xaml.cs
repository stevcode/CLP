using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PageInformationPanelView.xaml
    /// </summary>
    public partial class PageInformationPanelView
    {
        public PageInformationPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        protected override Type GetViewModelType() { return typeof(PageInformationPanelViewModel); }
    }
}