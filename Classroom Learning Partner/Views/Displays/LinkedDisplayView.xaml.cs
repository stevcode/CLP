using Classroom_Learning_Partner.ViewModels.Displays;
using System;

namespace Classroom_Learning_Partner.Views.Displays
{
    /// <summary>
    /// Interaction logic for LinkedDisplayView.xaml
    /// </summary>
    public partial class LinkedDisplayView : Catel.Windows.Controls.UserControl
    {
        public LinkedDisplayView()
        {
            InitializeComponent();
            //CloseViewModelOnUnloaded = false;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("DisplayView unloaded");
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(LinkedDisplayViewModel);
        }
    }
}
