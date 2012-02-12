using System.Windows;
using Microsoft.Windows.Controls.Ribbon;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : RibbonWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindowView class.
        /// </summary>
        public MainWindowView()
        {
            InitializeComponent();
            CLPService = new CLPServiceAgent();
        }

        private ICLPServiceAgent CLPService { get; set; }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit now?",
                                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                CLPService.Exit();
            }
        }

        private void RibbonWindow_Closed(object sender, System.EventArgs e)
        {
            (DataContext as MainWindowViewModel).SaveAndCloseViewModel();
        }


    }
}