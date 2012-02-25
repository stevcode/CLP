using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Microsoft.Ink;
using System.Windows.Ink;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPInkRegionViewModel : CLPPageObjectBaseViewModel
    {
        public CLPInkRegionViewModel(CLPInkRegion inkRegion, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            PageObject = inkRegion;
        }

        public void InterpretStrokes()
        {
            
        }
    }
}
