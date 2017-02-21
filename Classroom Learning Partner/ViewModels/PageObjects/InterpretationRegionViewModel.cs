using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class InterpretationRegionViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public InterpretationRegionViewModel(InterpretationRegion interpretationRegion)
        {
            PageObject = interpretationRegion;
        }

        #endregion // Constructor

        #region Static Methods

        public static bool InteractWithAcceptedStrokes(InterpretationRegion interpretationRegion, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            if (interpretationRegion == null)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();

            interpretationRegion.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);
            ACLPPageBaseViewModel.AddHistoryActionToPage(interpretationRegion.ParentPage,
                                                         new FillInAnswerChangedHistoryAction(interpretationRegion.ParentPage,
                                                                                              App.MainWindowViewModel.CurrentUser,
                                                                                              interpretationRegion,
                                                                                              addedStrokesList,
                                                                                              removedStrokesList));

            return true;
        }

        public static void AddInterpretationRegionToPage(CLPPage page)
        {
            var interpretationRegion = new InterpretationRegion(page);
            ACLPPageBaseViewModel.AddPageObjectToPage(interpretationRegion);
        }

        #endregion //Static Methods
    }
}