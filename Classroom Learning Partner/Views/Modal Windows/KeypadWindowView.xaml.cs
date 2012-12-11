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

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            NumbersEntered.Text = NumbersEntered.Text + btn.Content;
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            this.DialogResult = true;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (NumbersEntered.Text.Length > 0)
            {
                NumbersEntered.Text = NumbersEntered.Text.Substring(0, NumbersEntered.Text.Length - 1);
            }
        }
    }
}
