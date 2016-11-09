using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Media;
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
        string GetClassRosterJson();

        [OperationContract]
        string StudentLogin(string studentName, string studentID, string machineName, string machineAddress, bool useClassPeriod = true);

        [OperationContract]
        void StudentLogout(string studentID);

        [OperationContract]
        string GetStudentNotebookJson(string studentID);

        [OperationContract]
        Dictionary<string, byte[]> GetImages(List<string> imageHashIDs);

        [OperationContract]
        List<string> GetStudentNotebookPagesJson(string studentID);

        [OperationContract]
        List<string> GetStudentPageSubmissionsJson(string studentID);

        [OperationContract]
        string AddStudentSubmission(string submissionJson, string notebookID);

        [OperationContract]
        void AddSerializedPages(string zippedPages, string notebookID);

        [OperationContract]
        void CollectStudentNotebookAndSubmissions(string zippedNotebook, string zippedSubmissions, string studentName);

        [OperationContract]
        void CollectStudentNotebook(string zippedNotebook, string studentName);

        [OperationContract]
        void SendClassPeriod(string machineAddress);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class InstructorService : IInstructorContract
    {
        #region Message Constants

        public const string MESSAGE_NO_DATA_SERVICE = "no data service";
        public const string MESSAGE_NO_NETWORK_SERVICE = "no network service";
        public const string MESSAGE_SUCCESSFUL_STUDENT_LOG_IN = "successful student log in";
        public const string MESSAGE_STUDENT_NOT_IN_ROSTER = "student not in roster";
        public const string MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER = "notebook not loaded by teacher";
        public const string MESSAGE_SUBMISSION_NOT_DESERIALIZED = "submission not deserialized";
        public const string MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK = "page not loaded in notebook";
        public const string MESSAGE_SUBMISSION_SUCCESSFUL = "submission successful";

        #endregion // Message Constants

        #region IInstructorContract Members

        public string GetClassRosterJson()
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return string.Empty;
            }

            var currentClassRoster = dataService.CurrentClassRoster;
            var classRosterJsonString = currentClassRoster.ToJsonString(false);

            return classRosterJsonString;
        }

        public string StudentLogin(string studentName, string studentID, string machineName, string machineAddress, bool useClassPeriod = true)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return MESSAGE_NO_DATA_SERVICE;
            }

            var student = dataService.CurrentClassRoster.ListOfStudents.FirstOrDefault(s => s.ID == studentID);
            if (student == null)
            {
                student = Person.ParseFromFullName(studentName);
                student.ID = studentID;
                dataService.CurrentClassRoster.ListOfStudents.Add(student); // TODO: Handle saving of this, as well as not saving GUEST logins
            }

            if (student.IsConnected)
            {
                try
                {
                    var studentProxy = NetworkService.CreateStudentProxyFromMachineAddress(student.CurrentMachineAddress);
                    studentProxy.OtherAttemptedLogin(machineName);

                    // ReSharper disable once SuspiciousTypeConversion.Global
                    (studentProxy as ICommunicationObject).Close();
                }
                catch (Exception)
                {
                    // ignored
                }

                return student.CurrentMachineAddress;
            }

            student.CurrentMachineAddress = machineAddress;
            student.CurrentMachineName = machineName;
            student.IsConnected = true;

            return MESSAGE_SUCCESSFUL_STUDENT_LOG_IN;

            #region Commented Out

            //var task = Task<string>.Factory.StartNew(() =>
            //                                         {
            //                                             var student = App.MainWindowViewModel.AvailableUsers.FirstOrDefault(x => x.ID == studentID) ?? new Person
            //                                                                                                                                            {
            //                                                                                                                                                ID = studentID,
            //                                                                                                                                                FullName = studentName,
            //                                                                                                                                                IsStudent = true
            //                                                                                                                                            };

            //                                             if (App.MainWindowViewModel.AvailableUsers.All(x => x.ID != studentID))
            //                                             {
            //                                                 App.MainWindowViewModel.AvailableUsers.Add(student);
            //                                             }

            //                                             //if (student.IsConnected)
            //                                             //{
            //                                             //    try
            //                                             //    {
            //                                             //        var binding = new NetTcpBinding
            //                                             //                      {
            //                                             //                          Security =
            //                                             //                          {
            //                                             //                              Mode = SecurityMode.None
            //                                             //                          }
            //                                             //                      };
            //                                             //        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding,
            //                                             //                                                                          new EndpointAddress(student.CurrentMachineAddress));
            //                                             //        studentProxy.ForceLogOut(machineName);
            //                                             //        (studentProxy as ICommunicationObject).Close();
            //                                             //    }
            //                                             //    catch (Exception) { }
            //                                             //}

            //                                             student.CurrentMachineAddress = machineAddress;
            //                                             student.CurrentMachineName = machineName;
            //                                             student.IsConnected = true;
            //                                             return "connected";

            //                                             //var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            //                                             //if (notebookService == null)
            //                                             //{
            //                                             //    return string.Empty;
            //                                             //}

            //                                             //if (!useClassPeriod ||
            //                                             //    notebookService.CurrentClassPeriod == null)
            //                                             //{
            //                                             //    return string.Empty;
            //                                             //}
            //                                             //
            //                                             //try
            //                                             //{
            //                                             //    Notebook notebookToZip;
            //                                             //    var newNotebook = notebookService.OpenNotebooks.First().CopyForNewOwner(student);

            //                                             //    var studentNotebookFolderName = newNotebook.Name + ";" + newNotebook.ID + ";" + newNotebook.Owner.FullName + ";" +
            //                                             //                                    newNotebook.OwnerID;
            //                                             //    var studentNotebookFolderPath = Path.Combine(notebookService.CurrentNotebookCacheDirectory, studentNotebookFolderName);
            //                                             //    if (Directory.Exists(studentNotebookFolderPath))
            //                                             //    {
            //                                             //        var pageIDs = notebookService.CurrentClassPeriod.PageIDs;
            //                                             //        var studentNotebook = Notebook.OpenPartialNotebook(studentNotebookFolderPath, pageIDs, new List<string>());
            //                                             //        if (studentNotebook == null)
            //                                             //        {
            //                                             //            var newNotebookString = ObjectSerializer.ToString(newNotebook);
            //                                             //            var zippedNotebook = CLPServiceAgent.Instance.Zip(newNotebookString);

            //                                             //            return zippedNotebook;
            //                                             //        }
            //                                             //        var loadedPageIDs = studentNotebook.Pages.Select(page => page.ID).ToList();
            //                                             //        foreach (var page in newNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID)))
            //                                             //        {
            //                                             //            studentNotebook.Pages.Add(page);
            //                                             //        }
            //                                             //        var orderedPages = studentNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
            //                                             //        studentNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
            //                                             //        var studentNotebookString = ObjectSerializer.ToString(studentNotebook);
            //                                             //        var zippedStudentNotebook = CLPServiceAgent.Instance.Zip(studentNotebookString);

            //                                             //        return zippedStudentNotebook;
            //                                             //    }

            //                                             //    var newNotebookString2 = ObjectSerializer.ToString(newNotebook);
            //                                             //    var zippedNotebook2 = CLPServiceAgent.Instance.Zip(newNotebookString2);

            //                                             //    return zippedNotebook2;
            //                                             //}
            //                                             //catch (Exception ex)
            //                                             //{
            //                                             //    Logger.Instance.WriteToLog("Error, failed to send partial notebook: " + ex.Message);
            //                                             //    return string.Empty;
            //                                             //}
            //                                         },
            //                                         TaskCreationOptions.LongRunning);

            //return task.Result;

            #endregion // Commented Out
        }

        public void StudentLogout(string studentID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var student = dataService.CurrentClassRoster.ListOfStudents.FirstOrDefault(s => s.ID == studentID);
            if (student == null)
            {
                return;
            }

            student.IsConnected = false;
        }

        public string GetStudentNotebookJson(string studentID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return MESSAGE_NO_DATA_SERVICE;
            }

            var student = dataService.CurrentClassRoster.ListOfStudents.FirstOrDefault(s => s.ID == studentID);
            if (student == null)
            {
                return MESSAGE_STUDENT_NOT_IN_ROSTER;
            }

            var notebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.Owner.ID == studentID);
            if (notebook == null)
            {
                return MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER;
            }

            var notebookJsonString = notebook.ToJsonString(false);

            return notebookJsonString;
        }

        public Dictionary<string, byte[]> GetImages(List<string> imageHashIDs)
        {
            var imageList = new Dictionary<string, byte[]>();
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return imageList;
            }

            var imageConverter = new ImageSourceConverter();
            foreach (var keyValuePair in dataService.ImagePool)
            {
                var imageHashID = keyValuePair.Key;
                var bitmapImage = keyValuePair.Value;

                try
                {
                    var imageByteSource = (byte[])imageConverter.ConvertTo(bitmapImage, typeof(byte[]));
                    imageList.Add(imageHashID, imageByteSource);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return imageList;
        }

        public List<string> GetStudentNotebookPagesJson(string studentID)
        {
            var pageJsonStrings = new List<string>();

            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return pageJsonStrings;
            }

            var student = dataService.CurrentClassRoster.ListOfStudents.FirstOrDefault(s => s.ID == studentID);
            if (student == null)
            {
                return pageJsonStrings;
            }

            var notebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.Owner.ID == studentID);
            if (notebook == null)
            {
                return pageJsonStrings;
            }

            pageJsonStrings.AddRange(notebook.Pages.Select(page => page.ToJsonString(false)));

            return pageJsonStrings;
        }

        public List<string> GetStudentPageSubmissionsJson(string studentID)
        {
            var submissionJsonStrings = new List<string>();

            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return submissionJsonStrings;
            }

            var student = dataService.CurrentClassRoster.ListOfStudents.FirstOrDefault(s => s.ID == studentID);
            if (student == null)
            {
                return submissionJsonStrings;
            }

            var notebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.Owner.ID == studentID);
            if (notebook == null)
            {
                return submissionJsonStrings;
            }

            submissionJsonStrings.AddRange(from page in notebook.Pages
                                           from submission in page.Submissions
                                           select submission.ToJsonString(false));

            return submissionJsonStrings;
        }

        public string AddStudentSubmission(string submissionJson, string notebookID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return MESSAGE_NO_DATA_SERVICE;
            }

            var submission = AEntityBase.FromJsonString<CLPPage>(submissionJson);
            if (submission == null)
            {
                return MESSAGE_SUBMISSION_NOT_DESERIALIZED;
            }

            var studentID = submission.Owner.ID;
            var studentNotebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.ID == notebookID && n.Owner.ID == studentID);
            if (studentNotebook == null)
            {
                return MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER;
            }

            var studentPage = studentNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);
            if (studentPage == null)
            {
                return MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK;
            }

            var teacherNotebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.ID == notebookID && !n.Owner.IsStudent);
            if (teacherNotebook == null)
            {
                return MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER;
            }

            var teacherPage = teacherNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);
            if (teacherPage == null)
            {
                return MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK;
            }

            UIHelper.RunOnUI(() =>
                             {
                                 studentPage.Submissions.Add(submission);
                                 if (teacherPage == null)
                                 {
                                     return;
                                 }

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
                             });

            var networkService = ServiceLocator.Default.ResolveType<INetworkService>();
            if (networkService == null)
            {
                return MESSAGE_NO_NETWORK_SERVICE;
            }

            if (networkService.ProjectorProxy == null)
            {
                return MESSAGE_SUBMISSION_SUCCESSFUL;
            }

            var t = new Thread(() =>
                               {
                                   try
                                   {
                                       networkService.ProjectorProxy.AddStudentSubmission(submissionJson, notebookID);
                                   }
                                   catch (Exception)
                                   {
                                       // ignored
                                   }
                               })
                    {
                        IsBackground = true
                    };
            t.Start();

            return MESSAGE_SUBMISSION_SUCCESSFUL;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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

        public void AddSerializedPages(string zippedPages, string notebookID)
        {
            //Logger.Instance.WriteToLog("received pages");
            //var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            //if (dataService == null)
            //{
            //    return;
            //}

            //var currentNotebook = dataService.CurrentNotebook;

            //if (currentNotebook == null)
            //{
            //    return;
            //}

            //var unZippedPages = zippedPages.DecompressFromGZip();
            //var pages = ObjectSerializer.ToObject(unZippedPages) as List<CLPPage>;

            //if (pages == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to receive student pages. Pages is null");
            //    return;
            //}

            //foreach (var page in pages)
            //{
            //    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            //    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);

            //    var pageNameComposite = PageNameComposite.ParsePage(page);
            //    var notebookNameComposite = NotebookNameComposite.ParseNotebook(currentNotebook);
            //    notebookNameComposite.OwnerID = page.OwnerID;
            //    if (page.Owner == null)
            //    {
            //        return;
            //    }
            //    notebookNameComposite.OwnerName = page.Owner.FullName;
            //    notebookNameComposite.OwnerTypeTag = "S";

            //    var collectionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PartialNotebooks");
            //    if (!Directory.Exists(collectionPath))
            //    {
            //        Directory.CreateDirectory(collectionPath);
            //    }
            //    var notebookPath = Path.Combine(collectionPath, notebookNameComposite.ToFolderName());
            //    if (!Directory.Exists(notebookPath))
            //    {
            //        Directory.CreateDirectory(notebookPath);
            //    }
            //    var pagesPath = Path.Combine(notebookPath, "Pages");
            //    if (!Directory.Exists(pagesPath))
            //    {
            //        Directory.CreateDirectory(pagesPath);
            //    }
            //    var pageFilePath = Path.Combine(pagesPath, pageNameComposite.ToFileName() + ".xml");
            //    page.ToXML(pageFilePath);
            //}
        }

        public void CollectStudentNotebook(string zippedNotebook, string studentName)
        {
            //Task.Factory.StartNew(() =>
            //                      {
            //                          var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            //                          if (dataService == null)
            //                          {
            //                              return;
            //                          }

            //                          var unZippedNotebook = zippedNotebook.DecompressFromGZip();
            //                          var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

            //                          if (notebook == null)
            //                          {
            //                              Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
            //                              return;
            //                          }

            //                          var notebookFolderPath = dataService.CurrentNotebookInfo.NotebookFolderPath;
            //                          notebook.SavePartialNotebook(notebookFolderPath, false);
            //                      });
        }

        public void CollectStudentNotebookAndSubmissions(string zippedNotebook, string zippedSubmissions, string studentName)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            //    if (dataService == null)
            //    {
            //        return;
            //    }

            //    var unZippedNotebook = zippedNotebook.DecompressFromGZip();
            //    var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;

            //    if (notebook == null)
            //    {
            //        Logger.Instance.WriteToLog("Failed to collect notebook from " + studentName);
            //        return;
            //    }

            //    var notebookFolderPath = dataService.CurrentNotebookInfo.NotebookFolderPath;
            //    notebook.SavePartialNotebook(notebookFolderPath, false);
            //});
        }

        #endregion
    }
}