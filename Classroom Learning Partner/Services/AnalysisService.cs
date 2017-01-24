using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    

    public class AnalysisService
    {
        public static void GenerateAnalysisEntryForPage(CLPPage page)
        {
            #region Page Identification

            var entry = new AnalysisEntry(page.Owner.FullName, page.PageNumber);
            entry.SubmissionTime = page.SubmissionTime == null ? AnalysisEntry.UNSUBMITTED : $"{page.SubmissionTime:yyyy-MM-dd HH:mm:ss}";

            #endregion // Page Identification

            #region Problem Characteristics

            

            #endregion // Problem Characteristics

            #region Whole Page Characteristics

            

            #endregion // Whole Page Characteristics


            #region Whole Page Analysis

            var studentInkStrokes = page.InkStrokes.Concat(page.History.TrashedInkStrokes).Where(s => s.GetStrokeOwnerID() == page.Owner.ID).ToList();
            var colorsUsed = studentInkStrokes.Select(s => s.DrawingAttributes.Color).Distinct();
            entry.InkColorsUsedCount = colorsUsed.Count();

            #endregion // Whole Page Analysis

            #region Total History

            var pass3Event = page.History.SemanticEvents.FirstOrDefault(h => h.CodedObject == "PASS" && h.CodedObjectID == "3");
            var pass3Index = page.History.SemanticEvents.IndexOf(pass3Event);
            var pass3 = page.History.SemanticEvents.Skip(pass3Index + 1).Select(h => h.CodedValue).ToList();
            entry.FinalSemanticEvents = pass3;

            #endregion // Total History
        }
    }
}
