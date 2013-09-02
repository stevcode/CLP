using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayPreviewView
    {
        public GridDisplayPreviewView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(GridDisplayViewModel);
        }
    }
}
