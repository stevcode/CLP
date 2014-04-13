using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IPageObjectAccepter : IPageObject
    {
        bool CanAcceptPageObjects { get; set; }
        ObservableCollection<string> AcceptedPageObjectIDs { get; set; }

        void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects);
        void RefreshAcceptedPageObjects();
    }
}