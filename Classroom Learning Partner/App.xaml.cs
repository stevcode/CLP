using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
