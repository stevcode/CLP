using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MultipleChoiceView.xaml</summary>
    public partial class MultipleChoiceView
    {
        public MultipleChoiceView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (MultipleChoiceViewModel); }
    }
}