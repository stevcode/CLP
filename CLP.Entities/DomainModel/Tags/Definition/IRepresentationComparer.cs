using System.Collections.Generic;

namespace CLP.Entities.Demo
{
    public interface IRepresentationComparer
    {
        Correctness CompareRelationToRepresentations(double groupSize, double numberOfGroups, double product, bool isProductImportant, bool isOrderedGroup);
    }
}