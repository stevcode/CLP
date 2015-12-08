using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IRepresentationComparer
    {
        Correctness CompareRelationToRepresentations(List<IPageObject> pageObjects);
    }
}