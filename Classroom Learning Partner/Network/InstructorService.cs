using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Catel.IoC;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddSerializedSubmission(string zippedPage, string notebookID);

        [OperationContract]
        void AddSerializedPages(string zippedPages, string notebookID);

        [OperationContract]
        void CollectStudentNotebookAndSubmissions(string zippedNotebook, string zippedSubmissions, string studentName);

        [OperationContract]
        void CollectStudentNotebook(string zippedNotebook, string studentName);

        [OperationContract]
        string StudentLogin(string studentName, string studentID, string machineName, string machineAddress, bool useClassPeriod = true);

        [OperationContract]
        void StudentLogout(string studentID);

        [OperationContract]
        void SendClassPeriod(string machineAddress);

        [OperationContract]
        Dictionary<string, byte[]> SendImages(List<string> imageHashIDs);

        //    [OperationContract]
        //   List<string> SendSubmissions(string ownerID, List<string> pageIDs);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public Dictionary<string, byte[]> SendImages(List<string> imageHashIDs)
        {
            var imageList = new Dictionary<string, byte[]>();
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return imageList;
            }
            
            if (Directory.Exists(dataService.CurrentCacheInfo.ImagesFolderPath))
            {
                var localImageFilePaths = Directory.EnumerateFiles(dataService.CurrentCacheInfo.ImagesFolderPath);
                foreach (var localImageFilePath in localImageFilePaths)
                {
                    var imageHashID = Path.GetFileNameWithoutExtension(localImageFilePath);
                    var fileName = Path.GetFileName(localImageFilePath);
                    if (imageHashIDs.Contains(imageHashID))
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
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            // TODO: reimplement classPeriods
            //if (dataService == null || dataService.CurrentClassPeriod == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to send classperiod, currentclassperiod is null.");
            //    return;
            //}
            //try
            //{
            //    var classPeriodString = ObjectSerializer.ToString(notebookService.CurrentClassPeriod);
            //    var classPeriod = CLPServiceAgent.Instance.Zip(classPeriodString);

            //    var classSubjectString = ObjectSerializer.ToString(notebookService.CurrentClassPeriod.ClassInformation);
            //    var classsubject = CLPServiceAgent.Instance.Zip(classSubjectString);

            //    var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(machineAddress));
            //    studentProxy.OpenClassPeriod(classPeriod, classsubject);
            //    (studentProxy as ICommunicationObject).Close();
            //}
            //catch (Exception) { }
        }

        public void AddSerializedSubmission(string zippedPage, string notebookID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var submission = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            if (submission == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);
            submission.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(submission.History.SerializedTrashedInkStrokes);

            var currentNotebook = dataService.CurrentNotebook;

            if (currentNotebook == null)
            {
                return;
            }

            var submissionNameComposite = PageNameComposite.ParsePage(submission);
            var notebookNameComposite = NotebookNameComposite.ParseNotebook(currentNotebook);
            notebookNameComposite.OwnerID = submission.OwnerID;
            if (submission.Owner == null)
            {
                return;
            }
            notebookNameComposite.OwnerName = submission.Owner.FullName;
            notebookNameComposite.OwnerTypeTag = "S";

            var collectionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PartialNotebooks");
            if (!Directory.Exists(collectionPath))
            {
                Directory.CreateDirectory(collectionPath);
            }
            var notebookPath = Path.Combine(collectionPath, notebookNameComposite.ToFolderName());
            if (!Directory.Exists(notebookPath))
            {
                Directory.CreateDirectory(notebookPath);
            }
            var pagesPath = Path.Combine(notebookPath, "Pages");
            if (!Directory.Exists(pagesPath))
            {
                Directory.CreateDirectory(pagesPath);
            }
            var pageFilePath = Path.Combine(pagesPath, submissionNameComposite.ToFileName() + ".xml");
            submission.ToXML(pageFilePath);

            var studentNotebookInfo = dataService.LoadedNotebooksInfo.FirstOrDefault(ni => ni.Notebook != null && ni.Notebook.OwnerID == submission.OwnerID);
            if (studentNotebookInfo == null ||
                studentNotebookInfo.Notebook == null)
            {
                return;
            }

            var studentNotebook = studentNotebookInfo.Notebook;
            var studentPage = studentNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);
            if (studentPage == null ||
                !studentPage.Owner.IsStudent)
            {
                return;
            }

            var teacherPage = currentNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            studentPage.Submissions.Add(submission);

                                                                                            if (teacherPage != null)
                                                                                            {
                                                                                                var pageViewModels = teacherPage.GetAllViewModels();
                                                                                                foreach (var pageViewModel in pageViewModels)
                                                                                                {
                                                                                                    var pageVM = pageViewModel as ACLPPageBaseViewModel;
                                                                                                    if (pageVM == null)
                                                                                                    {
                                                                                                        continue;
                                                                                                    }
                                                                                                    pageVM.UpdateSubmissionCount();
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        catch (Exception e)
                                                                                        {
                                                                                            Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " +
                                                                                                                       e.Message);
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);

            if (App.Network.ProjectorProxy == null)
            {
                Logger.Instance.WriteToLog("Projector NOT Available for Student Submission");
                return;
            }

            var t = new Thread(() =>
            {
                try
                {
                    App.Network.ProjectorProxy.AddSerializedSubmission(zippedPage, notebookID);
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteToLog("Error Sending Submission: " + ex.Message);
                }
            })
            {
                IsBackground = true
            };
            t.Start();
        }

        public void AddSerializedPages(string zippedPages, string notebookID)
        {
            Logger.Instance.WriteToLog("received pages");
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var currentNotebook = dataService.CurrentNotebook;

            if (currentNotebook == null)
            {
                return;
            }

            var unZippedPages = CLPServiceAgent.Instance.UnZip(zippedPages);
            var pages = ObjectSerializer.ToObject(unZippedPages) as List<CLPPage>;

            if (pages == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student pages. Pages is null");
                return;
            }

            foreach (var page in pages)
            {
                page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);

                var pageNameComposite = PageNameComposite.ParsePage(page);
                var notebookNameComposite = NotebookNameComposite.ParseNotebook(currentNotebook);
                notebookNameComposite.OwnerID = page.OwnerID;
                if (page.Owner == null)
                {
                    return;
                }
                notebookNameComposite.OwnerName = page.Owner.FullName;
                notebookNameComposite.OwnerTypeTag = "S";

                var collectionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PartialNotebooks");
                if (!Directory.Exists(collectionPath))
                {
                    Directory.CreateDirectory(collectionPath);
                }
                var notebookPath = Path.Combine(collectionPath, notebookNameComposite.ToFolderName());
                if (!Directory.Exists(notebookPath))
                {
                    Directory.CreateDirectory(notebookPath);
                }
                var pagesPath = Path.Combine(notebookPath, "Pages");
                if (!Directory.Exists(pagesPath))
                {
                    Directory.CreateDirectory(pagesPath);
                }
                var pageFilePath = Path.Combine(pagesPath, pageNameComposite.ToFileName() + ".xml");
                page.ToXML(pageFilePath);
            }
        }

        public void CollectStudentNotebook(string zippedNotebook, string studentName)
        {
            Task.Factory.StartNew(() =>
                                  {
                                      var dataService = ServiceLocator.Default.ResolveType<IDataService>();
                                      if (dataService == null)
                                      {
                                          return;
                                      }

                                      var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                                      var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

                                      if (notebook == null)
                                      {
                                          Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
                                          return;
                                      }

                                      var notebookFolderPath = dataService.CurrentNotebookInfo.NotebookFolderPath;
                                      notebook.SavePartialNotebook(notebookFolderPath, false);
                                  });
        }

        public void CollectStudentNotebookAndSubmissions(string zippedNotebook, string zippedSubmissions, string studentName)
        {
            Task.Factory.StartNew(() =>
            {
                var dataService = ServiceLocator.Default.ResolveType<IDataService>();
                if (dataService == null)
                {
                    return;
                }

                var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

                if (notebook == null)
                {
                    Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
                    return;
                }

                var notebookFolderPath = dataService.CurrentNotebookInfo.NotebookFolderPath;
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

                                                         if (App.MainWindowViewModel.AvailableUsers.All(x => x.ID != studentID))
                                                         {
                                                             App.MainWindowViewModel.AvailableUsers.Add(student);
                                                         }

                                                         //if (student.IsConnected)
                                                         //{
                                                         //    try
                                                         //    {
                                                         //        var binding = new NetTcpBinding
                                                         //                      {
                                                         //                          Security =
                                                         //                          {
                                                         //                              Mode = SecurityMode.None
                                                         //                          }
                                                         //                      };
                                                         //        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding,
                                                         //                                                                          new EndpointAddress(student.CurrentMachineAddress));
                                                         //        studentProxy.ForceLogOut(machineName);
                                                         //        (studentProxy as ICommunicationObject).Close();
                                                         //    }
                                                         //    catch (Exception) { }
                                                         //}

                                                         student.CurrentMachineAddress = machineAddress;
                                                         student.CurrentMachineName = machineName;
                                                         student.IsConnected = true;
                                                         return "connected";

                                                         //var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                         //if (notebookService == null)
                                                         //{
                                                         //    return string.Empty;
                                                         //}

                                                         //if (!useClassPeriod ||
                                                         //    notebookService.CurrentClassPeriod == null)
                                                         //{
                                                         //    return string.Empty;
                                                         //}
                                                         //
                                                         //try
                                                         //{
                                                         //    Notebook notebookToZip;
                                                         //    var newNotebook = notebookService.OpenNotebooks.First().CopyForNewOwner(student);

                                                         //    var studentNotebookFolderName = newNotebook.Name + ";" + newNotebook.ID + ";" + newNotebook.Owner.FullName + ";" +
                                                         //                                    newNotebook.OwnerID;
                                                         //    var studentNotebookFolderPath = Path.Combine(notebookService.CurrentNotebookCacheDirectory, studentNotebookFolderName);
                                                         //    if (Directory.Exists(studentNotebookFolderPath))
                                                         //    {
                                                         //        var pageIDs = notebookService.CurrentClassPeriod.PageIDs;
                                                         //        var studentNotebook = Notebook.OpenPartialNotebook(studentNotebookFolderPath, pageIDs, new List<string>());
                                                         //        if (studentNotebook == null)
                                                         //        {
                                                         //            var newNotebookString = ObjectSerializer.ToString(newNotebook);
                                                         //            var zippedNotebook = CLPServiceAgent.Instance.Zip(newNotebookString);

                                                         //            return zippedNotebook;
                                                         //        }
                                                         //        var loadedPageIDs = studentNotebook.Pages.Select(page => page.ID).ToList();
                                                         //        foreach (var page in newNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID)))
                                                         //        {
                                                         //            studentNotebook.Pages.Add(page);
                                                         //        }
                                                         //        var orderedPages = studentNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
                                                         //        studentNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
                                                         //        var studentNotebookString = ObjectSerializer.ToString(studentNotebook);
                                                         //        var zippedStudentNotebook = CLPServiceAgent.Instance.Zip(studentNotebookString);

                                                         //        return zippedStudentNotebook;
                                                         //    }

                                                         //    var newNotebookString2 = ObjectSerializer.ToString(newNotebook);
                                                         //    var zippedNotebook2 = CLPServiceAgent.Instance.Zip(newNotebookString2);

                                                         //    return zippedNotebook2;
                                                         //}
                                                         //catch (Exception ex)
                                                         //{
                                                         //    Logger.Instance.WriteToLog("Error, failed to send partial notebook: " + ex.Message);
                                                         //    return string.Empty;
                                                         //}
                                                     },
                                                     TaskCreationOptions.LongRunning);

            return task.Result;
        }

        public void StudentLogout(string studentID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            // TODO: reimplement classPeriod
            //var student = dataService.CurrentClassPeriod.ClassInformation.StudentList.FirstOrDefault(x => x.ID == studentID);
            //if (student == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to log out student. student is null.");
            //    return;
            //}

            //student.IsConnected = false;
        }

        #endregion
    }
}