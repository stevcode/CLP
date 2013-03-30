using System.Collections.Generic;
using System.ServiceModel;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface INotebookContract
    {
        [OperationContract]
        void ModifyPageInkStrokes(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID);

        [OperationContract]
        void AddNewPage(string s_page, int index);

        [OperationContract]
        void ReplacePage(string s_page, int index);
    }
}
