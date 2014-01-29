using System.Collections.Generic;
using System.ServiceModel;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    //[ServiceContract(Namespace = "CLPNetworking")]
    //Need to define a namespace, otherwise the default namespace of http://tempuri.org is used
    [ServiceContract]
    public interface INotebookContract
    {
        [OperationContract]
        void ModifyPageInkStrokes(List<StrokeDTO> strokesAdded, List<StrokeDTO> strokesRemoved, string pageID);

        [OperationContract]
        void AddHistoryItem(string pageID, string zippedHistoryItem);

        [OperationContract]
        void AddNewPage(string zippedPage, int index);

        [OperationContract]
        void ReplacePage(string zippedPage, int index);
    }
}
