using System.ServiceModel;

namespace Classroom_Learning_Partner
{
    //[ServiceContract(Namespace = "CLPNetworking")]
    //Need to define a namespace, otherwise the default namespace of http://tempuri.org is used
    [ServiceContract]
    public interface INotebookContract
    {
        [OperationContract]
        void AddHistoryItem(string compositePageID, string zippedHistoryItem);

        [OperationContract]
        void AddNewPage(string zippedPage, int index);

        [OperationContract]
        void ReplacePage(string zippedPage, int index);
    }
}