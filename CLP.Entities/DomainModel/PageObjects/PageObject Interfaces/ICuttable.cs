using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface ICuttable
    {
        List<IPageObject> Cut(Stroke cuttingStroke);
    }
}