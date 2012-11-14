using System;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for KeypadWindowView.xaml
    /// </summary>
    public partial class KeypadWindowView : Window
    {
        public KeypadWindowView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PartsPropertyKey = DependencyProperty.Register(
               "Parts", typeof(String), typeof(KeypadWindowView), new FrameworkPropertyMetadata(" "));


        public String Parts
        {
            get { return (String)GetValue(PartsPropertyKey); }
            set { SetValue(PartsPropertyKey, value); }
        }

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            this.SetValue(PartsPropertyKey, btn.Content);
            this.DialogResult = true;
        }
    }
}
