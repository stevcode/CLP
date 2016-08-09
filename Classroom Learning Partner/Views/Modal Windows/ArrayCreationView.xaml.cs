using System;
using System.Runtime.InteropServices.WindowsRuntime;
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
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - 150;
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - 300;
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

            if(_focusedTextBox.Text.Length >= 4)
            {
                return;
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

        public bool IsColumnsHidden;
        public bool IsRowsHidden;

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            const int MAX_NUMBER_OF_ARRAYS = 10;
            int numberOfArrays;
            int.TryParse(NumberOfArrays.Text, out numberOfArrays);

            const int OBSCURED_DIMENSION_INCREMENT = 2;
            const int MAX_ARRAY_ROWSCOLUMNS = 301;
            int rowNum = 0;
            int colNum = 0;
            IsRowsHidden = Rows.Text == "N";
            IsColumnsHidden = Columns.Text == "N";
            var isRowsNum = !IsRowsHidden && int.TryParse(Rows.Text, out rowNum);
            var isColsNum = !IsColumnsHidden && int.TryParse(Columns.Text, out colNum);

            if (IsColumnsHidden &&
                isRowsNum)
            {
                colNum = rowNum + OBSCURED_DIMENSION_INCREMENT;
                Columns.Text = colNum.ToString();
                isColsNum = true;
            }
            else if (IsRowsHidden &&
                     isColsNum)
            {
                rowNum = colNum + OBSCURED_DIMENSION_INCREMENT;
                Rows.Text = rowNum.ToString();
                isRowsNum = true;
            }
            if (IsColumnsHidden && IsRowsHidden)
            {
                MessageBox.Show("Oops, Rows and Columns can't both be N.", "Oops");
            }
            else if (!(Rows.Text.Length > 0 && Columns.Text.Length > 0 && isRowsNum && isColsNum))
            {
                MessageBox.Show("Oops, you need to put in a number.", "Oops");
            }
            else if(rowNum < 1 || colNum < 1)
            {
                MessageBox.Show("Oops, you need to put in a number.", "Oops");
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
                    MessageBox.Show("Sorry, you need to use smaller numbers.", "Okay");
                }
                else if(rowNum == 1 && colNum > 44)
                {
                    MessageBox.Show("Sorry, you need to use smaller numbers.", "Okay");
                }
                else if(colNum == 1 && rowNum > 44)
                {
                    MessageBox.Show("Sorry, you need to use smaller numbers.", "Okay");
                }
                else
                {
                    DialogResult = true;
                }
            }
        }
    }
}
