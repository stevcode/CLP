using Classroom_Learning_Partner.ViewModels;
using System;

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
