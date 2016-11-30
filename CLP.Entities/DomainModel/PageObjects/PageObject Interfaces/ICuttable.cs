using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities.Ann
{
    public interface ICuttable : IPageObject
    {
        List<IPageObject> Cut(Stroke cuttingStroke);
    }
}