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
            //Left = 515;
            //Top = 315;
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

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int rowNum;
            int colNum;
            int numberOfArrays = 1;
            const int MAX_ARRAY_ROWSCOLUMNS = 301;
            const int MAX_NUMBER_OF_ARRAYS = 10;
            var isRowsNum = int.TryParse(Rows.Text, out rowNum);
            var isColsNum = int.TryParse(Columns.Text, out colNum);
            int.TryParse(NumberOfArrays.Text, out numberOfArrays);
            var isColumnsHidden = ToggleColumns.IsChecked != null && (bool)ToggleColumns.IsChecked;
            var isRowsHidden = ToggleRows.IsChecked != null && (bool)ToggleRows.IsChecked;

            if (isColumnsHidden &&
                isRowsNum)
            {
                colNum = rowNum + 1;
                Columns.Text = colNum.ToString();
                isColsNum = true;
            }
            else if (isRowsHidden &&
                     isColsNum)
            {
                rowNum = colNum + 1;
                Rows.Text = rowNum.ToString();
                isRowsNum = true;
            }

            if (!(Rows.Text.Length > 0 && Columns.Text.Length > 0 && isRowsNum && isColsNum))
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

        private void ToggleColumns_OnClick(object sender, RoutedEventArgs e)
        {
            var isOtherChecked = ToggleRows.IsChecked != null && (bool)ToggleRows.IsChecked;
            if (isOtherChecked)
            {
                ToggleColumns.IsChecked = false;
                return;
            }

            Columns.Background = new SolidColorBrush(Colors.White);
            Rows.Background = new SolidColorBrush(Colors.White);
            NumberOfArrays.Background = new SolidColorBrush(Colors.White);

            var textBox = Rows;
            if (textBox == null)
            {
                return;
            }

            _focusedTextBox = textBox;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.Background = new SolidColorBrush(Colors.LightGray);

            foreach (var button in KeyPadGrid.Children)
            {
                (button as Button).Foreground = textBox.Foreground;
            }
        }

        private void ToggleRows_OnClick(object sender, RoutedEventArgs e)
        {
            var isOtherChecked = ToggleColumns.IsChecked != null && (bool)ToggleColumns.IsChecked;
            if (isOtherChecked)
            {
                ToggleRows.IsChecked = false;
                return;
            }

            Columns.Background = new SolidColorBrush(Colors.White);
            Rows.Background = new SolidColorBrush(Colors.White);
            NumberOfArrays.Background = new SolidColorBrush(Colors.White);

            var textBox = Columns;
            if (textBox == null)
            {
                return;
            }

            _focusedTextBox = textBox;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.Background = new SolidColorBrush(Colors.LightGray);

            foreach (var button in KeyPadGrid.Children)
            {
                (button as Button).Foreground = textBox.Foreground;
            }
        }
    }
}
