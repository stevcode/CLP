﻿namespace Classroom_Learning_Partner.Views
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels;

    /// <summary>
    /// Interaction logic for WebcamPanel.xaml.
    /// </summary>
    public partial class WebcamPanelView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebcamPanel"/> class.
        /// </summary>
        public WebcamPanelView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(WebcamPanelViewModel);
        }
    }
}
