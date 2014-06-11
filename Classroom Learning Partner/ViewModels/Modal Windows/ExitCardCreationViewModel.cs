using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ExitCardCreationViewModel : ViewModelBase
    {

        public ExitCardCreationViewModel()
        {
            CLPPage basePage = new CLPPage();

            for(int i = 0; i < 4; i++)
            {
                CLPPage differentiatedPage = basePage.DuplicatePage();
                differentiatedPage.ID = basePage.ID;
                differentiatedPage.PageNumber = 999;
                differentiatedPage.DifferentiationLevel = "" + (char)('A' + i);
                foreach(var pageObject in differentiatedPage.PageObjects)
                {
                    pageObject.DifferentiationLevel = differentiatedPage.DifferentiationLevel;
                }
                foreach(var historyItem in differentiatedPage.History.UndoItems)
                {
                    historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
                }
                foreach(var historyItem in differentiatedPage.History.RedoItems)
                {
                    historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
                }
                foreach(var stroke in differentiatedPage.InkStrokes)
                {
                    stroke.SetStrokeDifferentiationGroup(differentiatedPage.DifferentiationLevel);
                }
                ExitCards.Add(differentiatedPage);
            }
        }

        public ObservableCollection<CLPPage> ExitCards
        {
            get { return GetValue<ObservableCollection<CLPPage>>(ExitCardsProperty); }
            set { SetValue(ExitCardsProperty, value); }
        }

        public static readonly PropertyData ExitCardsProperty = RegisterProperty("ExitCards", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());
    }
}
