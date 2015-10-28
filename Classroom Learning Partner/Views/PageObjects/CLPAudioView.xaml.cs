using System;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    public partial class CLPAudioView
    {
        public CLPAudioView() { InitializeComponent(); }
        protected override Type GetViewModelType() { return typeof (CLPAudioViewModel); }
    }
}