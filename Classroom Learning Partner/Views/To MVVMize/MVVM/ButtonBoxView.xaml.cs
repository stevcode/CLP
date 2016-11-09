using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ButtonBoxView.xaml</summary>
    public partial class ButtonBoxView
    {
        public ButtonBoxView(string text, List<string> buttonValues)
        {
            Text = text;
            ButtonLabels = buttonValues;
            InitializeComponent();
        }

        public string Text { get; set; }
        public List<string> ButtonLabels { get; set; }
        public string ButtonBoxReturnValue { get; set; }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ButtonBoxReturnValue = button.Content as string;
            DialogResult = true;
        }
    }
}