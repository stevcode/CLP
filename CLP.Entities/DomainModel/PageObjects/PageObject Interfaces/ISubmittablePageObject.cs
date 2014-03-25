namespace CLP.Entities
{
    public interface ISubmittablePageObject  //TODO: Used for AggregationDataTable, nothing else implements this yet.
    {
        void BeforeSubmit(bool isGroupSubmit);
        void AfterSubmit(bool isGroupSubmit); 
    }
}