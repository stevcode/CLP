using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSnapTileView.xaml
    /// </summary>
    public partial class CLPSnapTileContainerView : Catel.Windows.Controls.UserControl
    {
        public CLPSnapTileContainerView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPSnapTileContainerViewModel);
        }

    }
}
