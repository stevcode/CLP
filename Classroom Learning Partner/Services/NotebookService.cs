using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using Path = Catel.IO.Path;

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
            var directoryInfo = new DirectoryInfo(cacheDirectoryPath);
            var availableCacheNames =
                directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => directory.Name).OrderBy(x => x).ToList();

            if (availableCacheNames.Contains(cacheName))
            {
                return false;
            }

            CurrentLocalCacheDirectory = Path.Combine(cacheDirectoryPath, cacheName);
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
            var authoredNotebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == Person.Author.ID);
            var authoredPagesFolderPath = Path.Combine(authoredNotebookNameComposite.FullNotebookDirectoryPath, "Pages");
            var pageIDs = GetPageIDsFromStartIDAndForwardRange(authoredPagesFolderPath, classPeriod.StartPageID, classPeriod.NumberOfPages);

            var authoredPages = LoadOrCopyPagesForNotebook(authoredNotebook, pageIDs, false);
            authoredNotebook.Pages = new ObservableCollection<CLPPage>(authoredPages);

            var teacherNotebook = LoadClassPeriodNotebookForPerson(classPeriod, classPeriod.ClassSubject.TeacherID) ??
                                  CopyNotebookForNewOwner(authoredNotebook, classPeriod.ClassSubject.Teacher);

            var teacherPages = LoadOrCopyPagesForNotebook(teacherNotebook, pageIDs, true);
            teacherNotebook.Pages = new ObservableCollection<CLPPage>(teacherPages);

            OpenNotebooks.Clear();
            OpenNotebooks.Add(authoredNotebook);
            OpenNotebooks.Add(teacherNotebook);
            CurrentNotebook = teacherNotebook;
        }

        public Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID)
        {
            var notebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == ownerID);
            if (notebookNameComposite == null)
            {
                MessageBox.Show("Notebook for Class Period not found for " + ownerID + ".");
                return null;
            }

            var notebook = Notebook.LoadLocalNotebook(notebookNameComposite.FullNotebookDirectoryPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook for Class Period could not be loaded " + ownerID + ".");
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

        public List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, List<string> pageIDs, bool includeSubmissions)
        {
            var pages = new List<CLPPage>();
            var notebookNameComposite =
                GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory).FirstOrDefault(x => x.ID == notebook.ID && x.OwnerID == notebook.Owner.ID);
            if (notebookNameComposite == null)
            {
                return pages;
            }

            var pagesFolderPath = Path.Combine(notebookNameComposite.FullNotebookDirectoryPath, "Pages");
            var pageNameComposites = GetAvailablePagesNameCompositesInFolder(pagesFolderPath);
        }

        #endregion //Class Period

        #region Notebook

        public void OpenLocalNotebook(NotebookNameComposite notebookNameComposite, string localCacheFolderPath)
        {
            //Notebook is already loaded in memory
            var existingNotebook = OpenNotebooks.FirstOrDefault(x => x.ID == notebookNameComposite.ID && x.OwnerID == notebookNameComposite.OwnerID);
            if (existingNotebook != null)
            {
                if (CurrentNotebook == existingNotebook)
                {
                    return;
                }
                CurrentNotebook = existingNotebook;
                App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
                App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(existingNotebook);
                return;
            }

            //Open New Notebook from disk.
            var folderPath = notebookNameComposite.FullNotebookDirectoryPath;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var notebook = Notebook.LoadLocalFullNotebook(folderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            CurrentLocalCacheDirectory = localCacheFolderPath;
            OpenNotebooks.Add(notebook);
            CurrentNotebook = notebook;
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
            if (notebook.OwnerID == Person.Author.ID)
            {
                App.MainWindowViewModel.IsAuthoring = true;
            }
            App.MainWindowViewModel.CurrentUser = notebook.Owner;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        public void SaveCurrentNotebookLocally() { SaveNotebookLocally(CurrentNotebook); }

        public void SaveNotebookLocally(Notebook notebook)
        {
            var folderPath = Path.Combine(CurrentNotebookCacheDirectory, notebook.NotebookToNotebookFolderName());

            if (App.MainWindowViewModel.CurrentUser.ID == Person.Author.ID)
            {
                var pagesFolderPath = Path.Combine(folderPath, "Pages");
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

            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramModes.Author:
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
                    break;
                case ProgramModes.Projector:
                    notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
                    break;
                case ProgramModes.Student:
                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    notebook.SaveSubmissions(submissionsPath);
                    if (App.Network.InstructorProxy != null)
                    {
                        var sNotebook = ObjectSerializer.ToString(notebook);
                        var zippedNotebook = CLPServiceAgent.Instance.Zip(sNotebook);
                        App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
                    }
                    break;
            }

            if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher &&
                App.MainWindowViewModel.CurrentClassPeriod != null &&
                App.MainWindowViewModel.CurrentClassPeriod.ClassSubject != null)
            {
                App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.SaveClassSubject(CurrentClassCacheDirectory);
            }
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
                             .Select(directory => NotebookNameComposite.ParseDirectoryToNameComposite(directory.FullName))
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
                             .Select(file => PageNameComposite.ParseFilePathToNameComposite(file.FullName))
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

        #endregion //Static Notebook Methods
    }
}