using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ArrayCreationView.xaml
    /// </summary>
    public partial class ArrayCreationView
    {
        public ArrayCreationView()
        {
            InitializeComponent();
            _focusedTextBox = Rows;
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
                _focusedTextBox = Rows;
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
                _focusedTextBox = Rows;
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
            Columns.Background = new SolidColorBrush(Colors.White);
            Rows.Background = new SolidColorBrush(Colors.White);
            NumberOfArrays.Background = new SolidColorBrush(Colors.White);

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
            int rowNum;
            int colNum;
            int numberOfArrays;
            const int MAX_ARRAY_ROWSCOLUMNS = 200;
            const int MAX_NUMBER_OF_ARRAYS = 21;
            var isRowsNum = Int32.TryParse(Rows.Text, out rowNum);
            var isColsNum = Int32.TryParse(Columns.Text, out colNum);
            Int32.TryParse(NumberOfArrays.Text, out numberOfArrays);

            if(!(Rows.Text.Length > 0 && Columns.Text.Length > 0 && isRowsNum && isColsNum))
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else if(rowNum < 1 || colNum < 1)
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else if(rowNum >= MAX_ARRAY_ROWSCOLUMNS || colNum >= MAX_ARRAY_ROWSCOLUMNS)
            {
                MessageBox.Show("You put too many rows or columns. Please keep them below " + MAX_ARRAY_ROWSCOLUMNS + ".", "Oops");
            }
            else if(numberOfArrays > MAX_NUMBER_OF_ARRAYS)
            {
                MessageBox.Show("You cannot put more than " + MAX_NUMBER_OF_ARRAYS + " on the page at once.", "Oops");
            }
            else
            {
                var ratio = rowNum / (double)colNum;
                if(ratio > 60 || 1 / ratio > 60)
                {
                    MessageBox.Show("The ratio between the numbers you entered is too large. Please try again.", "Okay");
                }
                else
                {
                    DialogResult = true;
                }
            }
        }
    }
}
