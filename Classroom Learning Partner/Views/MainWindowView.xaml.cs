using System.ComponentModel;
using System.Threading.Tasks;
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

        private void MainWindowView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources["DynamicMainColor"] = new BrushConverter().ConvertFrom("#2F64B9");
        }

        private bool _isConfirmedClosing = false;

        void MainWindowView_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_isConfirmedClosing)
            {
                e.Cancel = true;
            }
            else
            {
                CLPServiceAgent.Instance.Exit();
            }
        }

        protected async override Task<bool> DiscardChangesAsync()
        {
            if (MessageBox.Show("Are you sure you want to exit now?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                _isConfirmedClosing = false;
                return false;
            }

            _isConfirmedClosing = true;
            return await base.DiscardChangesAsync();
        }
    }
}