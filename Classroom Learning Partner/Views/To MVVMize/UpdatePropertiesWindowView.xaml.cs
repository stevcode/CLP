using System;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for UpdatePropertiesWindowView.xaml
    /// </summary>
    public partial class UpdatePropertiesWindowView : Window
    {
        public UpdatePropertiesWindowView()
        {
            InitializeComponent();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
