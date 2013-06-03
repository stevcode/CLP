using System;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for KeypadWindowView.xaml
    /// </summary>
    public partial class CustomizeArrayView : Window
    {
        public CustomizeArrayView()
        {
            InitializeComponent();
        }

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(btn.Name == "buttonEnter")
            {
                Rows.Text = Rows.Text + btn.Content;
                Columns.Text = Columns.Text + btn.Content;
            }
            else if(btn.Name == "rowsButton1" || btn.Name == "rowsButton2" || btn.Name == "rowsButton3" ||
               btn.Name == "rowsButton4" || btn.Name == "rowsButton5" || btn.Name == "rowsButton6" ||
               btn.Name == "rowsButton7" || btn.Name == "rowsButton8" || btn.Name == "rowsButton9" ||
               btn.Name == "rowsButton0" || btn.Name == "rowsButtonBackspace")
            {
                Rows.Text = Rows.Text + btn.Content;
            }
            else
            {
                Columns.Text = Columns.Text + btn.Content;
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int rowNum;
            int colNum;
            bool isRowsNum = Int32.TryParse(Rows.Text, out rowNum);
            bool isColsNum = Int32.TryParse(Columns.Text, out colNum);


            if(!(Rows.Text.Length > 0 && Columns.Text.Length > 0 && isRowsNum && isColsNum))
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else if(rowNum < 1 || colNum < 1)
            {
                MessageBox.Show("Oops, it looks like one of the values you entered is not a positive integer", "Oops");
            }
            else
            {
                double ratio = (double)rowNum / (double)colNum;
                if(ratio > 25 || 1 / ratio > 50)
                {
                    MessageBox.Show("The ratio between the numbers you entered is too large. Please try again.", "Okay");
                }
                else
                {
                    this.DialogResult = true;
                }
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(btn.Name == "rowsButtonBackspace" && Rows.Text.Length > 0)
            {
                Rows.Text = Rows.Text.Substring(0, Rows.Text.Length - 1);
            }
            if(btn.Name == "colButtonBackspace" && Columns.Text.Length > 0)
            {
                Columns.Text = Columns.Text.Substring(0, Columns.Text.Length - 1);
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
