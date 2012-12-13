using System.Collections.ObjectModel;

namespace CLP.Models
{
    public interface IPageObjectAccepter
    {
        bool CanAcceptPageObjects { get; set; }
        ObservableCollection<string> AcceptedPageObjectParentIDs { get; set; }

        bool HitTest(ICLPPageObject pageObject, double percentage);
        void AcceptObjects(ObservableCollection<ICLPPageObject> addedPageObjects, ObservableCollection<ICLPPageObject> removedPageObjects);
        void RefreshPageObjectParentIDs();
        ObservableCollection<ICLPPageObject> GetPageObjectsOverThisPageObject();
    }
}
