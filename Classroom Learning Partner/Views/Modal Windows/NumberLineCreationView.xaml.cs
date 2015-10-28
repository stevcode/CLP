using System;
using System.Windows;
using System.Windows.Controls;
using CLP.Entities;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>Interaction logic for NumberLineCreationView.xaml</summary>
    public partial class NumberLineCreationView
    {
        public NumberLineCreationView()
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - 150;
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - 150;
        }

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
                partNum <= NumberLine.NUMBER_LINE_MAX_SIZE)
            {
                DialogResult = true;
            }
            else
            {
                var limit = NumberLine.NUMBER_LINE_MAX_SIZE + 1;
                MessageBox.Show("You need to end at a number less than " + limit, "Oops");
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