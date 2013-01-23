using System.ServiceModel;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddStudentSubmission(CLPPage page, string userName, string notebookName);
    }

    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public void AddStudentSubmission(CLPPage page, string userName, string notebookName)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
