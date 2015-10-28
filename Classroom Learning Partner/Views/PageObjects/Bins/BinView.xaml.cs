using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for BinView.xaml.</summary>
    public partial class BinView
    {
        /// <summary>Initializes a new instance of the <see cref="BinView" /> class.</summary>
        public BinView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (BinViewModel); }
    }
}