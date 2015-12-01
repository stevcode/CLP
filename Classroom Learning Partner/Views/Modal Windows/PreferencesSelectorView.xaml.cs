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
using Classroom_Learning_Partner.ViewModels.Modal_Windows;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PreferencesSelectorView : Window
    {
        private PreferencesSelectorViewModel preferencesSelectorViewModel;

        public PreferencesSelectorView()
        {
            InitializeComponent();
        }

        public PreferencesSelectorView(PreferencesSelectorViewModel preferencesSelectorViewModel)
        {
            this.preferencesSelectorViewModel = preferencesSelectorViewModel;
        }

        private void HandleChange(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Checkbox clicked");
        }

    }
}
