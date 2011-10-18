using System.Windows;
using Classroom_Learning_Partner.ViewModels;
using Microsoft.Windows.Controls.Ribbon;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
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
    }
}