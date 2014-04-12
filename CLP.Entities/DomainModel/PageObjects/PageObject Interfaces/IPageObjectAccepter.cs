using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IPageObjectAccepter
    {
        bool CanAcceptPageObjects { get; set; }
        List<IPageObject> AcceptedPageObjects { get; }
        ObservableCollection<string> AcceptedPageObjectIDs { get; set; }

        void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects);
        void RefreshAcceptedPageObjects();
    }
}