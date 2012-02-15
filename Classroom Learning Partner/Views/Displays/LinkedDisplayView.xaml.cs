using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels.Displays;
using System;

namespace Classroom_Learning_Partner.Views.Displays
{
    /// <summary>
    /// Interaction logic for LinkedDisplayView.xaml
    /// </summary>
    public partial class LinkedDisplayView : UserControl<LinkedDisplayViewModel>
    {
        public LinkedDisplayView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("DisplayView unloaded");
        }
    }
}
