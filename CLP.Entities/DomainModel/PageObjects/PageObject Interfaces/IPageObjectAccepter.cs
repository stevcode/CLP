using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IPageObjectAccepter : IPageObject
    {
        bool CanAcceptPageObjects { get; set; }
        List<IPageObject> AcceptedPageObjects { get; set; }
        List<string> AcceptedPageObjectIDs { get; set; }

        void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects);
        void RefreshAcceptedPageObjects();
    }
}