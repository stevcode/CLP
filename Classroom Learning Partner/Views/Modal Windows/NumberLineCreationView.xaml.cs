using System;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>Interaction logic for NumberLineCreationView.xaml</summary>
    public partial class NumberLineCreationView
    {
        public NumberLineCreationView() { InitializeComponent(); }

        private const int LIMIT = 85;

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                NumbersEntered.Text = NumbersEntered.Text + btn.Content;
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int partNum;
            var isNum = Int32.TryParse(NumbersEntered.Text, out partNum);
            if (NumbersEntered.Text.Length > 0 &&
                isNum &&
                partNum < LIMIT)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("You need to end at a number less than " + LIMIT, "Oops");
            }
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