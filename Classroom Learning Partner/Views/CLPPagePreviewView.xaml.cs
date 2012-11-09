using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView : Catel.Windows.Controls.UserControl
    {
        public CLPPagePreviewView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }
    }
}
