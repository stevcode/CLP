using System;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for KeypadWindowView.xaml
    /// </summary>
    public partial class KeypadWindowView
    {
        public KeypadWindowView()
            : this("Enter a number:", 99)
        { 
        }

        public KeypadWindowView(string textQuestion, int numberLimit)
        {
            InitializeComponent();
            QuestionText.Text = textQuestion;
            _limit = numberLimit;
        }

        private readonly int _limit;

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            NumbersEntered.Text = NumbersEntered.Text + btn.Content;
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int partNum;
            var isNum = Int32.TryParse(NumbersEntered.Text, out partNum);
            if (NumbersEntered.Text.Length > 0 && isNum && partNum < _limit)
            {
                DialogResult = true;
            }
            else 
            {
                MessageBox.Show("You need to enter a number less than " + _limit, "Oops");
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if(NumbersEntered.Text.Length > 0)
            {
                NumbersEntered.Text = NumbersEntered.Text.Substring(0, NumbersEntered.Text.Length - 1);
            }
        }
    }
}
