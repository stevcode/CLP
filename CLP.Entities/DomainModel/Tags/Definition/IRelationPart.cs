namespace CLP.Entities.Demo
{
    public interface IRelationPart
    {
        double RelationPartAnswerValue { get; }
        string FormattedRelation { get; }
        string ExpandedFormattedRelation { get; }
    }
}