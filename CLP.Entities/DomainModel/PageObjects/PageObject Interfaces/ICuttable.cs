using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface ICuttable : IPageObject
    {
        List<IPageObject> Cut(Stroke cuttingStroke);
    }
}