using System;
using System.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ProgressPanelView.xaml
    /// </summary>
    public partial class ProgressPanelView
    {
        public ProgressPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;

            //MainScrollViewer.ScrollChanged += new ScrollChangedEventHandler(MainScrollViewer_ScrollChanged);
        }

        void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
          //  NamesScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        protected override Type GetViewModelType() { return typeof(ProgressPanelViewModel); }
    }
}