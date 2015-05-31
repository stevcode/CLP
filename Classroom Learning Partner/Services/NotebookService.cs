using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class NotebookService : INotebookService
    {
        public NotebookService()
        {
            //Warm up Serializer to make loading of notebook faster.
            var typesToWarmup = new[] { typeof (Notebook), typeof (ClassPeriod) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);
        }

        #region Properties

        #region Cache

        public List<string> AvailableLocalCacheNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                return directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => directory.Name).OrderBy(x => x).ToList();
            }
        }

        public string CurrentLocalCacheDirectory { get; set; }

        public string CurrentClassCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Classes");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string CurrentImageCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string CurrentNotebookCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Notebooks");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        #endregion //Cache

        #region ClassPeriod

        public List<ClassPeriodNameComposite> AvailableLocalClassPeriodNameComposites
        {
            get { return CurrentLocalCacheDirectory == null ? new List<ClassPeriodNameComposite>() : GetAvailableClassPeriodNameCompositesInCache(CurrentLocalCacheDirectory); }
        }

        public ClassPeriod CurrentClassPeriod { get; set; }

        #endregion //ClassPeriod

        #region Notebook

        public List<NotebookNameComposite> AvailableLocalNotebookNameComposites
        {
            get { return CurrentLocalCacheDirectory == null ? new List<NotebookNameComposite>() : GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory); }
        }

        private readonly List<Notebook> _openNotebooks = new List<Notebook>();

        public List<Notebook> OpenNotebooks
        {
            get { return _openNotebooks; }
        }

        public Notebook CurrentNotebook { get; set; }

        public Notebook CurrentNotebooksAuthoredSource
        {
            get { return OpenNotebooks.FirstOrDefault(x => x.ID == CurrentNotebook.ID && x.OwnerID == Person.Author.ID); }
        }

        #endregion //Notebook

        #endregion //Properties

        #region Cache Methods

        public bool InitializeNewLocalCache(string cacheName) { return InitializeNewLocalCache(cacheName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop)); }

        public bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath)
        {
            cacheName = "Cache" + cacheName;
            var directoryInfo = new DirectoryInfo(cacheDirectoryPath);
            var availableCacheNames =
                directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => directory.Name).OrderBy(x => x).ToList();

            if (availableCacheNames.Contains(cacheName))
            {
                return false;
            }

            CurrentLocalCacheDirectory = Path.Combine(cacheDirectoryPath, cacheName);
            if (!Directory.Exists(CurrentLocalCacheDirectory))
            {
                Directory.CreateDirectory(CurrentLocalCacheDirectory);
            }
            var initializeClassDirectory = CurrentClassCacheDirectory;
            var initializeImageDirectory = CurrentImageCacheDirectory;
            var initializeNotebookDirectory = CurrentNotebookCacheDirectory;

            return true;
        }

        public void ArchiveNotebookCache(string notebookCacheDirectory)
        {
            if (!Directory.Exists(notebookCacheDirectory))
            {
                return;
            }

            var archiveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ArchivedCaches");
            var now = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            var newCacheDirectory = Path.Combine(archiveDirectory, "Cache-" + now);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            Directory.Move(notebookCacheDirectory, newCacheDirectory);
        }

        #endregion //Cache Methods

        #region Methods

        #region Class Period

        public void StartSoonestClassPeriod(string localCacheFolderPath)
        {
            var classesFolderPath = Path.Combine(localCacheFolderPath, "Classes");
            var classPeriodFilePaths = Directory.GetFiles(classesFolderPath);
            ClassPeriodNameComposite closestClassPeriodNameComposite = null;
            var closestTimeSpan = TimeSpan.MaxValue;
            var now = DateTime.Now;
            foreach (var classPeriodFilePath in classPeriodFilePaths)
            {
                var classPeriodNameComposite = ClassPeriodNameComposite.ParseFilePathToNameComposite(classPeriodFilePath);
                if (classPeriodNameComposite == null)
                {
                    continue;
                }
                var time = classPeriodNameComposite.StartTime;
                var timeParts = time.Split('.');
                var year = Int32.Parse(timeParts[0]);
                var month = Int32.Parse(timeParts[1]);
                var day = Int32.Parse(timeParts[2]);
                var hour = Int32.Parse(timeParts[3]);
                var minute = Int32.Parse(timeParts[4]);
                var dateTime = new DateTime(year, month, day, hour, minute, 0);

                var timeSpan = now - dateTime;
                var duration = timeSpan.Duration();
                var closestTimeSpanDuration = closestTimeSpan.Duration();
                if (duration >= closestTimeSpanDuration)
                {
                    continue;
                }
                closestTimeSpan = timeSpan;
                closestClassPeriodNameComposite = classPeriodNameComposite;
            }

            if (closestClassPeriodNameComposite == null)
            {
                MessageBox.Show("ERROR: Could not find ClassPeriod .xml file.");
                return;
            }

            StartLocalClassPeriod(closestClassPeriodNameComposite, localCacheFolderPath);
        }

        public void StartLocalClassPeriod(ClassPeriodNameComposite classPeriodNameComposite, string localCacheFolderPath)
        {
            var filePath = classPeriodNameComposite.FullClassPeriodFilePath;
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Class Period doesn't exist!");
                return;
            }

            var classPeriod = ClassPeriod.LoadLocalClassPeriod(filePath);
            if (classPeriod == null)
            {
                MessageBox.Show("Class Period could not be opened. Check error log.");
                return;
            }

            CurrentLocalCacheDirectory = localCacheFolderPath;
            CurrentClassPeriod = classPeriod;
            var authoredNotebook = LoadClassPeriodNotebookForPerson(classPeriod, Person.Author.ID);
            if (authoredNotebook == null)
            {
                return;
            }
            //var authoredNotebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == Person.Author.ID);
            //var authoredPagesFolderPath = Path.Combine(authoredNotebookNameComposite.NotebookFolderPath, "Pages");
            //var pageIDs = GetPageIDsFromStartIDAndForwardRange(authoredPagesFolderPath, classPeriod.StartPageID, classPeriod.NumberOfPages);
            //if (!pageIDs.Contains(classPeriod.TitlePageID))
            //{
            //    pageIDs.Insert(0, classPeriod.TitlePageID);
            //}

            //var authoredPages = LoadOrCopyPagesForNotebook(authoredNotebook, null, pageIDs, false);
            //authoredNotebook.Pages = new ObservableCollection<CLPPage>(authoredPages);
            //authoredNotebook.CurrentPage = authoredNotebook.Pages.FirstOrDefault(); //HACK

            //var teacherNotebook = LoadClassPeriodNotebookForPerson(classPeriod, classPeriod.ClassSubject.TeacherID) ??
            //                      CopyNotebookForNewOwner(authoredNotebook, classPeriod.ClassSubject.Teacher);

            //var teacherPages = LoadOrCopyPagesForNotebook(teacherNotebook, authoredNotebook, pageIDs, true);
            //teacherNotebook.Pages = new ObservableCollection<CLPPage>(teacherPages);
            //teacherNotebook.CurrentPage = teacherNotebook.Pages.FirstOrDefault(); //HACK

            ////Generates pages in cache
            //foreach (var student in classPeriod.ClassSubject.StudentList)
            //{
            //    var studentNotebook = LoadClassPeriodNotebookForPerson(classPeriod, student.ID) ??
            //                      CopyNotebookForNewOwner(authoredNotebook, student);

            //    var studentPages = LoadOrCopyPagesForNotebook(studentNotebook, authoredNotebook, pageIDs, false);
            //    studentNotebook.Pages = new ObservableCollection<CLPPage>(studentPages);
            //    studentNotebook.CurrentPage = studentNotebook.Pages.FirstOrDefault(); //HACK
            //    SaveNotebookLocally(studentNotebook);
            //}

            //OpenNotebooks.Clear();
            //OpenNotebooks.Add(authoredNotebook);
            //OpenNotebooks.Add(teacherNotebook);
            //SetNotebookAsCurrentNotebook(teacherNotebook);
        }

        public Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID)
        {
            var notebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == ownerID);
            if (notebookNameComposite == null)
            {
               // MessageBox.Show("Notebook for Class Period not found for " + ownerID + ".");
                return null;
            }

            Notebook notebook = null; // = Notebook.LoadLocalNotebook(notebookNameComposite.NotebookFolderPath);
            if (notebook == null)
            {
                //MessageBox.Show("Notebook for Class Period could not be loaded " + ownerID + ".");
                return null;
            }

            return notebook;
        }

        public Notebook CopyNotebookForNewOwner(Notebook originalNotebook, Person newOwner)
        {
            var newNotebook = originalNotebook.Clone() as Notebook;
            if (newNotebook == null)
            {
                return null;
            }
            newNotebook.Owner = newOwner;
            newNotebook.CreationDate = DateTime.Now;
            newNotebook.LastSavedDate = null;

            return newNotebook;
        }

        public CLPPage CopyPageForNewOwner(CLPPage originalPage, Person newOwner)
        {
            var newPage = originalPage.Clone() as CLPPage;
            if (newPage == null)
            {
                return null;
            }
            newPage.Owner = newOwner;
            newPage.CreationDate = DateTime.Now;

            foreach (var pageObject in newPage.PageObjects)
            {
                pageObject.ParentPage = newPage;
                if (pageObject.IsBackgroundInteractable)
                {
                    pageObject.OwnerID = newOwner.ID;
                }
            }

            foreach (var tag in newPage.Tags)
            {
                tag.ParentPage = newPage;
            }

            newPage.AfterDeserialization();

            return newPage;
        }

        public List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, Notebook authoredNotebook, List<string> pageIDs, bool includeSubmissions)
        {
            var pages = new List<CLPPage>();

            var notebookNameComposite =
                GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory).FirstOrDefault(x => x.ID == notebook.ID && x.OwnerID == notebook.Owner.ID);

            var pageNameComposites = new List<PageNameComposite>();
            if (notebookNameComposite != null)
            {
                //var pagesFolderPath = Path.Combine(notebookNameComposite.NotebookFolderPath, "Pages");
                //pageNameComposites = Directory.EnumerateFiles(pagesFolderPath, "*.xml").Select(PageNameComposite.ParseFilePath).Where(x => pageIDs.Contains(x.ID)).ToList();
            }

            //foreach (var pageID in pageIDs)
            //{
            //    var pageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == "0");
            //    if (pageNameComposite == null)
            //    {
            //        if (authoredNotebook == null)
            //        {
            //            continue;
            //        }
            //        var authoredPage = authoredNotebook.Pages.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == 0);
            //        if (authoredPage == null)
            //        {
            //            continue;
            //        }

            //        var newPage = CopyPageForNewOwner(authoredPage, notebook.Owner);
            //        if (newPage == null)
            //        {
            //            continue;
            //        }

            //        pages.Add(newPage);
            //        continue;
            //    }

            //    var page = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
            //    if (page == null)
            //    {
            //        continue;
            //    }

            //    if (includeSubmissions)
            //    {
            //        var id = pageID;
            //        foreach (var submissionComposite in pageNameComposites.Where(x => x.ID == id && x.VersionIndex != "0"))
            //        {
            //            var submission = CLPPage.LoadLocalPage(submissionComposite.FullPageFilePath);
            //            if (submission == null)
            //            {
            //                continue;
            //            }
            //            page.Submissions.Add(submission);
            //        }
            //    }

            //    pages.Add(page);
            //}

            return pages;
        }

        #endregion //Class Period

        #region Notebook

        public void OpenLocalNotebook(NotebookNameComposite notebookNameComposite, string localCacheFolderPath, bool includeSubmissions, List<int> pageNumbers = null)
        {
            //Notebook is already loaded in memory
            var existingNotebook = OpenNotebooks.FirstOrDefault(x => x.ID == notebookNameComposite.ID && x.OwnerID == notebookNameComposite.OwnerID);
            if (existingNotebook != null)
            {
                if (CurrentNotebook == existingNotebook)
                {
                    return;
                }
                SetNotebookAsCurrentNotebook(existingNotebook);
                return;
            }

            //Open New Notebook from disk.
            //var folderPath = notebookNameComposite.NotebookFolderPath;
            //if (!Directory.Exists(folderPath))
            //{
            //    MessageBox.Show("Notebook doesn't exist");
            //    return;
            //}

            //var notebook = pageNumbers == null ? Notebook.LoadLocalFullNotebook(folderPath) : Notebook.LoadLocalPartialNotebook(folderPath, pageNumbers);
            
            //if (notebook == null)
            //{
            //    MessageBox.Show("Notebook could not be opened. Check error log.");
            //    return;
            //}

            //if ((App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher ||
            //     App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector) &&
            //    notebook.Owner.ID != Person.Author.ID &&
            //    !notebook.Owner.IsStudent &&
            //    includeSubmissions)
            //{
            //    foreach (var page in notebook.Pages)
            //    {
            //        var notebookNameComposites = GetAvailableNotebookNameCompositesInCache(localCacheFolderPath);
            //        foreach (var nameComposite in notebookNameComposites.Where(x => x.ID == notebook.ID && x.OwnerTypeTag == "S"))
            //        {
            //            var pageFolderPath = Path.Combine(nameComposite.NotebookFolderPath, "Pages");
            //            var pageNameComposites = GetAvailablePagesNameCompositesInFolder(pageFolderPath).Where(x => x.ID == page.ID && x.VersionIndex != "0").ToList();
            //            foreach (var pageNameComposite in pageNameComposites)
            //            {
            //                var submission = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
            //                if (submission != null)
            //                {
            //                    page.Submissions.Add(submission);
            //                }
            //            }
            //        }
            //    }
            //}

            //CurrentLocalCacheDirectory = localCacheFolderPath;
            //OpenNotebooks.Add(notebook);
            //SetNotebookAsCurrentNotebook(notebook);
        }

        public void SetNotebookAsCurrentNotebook(Notebook notebook, bool logInAsNotebookOwner = true)
        {
            CurrentNotebook = notebook;
            App.MainWindowViewModel.CurrentNotebookName = CurrentNotebook.Name;
            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
            if (logInAsNotebookOwner)
            {
                App.MainWindowViewModel.CurrentUser = notebook.Owner;
            }
            if (notebook.OwnerID == Person.Author.ID)
            {
                App.MainWindowViewModel.IsAuthoring = true;
            }
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        public void SaveCurrentNotebookLocally() { SaveNotebookLocally(CurrentNotebook); }

        public void SaveNotebookLocally(Notebook notebook, string alternativeLocation = null)
        {
            var savePath = alternativeLocation ?? CurrentNotebookCacheDirectory;
            var folderPath = Path.Combine(savePath, NotebookNameComposite.ParseNotebook(notebook).ToFolderName());
            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if (App.MainWindowViewModel.CurrentUser.ID == Person.Author.ID)
            {
                if (Directory.Exists(pagesFolderPath))
                {
                    var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml").ToList();
                    foreach (var pageFilePath in pageFilePaths)
                    {
                        File.Delete(pageFilePath);
                    }
                }
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            notebook.SaveNotebook(folderPath, true);
            notebook.SaveSubmissions(pagesFolderPath);

            //switch (App.MainWindowViewModel.CurrentProgramMode)
            //{
            //    case ProgramModes.Author:
            //    case ProgramModes.Database:
            //        break;
            //    case ProgramModes.Teacher:
            //     //   notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
            //        break;
            //    case ProgramModes.Projector:
            //      //  notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
            //        break;
            //    case ProgramModes.Student:
            //        var submissionsPath = Path.Combine(folderPath, "Pages");
            //        notebook.SaveSubmissions(submissionsPath);
            //        if (App.Network.InstructorProxy != null)
            //        {
            //            var sNotebook = ObjectSerializer.ToString(notebook);
            //            var zippedNotebook = CLPServiceAgent.Instance.Zip(sNotebook);
            //            App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
            //        }
            //        break;
            //}
        }

        #endregion //Notebook

        #endregion //Methods

        #region Static Notebook Methods

        public static List<ClassPeriodNameComposite> GetAvailableClassPeriodNameCompositesInCache(string cachePath)
        {
            var classesCacheDirectory = Path.Combine(cachePath, "Classes");
            if (!Directory.Exists(classesCacheDirectory))
            {
                Directory.CreateDirectory(classesCacheDirectory);
            }
            var directoryInfo = new DirectoryInfo(classesCacheDirectory);
            return
                directoryInfo.GetFiles()
                             .Select(file => ClassPeriodNameComposite.ParseFilePathToNameComposite(file.FullName))
                             .Where(x => x != null)
                             .OrderByDescending(x => x.StartTime)
                             .ToList();
        }

        public static List<NotebookNameComposite> GetAvailableNotebookNameCompositesInCache(string cachePath)
        {
            var notebookCacheDirectory = Path.Combine(cachePath, "Notebooks");
            if (!Directory.Exists(notebookCacheDirectory))
            {
                Directory.CreateDirectory(notebookCacheDirectory);
            }
            var directoryInfo = new DirectoryInfo(notebookCacheDirectory);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => NotebookNameComposite.ParseFolderPath(directory.FullName))
                             .Where(x => x != null)
                             .OrderBy(x => x.OwnerTypeTag != "T")
                             .ThenBy(x => x.OwnerTypeTag != "A")
                             .ThenBy(x => x.OwnerTypeTag != "S")
                             .ThenBy(x => x.OwnerName)
                             .ToList();
        }

        public static List<PageNameComposite> GetAvailablePagesNameCompositesInFolder(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            return
                directoryInfo.GetFiles()
                             .Select(file => PageNameComposite.ParseFilePath(file.FullName))
                             .Where(x => x != null)
                             .OrderBy(x => UInt32.Parse(x.PageNumber))
                             .ToList();
        }

        public static List<string> GetPageIDsFromStartIDAndForwardRange(string pagesFolderPath, string startID, uint range)
        {
            var pageIDs = new List<string>();
            var pageNameComposites = GetAvailablePagesNameCompositesInFolder(pagesFolderPath);
            var startPageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == startID);
            if (startPageNameComposite == null)
            {
                return pageIDs;
            }
            var startPageNumber = UInt32.Parse(startPageNameComposite.PageNumber);
            var endPageNumber = startPageNumber + range - 1;
            pageIDs = pageNameComposites.Where(x => UInt32.Parse(x.PageNumber) >= startPageNumber && UInt32.Parse(x.PageNumber) <= endPageNumber).Select(x => x.ID).ToList();

            return pageIDs;
        }

        public static List<string> GetPageIDsFromARangeBeforeStartID(string pagesFolderPath, string startID, uint range)
        {
            var pageIDs = new List<string>();
            var pageNameComposites = GetAvailablePagesNameCompositesInFolder(pagesFolderPath);
            var startPageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == startID);
            if (startPageNameComposite == null)
            {
                return pageIDs;
            }
            var startPageNumber = UInt32.Parse(startPageNameComposite.PageNumber);
            var endPageNumber = startPageNumber - range;
            pageIDs = pageNameComposites.Where(x => UInt32.Parse(x.PageNumber) < startPageNumber && UInt32.Parse(x.PageNumber) >= endPageNumber).Select(x => x.ID).ToList();

            return pageIDs;
        }

        public static void GenerateSubmissionsFromNotebookPages(string cachePath)
        {
            //var notebookNameComposites = GetAvailableNotebookNameCompositesInCache(cachePath);
            //foreach (var notebookNameComposite in notebookNameComposites.Where(x => x.OwnerTypeTag == "S"))
            //{
            //    var pageFolderPath = Path.Combine(notebookNameComposite.NotebookFolderPath, "Pages");
            //    var pageNameComposites = GetAvailablePagesNameCompositesInFolder(pageFolderPath);
            //    foreach (var pageNameComposite in pageNameComposites)
            //    {
            //        var page = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
            //        var submission = page.NextVersionCopy();
            //        submission.SavePageLocally(pageFolderPath);
            //    }
            //}
        }

        #endregion //Static Notebook Methods
    }
}