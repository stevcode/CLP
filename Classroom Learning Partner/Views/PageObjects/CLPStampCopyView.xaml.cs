using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampCopyView.xaml
    /// </summary>
    public partial class CLPStampCopyView
    {
        public CLPStampCopyView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPStampCopyViewModel);
        }
    }
}
