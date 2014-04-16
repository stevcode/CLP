using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CLP.Entities;
using Microsoft.Ink;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddSerializedSubmission(string zippedPage, string notebookID);

        [OperationContract]
        void CollectStudentNotebook(string zippedNotebook, string studentName);

        [OperationContract]
        string StudentLogin(string studentID, string machineName, string machineAddress);

        [OperationContract]
        void StudentLogout(string studentID);

        [OperationContract]
        void SendClassPeriod(string machineAddress);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public void SendClassPeriod(string machineAddress)
        {
            if(App.MainWindowViewModel.CurrentClassPeriod == null)
            {
                Logger.Instance.WriteToLog("Failed to send classperiod, currentclassperiod is null.");
                return;
            }
            try
            {
                var classPeriodString = ObjectSerializer.ToString(App.MainWindowViewModel.CurrentClassPeriod);
                var classPeriod = CLPServiceAgent.Instance.Zip(classPeriodString);

                var classSubjectString = ObjectSerializer.ToString(App.MainWindowViewModel.CurrentClassPeriod.ClassSubject);
                var classsubject = CLPServiceAgent.Instance.Zip(classSubjectString);

                var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(machineAddress));
                studentProxy.OpenClassPeriod(classPeriod, classsubject);
                (studentProxy as ICommunicationObject).Close();
            }
            catch(Exception)
            {
            }
        }

        public void AddSerializedSubmission(string zippedPage, string notebookID)
        {
            if(App.Network.ProjectorProxy != null)
            {
                var t = new Thread(() =>
                                   {
                                       try
                                       {
                                           App.Network.ProjectorProxy.AddSerializedSubmission(zippedPage, notebookID);
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
            var submission = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            if(submission == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);
            var currentNotebook = App.MainWindowViewModel.OpenNotebooks.FirstOrDefault(notebook => notebookID == notebook.ID && notebook.OwnerID == App.MainWindowViewModel.CurrentUser.ID);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            if(currentNotebook != null)
                                                                                            {
                                                                                                var page = currentNotebook.Pages.FirstOrDefault(x => x.ID == submission.ID);
                                                                                                if(page == null)
                                                                                                {
                                                                                                    return null;
                                                                                                }
                                                                                                page.Submissions.Add(submission);
                                                                                            }
                                                                                            //TODO: QuickSave
                                                                                        }
                                                                                        catch(Exception e)
                                                                                        {
                                                                                            Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void CollectStudentNotebook(string zippedNotebook, string studentName)
        {
            var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
            var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

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

            var filePathName = filePath + @"\" + time + "-" + studentName + "-" + notebook.Name + @".clp";
           // TODO: Entities            notebook.Save(filePathName);
        }

        public string StudentLogin(string studentID, string machineName, string machineAddress)
        {
            var student = App.MainWindowViewModel.AvailableUsers.FirstOrDefault(x => x.ID == studentID);
            if(student == null)
            {
                Logger.Instance.WriteToLog("Failed to log in student. student is null.");
                return string.Empty;
            }
            student.CurrentMachineAddress = machineAddress;
            student.CurrentMachineName = machineName;

            try
            {
                var newNotebook = App.MainWindowViewModel.OpenNotebooks.First().CopyForNewOwner(student);
                var newNotebookString = ObjectSerializer.ToString(newNotebook);
                var zippedNotebook = CLPServiceAgent.Instance.Zip(newNotebookString);
                student.IsConnected = true;
                return zippedNotebook;
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("Error, failed to send partial notebook: " + ex.Message);
                return string.Empty;
            }
        }

        public void StudentLogout(string studentID)
        {
            var student = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList.FirstOrDefault(x => x.ID == studentID);
            if(student == null)
            {
                Logger.Instance.WriteToLog("Failed to log out student. student is null.");
                return;
            }

            student.IsConnected = false;
        }

        #endregion
    }
}
