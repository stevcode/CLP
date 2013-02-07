using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        void AddStudentSubmission(ObservableCollection<List<byte>> byteStrokes, 
            ObservableCollection<ICLPPageObject> pageObjects, 
            string userName, 
            string notebookID, 
            string pageID, string submissionID, DateTime submissionTime);

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

        public void AddStudentSubmission(ObservableCollection<List<byte>> byteStrokes, ObservableCollection<ICLPPageObject> pageObjects, string userName, string notebookID, string pageID, string submissionID, DateTime submissionTime)
        {
            if(App.Network.ProjectorProxy != null)
            {
                Thread t = new Thread(() =>
                {
                    try
                    {
                        //App.Network.ProjectorProxy.AddStudentSubmission(page, userName, notebookName);
                    }
                    catch(System.Exception ex)
                    {
                        Logger.Instance.WriteToLog("Submit to Projector Error: " + ex.Message);
                    }
                });
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                //TODO: Steve - add pages to a queue and send when a projector is found
                Console.WriteLine("Projector NOT Available");
            }

            CLPPage submission = null;

            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                if(notebookID == notebook.UniqueID)
                {
                    submission = notebook.GetNotebookPageByID(pageID).Clone() as CLPPage;
                    break;
                }
            }

            if(submission != null)
            {
                foreach(ICLPPageObject pageObject in submission.PageObjects)
                {
                    pageObject.ParentPage = submission;
                }

                submission.ByteStrokes = byteStrokes;
                submission.InkStrokes = CLPPage.BytesToStrokes(byteStrokes);

                foreach(ICLPPageObject pageObject in pageObjects)
                {
                    submission.PageObjects.Add(pageObject);
                }

                submission.IsSubmission = true;
                submission.SubmissionID = submissionID;
                submission.SubmissionTime = submissionTime;
                submission.SubmitterName = userName;

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    try
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            if(submission.ParentNotebookID == notebook.UniqueID)
                            {
                                CLPServiceAgent.Instance.AddSubmission(notebook, submission);
                                //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
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

            

            //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
        }

        public void AddStudentSubmissionViaString(string sPage, string userName, string notebookName)
        {
            if(App.Network.ProjectorProxy != null)
            {
                Thread t = new Thread(() =>
                    {
                        try
                        {
                            App.Network.ProjectorProxy.AddStudentSubmissionViaString(sPage, userName, notebookName);
                        }
                        catch(System.Exception ex)
                        {
                            Logger.Instance.WriteToLog("Submit to Projector Error: " + ex.Message);
                        }
                    });
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                //TODO: Steve - add pages to a queue and send when a projector is found
                Console.WriteLine("Projector NOT Available");
            }

            CLPPage page = (ObjectSerializer.ToObject(sPage) as CLPPage);

            foreach(ICLPPageObject pageObject in page.PageObjects)
            {
                pageObject.ParentPage = page;
            }

            page.IsSubmission = true;
            page.SubmitterName = userName;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    try
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            if(page.ParentNotebookID == notebook.UniqueID)
                            {
                                CLPServiceAgent.Instance.AddSubmission(notebook, page);
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

            CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
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
