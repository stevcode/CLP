using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MainWindowView.xaml</summary>
    public partial class MainWindowView
    {
        /// <summary>Initializes a new instance of the MainWindowView class.</summary>
        public MainWindowView()
            : base(DataWindowMode.Custom) { InitializeComponent(); }

        private void RibbonWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit now?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                CLPServiceAgent.Instance.Exit();
            }
        }

        private void MainWindowView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources["DynamicMainColor"] = new BrushConverter().ConvertFrom("#2F64B9");
        }
    }
}