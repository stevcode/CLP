using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for InterpretationRegionView.xaml</summary>
    public partial class InterpretationRegionView
    {
        public InterpretationRegionView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof (InterpretationRegionViewModel);
        }
    }
}