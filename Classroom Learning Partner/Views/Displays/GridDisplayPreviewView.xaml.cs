using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayPreviewView
    {
        public GridDisplayPreviewView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(GridDisplayViewModel); }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is GridDisplayViewModel)
            {
                (ViewModel as GridDisplayViewModel).IsDisplayPreview = true;
            }

            base.OnViewModelChanged();
        }
    }
}