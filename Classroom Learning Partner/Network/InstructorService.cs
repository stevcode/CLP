using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddSerializedSubmission(string zippedPage, string submissionID, 
            DateTime submissionTime, string notebookID, string zippedSubmitter);

        [OperationContract]
        void CollectStudentNotebook(string zippedNotebook, string studentName);

        [OperationContract]
        void StudentLogin(string zippedStudent);

        [OperationContract]
        void StudentLogout(string zippedStudent);
    }

    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public void AddSerializedSubmission(string zippedPage, string submissionID,
            DateTime submissionTime, string notebookID, string zippedSubmitter)
        {
            if(App.Network.ProjectorProxy != null)
            {
                var t = new Thread(() =>
                                   {
                                       try
                                       {
                                           App.Network.ProjectorProxy.AddSerializedSubmission(zippedPage, submissionID, submissionTime, notebookID, zippedSubmitter);
                                       }
                                       catch(Exception ex)
                                       {
                                           Logger.Instance.WriteToLog("Submit to Projector Error: " + ex.Message);
                                       }
                                   })
                        {
                            IsBackground = true
                        };
                t.Start();
            }
            else
            {
                //TODO: Steve - add pages to a queue and send when a projector is found
                Console.WriteLine("Projector NOT Available");
            }

            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage);
            ICLPPage submission = null;
            if(page is CLPPage)
            {
                submission = page as CLPPage;
            }
            else if(page is CLPAnimationPage)
            {
                submission = page as CLPAnimationPage;
            }

            var unZippedSubmitter = CLPServiceAgent.Instance.UnZip(zippedSubmitter);
            var submitter = ObjectSerializer.ToObject(unZippedSubmitter) as Person;

            if(submission == null || submitter == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.SubmissionType = SubmissionType.Single;
            submission.SubmissionID = submissionID;
            submission.SubmissionTime = submissionTime;
            submission.Submitter = submitter;

            ACLPPageBase.Deserialize(submission);

            var currentNotebook = App.MainWindowViewModel.OpenNotebooks.FirstOrDefault(notebook => notebookID == notebook.UniqueID);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            CLPServiceAgent.Instance.AddSubmission(currentNotebook, submission);
                                                                                            //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
                                                                                        }
                                                                                        catch(Exception e)
                                                                                        {
                                                                                            Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " +
                                                                                                                       e.Message);
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void CollectStudentNotebook(string zippedNotebook, string studentName)
        {
            var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
            var notebook = ObjectSerializer.ToObject(unZippedNotebook) as CLPNotebook;

            if(notebook == null)
            {
                Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
                return;
            }
            
            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\CollectedStudentNotebooks";

            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var saveTime = DateTime.Now;
            var time = saveTime.Year + "." + saveTime.Month + "." + saveTime.Day;

            var filePathName = filePath + @"\" + time + "-" + studentName + "-" + notebook.NotebookName + @".clp";
            notebook.Save(filePathName);
        }

        public void StudentLogin(string zippedStudent)
        {
            var unZippedStudent = CLPServiceAgent.Instance.UnZip(zippedStudent);
            var student = ObjectSerializer.ToObject(unZippedStudent) as Person;

            if(student == null)
            {
                Logger.Instance.WriteToLog("Failed to log in student. student is null.");
                return;
            }
            Logger.Instance.WriteToLog("Student Logged In: " + student.FullName);
            App.Network.ClassList.Add(student);
        }

        public void StudentLogout(string zippedStudent)
        {
            var unZippedStudent = CLPServiceAgent.Instance.UnZip(zippedStudent);
            var student = ObjectSerializer.ToObject(unZippedStudent) as Person;

            if(student == null)
            {
                Logger.Instance.WriteToLog("Failed to log out student. student is null.");
                return;
            }
            Logger.Instance.WriteToLog("Student Logged Out: " + student.FullName);
        }

        #endregion
    }
}
