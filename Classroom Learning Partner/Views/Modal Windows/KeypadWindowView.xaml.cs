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

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            NumbersEntered.Text = NumbersEntered.Text + btn.Content;
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int partNum;
            bool isNum = Int32.TryParse(NumbersEntered.Text, out partNum);
            if (NumbersEntered.Text.Length > 0 && isNum)
            {
                this.DialogResult = true;
            }
            else {
                MessageBox.Show("Oops, the parts doesn't look quite right. Are you sure it is a positive integer?", "Oops");
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if(NumbersEntered.Text.Length > 0)
            {
                NumbersEntered.Text = NumbersEntered.Text.Substring(0, NumbersEntered.Text.Length - 1);
            }
        }

        //void OnClosing(System.ComponentModel.CancelEventArgs e)
        //{
        //    base.OnClosing(e);
        //    if(Owner != null)
        //    {
        //        Owner.Activate();
        //    }
        //}
    }
}
