using System.Linq;
using System.Windows.Controls.Ribbon;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml.
    /// </summary>
    public partial class RibbonView
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
            var ribbonToggleButton = sender as RibbonToggleButton;
            if(ribbonToggleButton != null)
            {
                var ribbonGroup = ribbonToggleButton.Parent as RibbonGroup;
                if(ribbonGroup != null)
                {
                    foreach(var toggleButton in ribbonGroup.Items.Cast<object>().Where(item => item.GetType() == typeof(RibbonToggleButton)).OfType<RibbonToggleButton>())
                    {
                        toggleButton.IsChecked = false;
                    }
                }
            }
            EditObjectPropertiesToggleButton.IsChecked = false;

            var button = sender as RibbonToggleButton;
            if(button != null)
            {
                button.IsChecked = true;
            }
        }

        private void DebugGroupToggle_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach(var ribbonToggleButton in ToolsRibbonGroup.Items.Cast<object>().Where(item => item.GetType() == typeof(RibbonToggleButton)).OfType<RibbonToggleButton>())
            {
                ribbonToggleButton.IsChecked = false;
            }
            var toggleButton = sender as RibbonToggleButton;
            if(toggleButton != null)
            {
                toggleButton.IsChecked = true;
            }
        }

        private int _minimizeClick;
        private void MinimizeButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            _minimizeClick++;
            Logger.Instance.WriteToLog("[METRICS LOGGING]: Ribbon minimized " + _minimizeClick + " times.");
        }

        private void MinimizeButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Logger.Instance.WriteToLog("[METRICS LOGGING]: Ribbon restored " + _minimizeClick + " times.");
        }
    }
}
