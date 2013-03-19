using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml.
    /// </summary>
    public partial class RibbonView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonView"/> class.
        /// </summary>
        public RibbonView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(RibbonViewModel);
        }

        private void ToolGroupToggle_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach(var item in ((sender as RibbonToggleButton).Parent as RibbonGroup).Items)
            {
                if(item.GetType() == typeof(RibbonToggleButton))
                {
                    (item as RibbonToggleButton).IsChecked = false;
                }
            }
            EditObjectPropertiesToggleButton.IsChecked = false;

            (sender as RibbonToggleButton).IsChecked = true;
        }

        private void DebugGroupToggle_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in ToolsRibbonGroup.Items)
            {
                if (item.GetType() == typeof(RibbonToggleButton))
                {
                    (item as RibbonToggleButton).IsChecked = false;
                }
            }
            (sender as RibbonToggleButton).IsChecked = true;
        }

        private int minimizeClick = 0;
        private void MinimizeButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            minimizeClick++;
            Logger.Instance.WriteToLog("[METRICS LOGGING]: Ribbon minimized " + minimizeClick + " times.");
        }

        private void MinimizeButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Logger.Instance.WriteToLog("[METRICS LOGGING]: Ribbon restored " + minimizeClick + " times.");
        }
    }
}
