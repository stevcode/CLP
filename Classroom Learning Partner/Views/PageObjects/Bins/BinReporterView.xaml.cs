using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for BinReporterView.xaml.</summary>
    public partial class BinReporterView
    {
        /// <summary>Initializes a new instance of the <see cref="BinReporterView" /> class.</summary>
        public BinReporterView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (BinReporterViewModel); }
    }
}