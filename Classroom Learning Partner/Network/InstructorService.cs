using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;
using Classroom_Learning_Partner.Model;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddStudentSubmission(CLPPage page, string userName, string notebookName);

        [OperationContract]
        void AddStudentSubmissionViaString(string sPage, string userName, string notebookName);

        [OperationContract]
        void CollectStudentNotebook(string sNotebook, string studentName);

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

        public void AddStudentSubmissionViaString(string sPage, string userName, string notebookName)
        {
            if(App.Network.DiscoveredProjectors.Addresses.Count() > 0)
            {
                try
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.None;
                    IProjectorContract ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(binding, App.Network.DiscoveredProjectors.Addresses[0]);
                    ProjectorProxy.AddStudentSubmissionViaString(sPage, userName, notebookName);
                    (ProjectorProxy as ICommunicationObject).Close();
                }
                catch(System.Exception ex)
                {

                }
            }
            else
            {
                //TODO: Steve - add pages to a queue and send when a projector is found
                Console.WriteLine("Address NOT Available");
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    CLPPage page = (ObjectSerializer.ToObject(sPage) as CLPPage);

                    foreach(ICLPPageObject pageObject in page.PageObjects)
                    {
                        pageObject.ParentPage = page;
                    }

                    page.IsSubmission = true;
                    page.SubmitterName = userName;

                    try
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            if(page.ParentNotebookID == notebook.UniqueID)
                            {
                                CLPServiceAgent.Instance.AddSubmission(notebook, page);
                                //TODO: Steve - AutoSave Here
                                break;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
        }

        public void CollectStudentNotebook(string sNotebook, string studentName)
        {
            CLPNotebook notebook = ObjectSerializer.ToObject(sNotebook) as CLPNotebook;
            
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\CollectedStudentNotebooks";

            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            DateTime saveTime = DateTime.Now;
            string time = saveTime.Year + "." + saveTime.Month + "." + saveTime.Day;

            string filePathName = filePath + @"\" + time + "-" + studentName + "-" + notebook.NotebookName + @".clp";
            notebook.Save(filePathName);
        }

        public void StudentLogin(Person student)
        {
            App.Network.ClassList.Add(student);
            Logger.Instance.WriteToLog("Student Logged In: " + student.FullName);
        }

        public void StudentLogout(Person student)
        {
            Console.WriteLine("Logout");
        }

        #endregion
    }
}
