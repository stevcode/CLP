using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities.Old
{
    public interface ICuttable : IPageObject
    {
        List<IPageObject> Cut(Stroke cuttingStroke);
    }
}