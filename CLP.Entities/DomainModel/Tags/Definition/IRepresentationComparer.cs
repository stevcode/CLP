namespace CLP.Entities
{
    public interface IRepresentationComparer
    {
        Correctness CompareRelationToRepresentations(double groupSize, double numberOfGroups, double product, bool isProductImportant, bool isOrderedGroup);
    }
}