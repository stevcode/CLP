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
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.Views.Adorners
{
    /// <summary>
    /// Interaction logic for CloseButtonView.xaml
    /// </summary>
    public partial class CloseButtonView : UserControl
    {
        public CloseButtonView()
        {
            InitializeComponent();
            CLPService = new CLPServiceAgent();
        }

        private ICLPServiceAgent CLPService { get; set; }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement _p = this.Parent as FrameworkElement;
            while (_p.Parent != null)
            {
                _p = _p.Parent as FrameworkElement;
                if (_p.Name == "adornedControl")
                {
                    PageObjectContainerView po = VisualTreeHelper.GetParent(_p) as PageObjectContainerView;
                    break;
                }
            }

            PageObjectContainerViewModel pageObjectContainerViewModel = ((_p as PageObjectContainerView).DataContext as PageObjectContainerViewModel);
            CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
        }
    }
}
