using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MultipleChoiceBoxView.xaml</summary>
    public partial class MultipleChoiceBoxView
    {
        public MultipleChoiceBoxView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (MultipleChoiceBoxViewModel); }
    }
}