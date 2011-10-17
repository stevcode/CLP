using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            //MainWindow window = new MainWindow();
            //var viewModel = new MainViewModel();
            //window.DataContext = viewModel;
            //window.Show();

            DispatcherHelper.Initialize();
        }
    }
}
