namespace CLP.Entities.Old
{
    public interface ISubmittablePageObject : IPageObject  //TODO: Used for AggregationDataTable, nothing else implements this yet.
    {
        void BeforeSubmit(bool isGroupSubmit);
        void AfterSubmit(bool isGroupSubmit); 
    }
}