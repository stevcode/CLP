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
using System.Windows.Shapes;

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

        public static readonly DependencyProperty PartsPropertyKey = DependencyProperty.Register(
               "Parts", typeof(String), typeof(KeypadWindowView), new FrameworkPropertyMetadata(" "));


        public String Parts
        {
            get { return (String)GetValue(PartsPropertyKey); }
            set { SetValue(PartsPropertyKey, value); }
        }

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            this.SetValue(PartsPropertyKey, btn.Content);
            this.DialogResult = true;
        }
    }
}
