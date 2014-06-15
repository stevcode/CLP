using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
        string StudentLogin(string studentName, string studentID, string machineName, string machineAddress, bool useClassPeriod = true);

        [OperationContract]
        void StudentLogout(string studentID);

        [OperationContract]
        void SendClassPeriod(string machineAddress);

        [OperationContract]
        Dictionary<string,byte[]> SendImages(List<string> imageHashIDs);

     //    [OperationContract]
     //   List<string> SendSubmissions(string ownerID, List<string> pageIDs);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public Dictionary<string,byte[]> SendImages(List<string> imageHashIDs)
        {
            var imageList = new Dictionary<string,byte[]>();
            if(Directory.Exists(App.ImageCacheDirectory))
            {
                var localImageFilePaths = Directory.EnumerateFiles(App.ImageCacheDirectory);
                foreach(var localImageFilePath in localImageFilePaths)
                {
                    var imageHashID = Path.GetFileNameWithoutExtension(localImageFilePath);
                    var fileName = Path.GetFileName(localImageFilePath);
                    if(imageHashIDs.Contains(imageHashID))
                    {
                        var byteSource = File.ReadAllBytes(localImageFilePath);
                        imageList.Add(fileName, byteSource);
                    }
                }
            }

            return imageList;
        }

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
            submission.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(submission.History.SerializedTrashedInkStrokes);
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
            Task.Factory.StartNew(() =>
                                  {
                                      var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                                      var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

                                      if(notebook == null)
                                      {
                                          Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
                                          return;
                                      }

                                      var notebookFolderName = notebook.Name + ";" + notebook.ID + ";" + notebook.Owner.FullName + ";" + notebook.OwnerID;
                                      var notebookFolderPath = Path.Combine(App.NotebookCacheDirectory, notebookFolderName);
                                      notebook.SavePartialNotebook(notebookFolderPath, false);
                                  });
        }

        public string StudentLogin(string studentName, string studentID, string machineName, string machineAddress, bool useClassPeriod = true)
        {
            var task = Task<string>.Factory.StartNew(() =>
            {
                var student = App.MainWindowViewModel.AvailableUsers.FirstOrDefault(x => x.ID == studentID) ?? new Person
                                                                                                           {
                                                                                                               ID = studentID,
                                                                                                               FullName = studentName,
                                                                                                               IsStudent = true
                                                                                                           };
                if(student.IsConnected)
                {
                    try
                    {
                        var binding = new NetTcpBinding
                                        {
                                            Security = {
                                                            Mode = SecurityMode.None
                                                        }
                                        };
                        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                        studentProxy.ForceLogOut(machineName);
                        (studentProxy as ICommunicationObject).Close();
                    }
                    catch(Exception)
                    {
                    }
                }

                student.CurrentMachineAddress = machineAddress;
                student.CurrentMachineName = machineName;
                student.IsConnected = true;

                if(!useClassPeriod ||
                    App.MainWindowViewModel.CurrentClassPeriod == null)
                {
                    return string.Empty;
                }

                try
                {
                    Notebook notebookToZip;
                    var newNotebook = App.MainWindowViewModel.OpenNotebooks.First().CopyForNewOwner(student);

                    var studentNotebookFolderName = newNotebook.Name + ";" + newNotebook.ID + ";" + newNotebook.Owner.FullName + ";" + newNotebook.OwnerID;
                    var studentNotebookFolderPath = Path.Combine(App.NotebookCacheDirectory, studentNotebookFolderName);
                    if(Directory.Exists(studentNotebookFolderPath))
                    {
                        var pageIDs = App.MainWindowViewModel.CurrentClassPeriod.PageIDs;
                        var studentNotebook = Notebook.OpenPartialNotebook(studentNotebookFolderPath, pageIDs, new List<string>());
                        if(studentNotebook == null)
                        {
                            var newNotebookString = ObjectSerializer.ToString(newNotebook);
                            var zippedNotebook = CLPServiceAgent.Instance.Zip(newNotebookString);

                            return zippedNotebook;
                        }
                        var loadedPageIDs = studentNotebook.Pages.Select(page => page.ID).ToList();
                        foreach(var page in newNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID))) 
                        {
                            studentNotebook.Pages.Add(page);
                        }
                        var orderedPages = studentNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
                        studentNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
                        var studentNotebookString = ObjectSerializer.ToString(studentNotebook);
                        var zippedStudentNotebook = CLPServiceAgent.Instance.Zip(studentNotebookString);

                        return zippedStudentNotebook;
                    }

                    var newNotebookString2 = ObjectSerializer.ToString(newNotebook);
                    var zippedNotebook2 = CLPServiceAgent.Instance.Zip(newNotebookString2);

                    return zippedNotebook2;
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteToLog("Error, failed to send partial notebook: " + ex.Message);
                    return string.Empty;
                }
            }, TaskCreationOptions.LongRunning);

            return task.Result;
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
