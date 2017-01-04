using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities.Demo
{
    public interface ICuttable : IPageObject
    {
        double CuttingStrokeDistance(Stroke cuttingStroke);
        List<IPageObject> Cut(Stroke cuttingStroke);
    }
}