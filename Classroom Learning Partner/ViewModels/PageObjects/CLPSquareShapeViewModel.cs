using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSquareShapeViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPSquareShapeViewModel(CLPSquareShape square)
            : base()
        {
            PageObject = square;
        }

        public override string Title { get { return "SquareShapeVM"; } }
    }
}
