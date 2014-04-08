using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml.
    /// </summary>
    public partial class RibbonView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonView" /> class.
        /// </summary>
        public RibbonView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(RibbonViewModel); }
    }
}