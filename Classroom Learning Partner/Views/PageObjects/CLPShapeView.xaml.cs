using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class CLPShapeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPSquareShapeView"/> class.
        /// </summary>
        public CLPShapeView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPShapeViewModel);
        }
    }
}
