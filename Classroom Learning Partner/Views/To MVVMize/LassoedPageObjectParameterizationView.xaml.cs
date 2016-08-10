using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for LassoedPageObjectParameterizationView.xaml
    /// </summary>
    public partial class LassoedPageObjectParameterizationView : Window
    {
        public LassoedPageObjectParameterizationView()
        {
            InitializeComponent();
            _focusedTextBox = NumberOfCopies;
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
                _focusedTextBox = NumberOfCopies;
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
                _focusedTextBox = NumberOfCopies;
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
            NumberOfCopies.Background = new SolidColorBrush(Colors.White);

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
            int numberOfCopies;
            var isRowsNum = Int32.TryParse(NumberOfCopies.Text, out numberOfCopies);

            if(!isRowsNum && numberOfCopies < 1)
            {
                MessageBox.Show("Oops, you need to have 1 or more copies", "Oops");
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
