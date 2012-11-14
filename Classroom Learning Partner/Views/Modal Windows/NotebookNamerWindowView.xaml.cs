using System.Windows;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for NotebookNamerWindowView.xaml
    /// </summary>
    public partial class NotebookNamerWindowView : Window
    {
        public NotebookNamerWindowView()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
