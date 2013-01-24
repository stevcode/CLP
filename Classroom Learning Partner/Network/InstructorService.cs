using System;
using System.ServiceModel;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddStudentSubmission(CLPPage page, string userName, string notebookName);

        [OperationContract]
        void CollectStudentNotebook(CLPNotebook notebook);

        [OperationContract]
        void StudentLogin(Person student);

        [OperationContract]
        void StudentLogout(Person student);
    }

    public class InstructorService : IInstructorContract
    {
        public InstructorService() { }

        #region IInstructorContract Members

        public void AddStudentSubmission(CLPPage page, string userName, string notebookName)
        {
            Console.WriteLine("Submission Added");
        }

        public void CollectStudentNotebook(CLPNotebook notebook)
        {
            Console.WriteLine("Notebook Collected");
        }

        public void StudentLogin(Person student)
        {
            Console.WriteLine("Login");
        }

        public void StudentLogout(Person student)
        {
            Console.WriteLine("Logout");
        }

        #endregion
    }
}
