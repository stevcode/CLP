using System.Collections.Generic;
using System.ServiceModel;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface INotebookContract
    {
        [OperationContract]
        void ModifyPageInkStrokes(List<StrokeDTO> strokesAdded, List<StrokeDTO> strokesRemoved, string pageID);

        [OperationContract]
        void AddNewPage(string s_page, int index);

        [OperationContract]
        void ReplacePage(string s_page, int index);
    }
}
