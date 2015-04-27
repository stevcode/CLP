using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for RemainderTilesView.xaml</summary>
    public partial class RemainderTilesView
    {
        /// <summary>Initializes a new instance of the <see cref="RemainderTilesView" /> class.</summary>
        public RemainderTilesView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (RemainderTilesViewModel); }
    }
}