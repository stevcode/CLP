using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for FuzzyFactorCardWithTilesCreationView.xaml
    /// </summary>
    public partial class FuzzyFactorCardWithTilesCreationView
    {
        public FuzzyFactorCardWithTilesCreationView()
        {
            InitializeComponent();
            _focusedTextBox = Product;
            _focusedTextBox.Focus();
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
            _focusedTextBox.Background = new SolidColorBrush(Colors.LightGray);

            foreach(var button in KeyPadGrid.Children)
            {
                (button as Button).Foreground = _focusedTextBox.Foreground;
            }
        }

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button == null)
            {
                return;
            }

            if(_focusedTextBox == null)
            {
                _focusedTextBox = Product;
                _focusedTextBox.Focus();
                _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
                _focusedTextBox.Background = new SolidColorBrush(Colors.LightGray);
            }

            _focusedTextBox.Text += button.Content;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if(_focusedTextBox == null)
            {
                _focusedTextBox = Product;
                _focusedTextBox.Focus();
                _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
                _focusedTextBox.Background = new SolidColorBrush(Colors.LightGray);
            }

            if(_focusedTextBox.Text.Length > 0)
            {
                _focusedTextBox.Text = _focusedTextBox.Text.Substring(0, _focusedTextBox.Text.Length - 1);
            }
        }

        private TextBox _focusedTextBox;
        private void TextBox_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Product.Background = new SolidColorBrush(Colors.White);
            Factor.Background = new SolidColorBrush(Colors.White);

            var textBox = sender as TextBox;
            if(textBox == null)
            {
                return;
            }

            _focusedTextBox = textBox;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.Background = new SolidColorBrush(Colors.LightGray);

            foreach(var button in KeyPadGrid.Children)
            {
                (button as Button).Foreground = textBox.Foreground;
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int productNum;
            int factorNum;
            const int MAX_ARRAY_ROWSCOLUMNS = 200;
            var isRowsNum = Int32.TryParse(Product.Text, out productNum);
            var isColsNum = Int32.TryParse(Factor.Text, out factorNum);

            if(!(Product.Text.Length > 0 && Factor.Text.Length > 0 && isRowsNum && isColsNum))
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else if(productNum < 1 || factorNum < 1)
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else if(productNum >= 51)
            {
                MessageBox.Show("Your product is too big a number. Please keep it below " + 51 + ".", "Oops");
            }
            else if(factorNum >= MAX_ARRAY_ROWSCOLUMNS)
            {
                MessageBox.Show("Your factor is too big a number. Please keep it below " + MAX_ARRAY_ROWSCOLUMNS + ".", "Oops");
            }
            else if(productNum / (factorNum * factorNum) > 5 || (factorNum * factorNum) / productNum > 8)
            {
                MessageBox.Show("The ratio between the dimensions for the numbers you entered is too large. Please try again.", "Okay");
            }
            else
            {
                var answer = productNum / (double)factorNum;
                DialogResult = true;
            }
        }
    }
}