using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Ink;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPInkRegionViewModel : CLPPageObjectBaseViewModel
    {
        private const bool SHOW_RESULTS = true;

        public CLPInkRegionViewModel(CLPInkRegion inkRegion, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            PageObject = inkRegion;
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
        }

        public string StoredAnswer
        {
            get 
            {
                if (SHOW_RESULTS)
                    return (PageObject as CLPInkRegion).StoredAnswer;
                else
                    return "";
            }
        }

        public string AnalysisType
        {
            get
            {
                if (SHOW_RESULTS)
                    return (PageObject as CLPInkRegion).AnalysisType.ToString();
                else
                    return "";
            }
        }
    }
}
