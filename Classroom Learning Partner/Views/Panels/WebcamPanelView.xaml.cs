using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for WebcamPanel.xaml.
    /// </summary>
    public partial class WebcamPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebcamPanel" /> class.
        /// </summary>
        public WebcamPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        protected override Type GetViewModelType() { return typeof(WebcamPanelViewModel); }
    }
}