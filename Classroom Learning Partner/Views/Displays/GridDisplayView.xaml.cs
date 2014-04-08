using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayView
    {
        public GridDisplayView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(GridDisplayViewModel); }
    }
}