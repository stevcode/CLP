using System.Collections.Generic;
using System.Windows;

namespace CLP.Entities.Demo
{
    public interface IPageObjectAccepter : IPageObject
    {
        int PageObjectHitTestPercentage { get; }
        Rect PageObjectAcceptanceBoundingBox { get; }
        bool CanAcceptPageObjects { get; set; }
        List<IPageObject> AcceptedPageObjects { get; set; }
        List<string> AcceptedPageObjectIDs { get; set; }

        void LoadAcceptedPageObjects();
        void ChangeAcceptedPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects);
        bool IsPageObjectTypeAcceptedByThisPageObject(IPageObject pageObject);
        bool IsPageObjectOverThisPageObject(IPageObject pageObject);
        double PercentageOfPageObjectOverThisPageObject(IPageObject pageObject);
        List<IPageObject> GetPageObjectsOverThisPageObject();
    }
}