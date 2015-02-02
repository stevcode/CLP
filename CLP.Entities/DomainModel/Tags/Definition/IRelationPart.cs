namespace CLP.Entities
{
    public interface IRelationPart
    {
        double RelationPartAnswerValue { get; }
        string FormattedRelation { get; }
        string ExpandedFormattedRelation { get; }
    }
}