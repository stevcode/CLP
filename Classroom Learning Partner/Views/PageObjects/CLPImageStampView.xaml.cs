using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPImageStampView.xaml
    /// </summary>
    public partial class CLPImageStampView : UserControl
    {
        public CLPImageStampView()
        {
            InitializeComponent();
        }


        private void PartsButton_Click(object sender, RoutedEventArgs e)
        {
            KeypadWindowView keyPad = new KeypadWindowView();
            keyPad.ShowDialog();
            if (keyPad.DialogResult == true) {
                Button partsBtn = sender as Button;
                partsBtn.Content = keyPad.Parts;
            }
        }
        
    }
}
