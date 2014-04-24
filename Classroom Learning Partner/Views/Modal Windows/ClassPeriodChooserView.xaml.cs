using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ClassPeriodChooserView.xaml
    /// </summary>
    public partial class ClassPeriodChooserView
    {
        public ClassPeriodChooserView(ObservableCollection<ClassPeriodForDisplay> ClassPeriods)
        {
            DataContext = ClassPeriods;
            InitializeComponent();
        }

        public void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
