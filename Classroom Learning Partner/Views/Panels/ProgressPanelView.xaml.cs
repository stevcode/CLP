using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ProgressPanelView.xaml</summary>
    public partial class ProgressPanelView
    {
        public ProgressPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private void MainScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            for (int i = 0; i < StudentNotebooks.Items.Count; i++)
            {
                var c = StudentNotebooks.ItemContainerGenerator.ContainerFromItem(StudentNotebooks.Items[i]) as ContentPresenter;
                if (c == null)
                {
                    return;
                }
                var scroll = c.ContentTemplate.FindName("StudentScrollViewer", c) as ScrollViewer;

                if (scroll != null)
                {
                    scroll.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
            }
        }
    }
}