using System.ComponentModel;
using System.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    ///     Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView
    {
        /// <summary>
        ///     Initializes a new instance of the MainWindowView class.
        /// </summary>
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Closing(object sender, CancelEventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to exit now?", "Confirmation", MessageBoxButton.YesNo) ==
               MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                CLPServiceAgent.Instance.Exit();
            }
        }
    }
}