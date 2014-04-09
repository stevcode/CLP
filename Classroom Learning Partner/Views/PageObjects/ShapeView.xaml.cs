using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class ShapeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeView" /> class.
        /// </summary>
        public ShapeView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(ShapeViewModel); }
    }
}