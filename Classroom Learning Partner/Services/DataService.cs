using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{

    #region Info Classes

    public class CacheInfo
    {
        public CacheInfo(string cacheFolderPath) { CacheFolderPath = cacheFolderPath; }

        public string CacheFolderPath { get; set; }

        public string CacheName
        {
            get
            {
                var directoryInfo = new DirectoryInfo(CacheFolderPath);
                return string.Join(" ", directoryInfo.Name.Split('.').Where(s => !s.ToLower().Contains("cache")));
            }
        }

        public string ClassesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Classes"); }
        }

        public string ImagesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Images"); }
        }

        public string NotebooksFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Notebooks"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            if (!Directory.Exists(ClassesFolderPath))
            {
                Directory.CreateDirectory(ClassesFolderPath);
            }

            if (!Directory.Exists(ImagesFolderPath))
            {
                Directory.CreateDirectory(ImagesFolderPath);
            }

            if (!Directory.Exists(NotebooksFolderPath))
            {
                Directory.CreateDirectory(NotebooksFolderPath);
            }
        }
    }

    public class NotebookInfo
    {
        public NotebookInfo(CacheInfo cache, string notebookFolderPath)
        {
            Cache = cache;
            NotebookFolderPath = notebookFolderPath;
        }

        public CacheInfo Cache { get; set; }

        public Notebook Notebook { get; set; }

        public List<CLPPage> Pages { get; set; }

        public string NotebookFolderPath { get; set; }

        public string DisplaysFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Displays"); }
        }

        public string PagesFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Pages"); }
        }

        public string NotebookFilePath
        {
            get { return Path.Combine(NotebookFolderPath, "notebook.xml"); }
        }

        public NotebookNameComposite NameComposite
        {
            get { return NotebookNameComposite.ParseFolderPath(NotebookFolderPath); }
        }

        public string LastSavedTime
        {
            get { return !File.Exists(NotebookFilePath) ? string.Empty : File.GetLastWriteTime(NotebookFilePath).ToString("MM/dd/yy HH:mm:ss"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(NotebookFolderPath))
            {
                Directory.CreateDirectory(NotebookFolderPath);
            }

            if (!Directory.Exists(DisplaysFolderPath))
            {
                Directory.CreateDirectory(DisplaysFolderPath);
            }

            if (!Directory.Exists(PagesFolderPath))
            {
                Directory.CreateDirectory(PagesFolderPath);
            }
        }
    }

    #endregion //Info Classes

    public class DataService : IDataService
    {
        public DataService()
        {
            //Warm up Serializer to make deserializing Notebooks, ClassPeriods, and ClassSubjects faster.
            var typesToWarmup = new[] { typeof (Notebook), typeof (ClassPeriod), typeof (ClassSubject) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);

            CurrentCachesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        #region Properties

        #region Cache Properties

        public string CurrentCachesFolderPath { get; set; }

        public List<CacheInfo> AvailableCaches
        {
            get { return GetCachesInFolder(CurrentCachesFolderPath); }
        }

        public CacheInfo CurrentCache { get; set; }

        #endregion //Cache Properties

        #region Notebook Properties

        public List<NotebookInfo> NotebooksInCurrentCache
        {
            get { return GetNotebooksInCache(CurrentCache); }
        }

        private readonly List<NotebookInfo> _openNotebooksInfo = new List<NotebookInfo>();

        public List<NotebookInfo> OpenNotebooksInfo
        {
            get { return _openNotebooksInfo; }
        }

        public NotebookInfo CurrentNotebookInfo { get; set; }

        public Notebook CurrentNotebook
        {
            get { return CurrentNotebookInfo == null ? null : CurrentNotebookInfo.Notebook; }
        }

        #endregion //Notebook Properties

        #endregion //Properties

        #region Methods

        #region Cache Methods

        public static List<CacheInfo> GetCachesInFolder(string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return new List<CacheInfo>();
            }

            var directoryInfo = new DirectoryInfo(cachesFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Where(directory => directory.Name.StartsWith("Cache"))
                             .Select(directory => new CacheInfo(directory.FullName))
                             .OrderBy(c => c.CacheName)
                             .ToList();
        }

        public CacheInfo CreateNewCache(string cacheName) { return CreateNewCache(cacheName, CurrentCachesFolderPath); }

        public CacheInfo CreateNewCache(string cacheName, string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return null;
            }

            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            cacheName = invalidFileNameCharacters.Aggregate(cacheName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var cacheFileName = "Cache." + cacheName;
            var cacheFolderPath = Path.Combine(cachesFolderPath, cacheFileName);
            var availableCaches = GetCachesInFolder(cachesFolderPath);

            var existingCache = availableCaches.FirstOrDefault(c => c.CacheFolderPath == cacheFolderPath);
            if (existingCache != null)
            {
                CurrentCache = existingCache;
                return existingCache;
            }

            //TODO: Necessary to create yet? Wait 'til save?
            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }

            var newCache = new CacheInfo(cacheFolderPath);
            CurrentCache = newCache;

            return newCache;
        }

        #endregion //Cache Methods 

        #region Notebook Methods

        public static List<NotebookInfo> GetNotebooksInCache(CacheInfo cache)
        {
            if (!Directory.Exists(cache.NotebooksFolderPath))
            {
                return new List<NotebookInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.NotebooksFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => new NotebookInfo(cache, directory.FullName))
                             .Where(nc => nc != null)
                             .OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                             .ThenBy(nc => nc.NameComposite.OwnerName)
                             .ToList();
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum) { return CreateNewNotebook(notebookName, curriculum, CurrentCache); }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            notebookName = invalidFileNameCharacters.Aggregate(notebookName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var newNotebook = new Notebook(notebookName, Person.Author)
                              {
                                  Curriculum = curriculum
                              };

            var newPage = new CLPPage(Person.Author);
            newNotebook.AddPage(newPage);

            var notebookFolderName = NotebookNameComposite.ParseNotebook(newNotebook).ToFolderName();
            var notebookFolderPath = Path.Combine(cache.NotebooksFolderPath, notebookFolderName);
            if (Directory.Exists(notebookFolderPath))
            {
                return null;
            }

            var notebookInfo = new NotebookInfo(cache, notebookFolderPath);

            OpenNotebooksInfo.Add(notebookInfo);
            SetCurrentNotebook(notebookInfo);

            return notebookInfo;
        }

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isNotebookCurrentNotebook = true)
        {
            // Guarantee folder structure.
            notebookInfo.Cache.Initialize();
            notebookInfo.Initialize();

            // Is Notebook already loaded in memory?
            var existingNotebookInfo = OpenNotebooksInfo.FirstOrDefault(n => n.NotebookFilePath == notebookInfo.NotebookFilePath);
            if (existingNotebookInfo != null)
            {
                if (isForcedOpen)
                {
                    OpenNotebooksInfo.Remove(existingNotebookInfo);
                }
                else
                {
                    if (CurrentNotebookInfo == existingNotebookInfo)
                    {
                        notebookInfo.Notebook = CurrentNotebook;
                        App.MainWindowViewModel.IsBackStageVisible = false;
                        return;
                    }
                    SetCurrentNotebook(existingNotebookInfo);
                    return;
                }
            }

            // Is Notebook included in notebookInfo (e.g. send across the network instead of being loaded from the disk).
            if (notebookInfo.Notebook != null)
            {
                OpenNotebooksInfo.Add(notebookInfo);
                if (isNotebookCurrentNotebook)
                {
                    SetCurrentNotebook(notebookInfo);
                }
                return;
            }

            // Load Notebook from disk.
            var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFolderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook couldn't be loaded.");
                return;
            }

            notebookInfo.Notebook = notebook;

            OpenNotebooksInfo.Add(notebookInfo);
            if (isNotebookCurrentNotebook)
            {
                SetCurrentNotebook(notebookInfo);
            }
        }

        public void SetCurrentNotebook(NotebookInfo notebookInfo)
        {
            CurrentNotebookInfo = notebookInfo;
            
            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            App.MainWindowViewModel.CurrentNotebookName = CurrentNotebookInfo.Notebook.Name;
            App.MainWindowViewModel.CurrentUser = CurrentNotebookInfo.Notebook.Owner;
            App.MainWindowViewModel.IsAuthoring = CurrentNotebookInfo.Notebook.OwnerID == Person.Author.ID;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #endregion //Notebook Methods

        #region Page Methods

        public void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced, bool isLoadingSubmissions)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (isExistingPagesReplaced)
            {
                notebookInfo.Notebook.Pages.Clear();
            }

            var newNotebookPages = new List<CLPPage>();

            if (notebookInfo.Pages != null &&
                notebookInfo.Pages.Any()) // Load pages includeded in notebookInfo (e.g. ones sent across the network).
            {
                newNotebookPages = notebookInfo.Pages;
            }
            else // Load local pages.
            {
                var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

                Parallel.ForEach(pageFilePaths,
                                 pageFilePath =>
                                 {
                                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                     if (pageNameComposite == null ||
                                         pageNameComposite.VersionIndex != "0")
                                     {
                                         return;
                                     }

                                     var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
                                     if (!isPageToBeLoaded)
                                     {
                                         return;
                                     }

                                     var page = CLPPage.LoadFromXML(pageFilePath);
                                     if (page == null)
                                     {
                                         return;
                                     }

                                     newNotebookPages.Add(page);
                                 });
            }

            foreach (var page in newNotebookPages)
            {
                var index = notebookInfo.Notebook.Pages.ToList().BinarySearch(page, new PageNumberComparer());
                if (index < 0)
                {
                    index = ~index;
                }
                notebookInfo.Notebook.Pages.Insert(index, page);

                if (notebookInfo.Notebook.CurrentPageID == page.ID &&
                    notebookInfo.Notebook.CurrentPageOwnerID == page.OwnerID &&
                    notebookInfo.Notebook.CurrentPageVersionIndex == page.VersionIndex)
                {
                    notebookInfo.Notebook.CurrentPage = page;
                }
            }

            if (notebookInfo.Notebook.CurrentPage == null)
            {
                notebookInfo.Notebook.CurrentPage = notebookInfo.Notebook.Pages.FirstOrDefault();
            }

            if (!isLoadingSubmissions ||
                notebookInfo.Pages != null && 
                notebookInfo.Pages.Any())
            {
                return;
            }

            // Load submissions from disk.
            if ((App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher && 
                 App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector) ||
                notebookInfo.Notebook.Owner.ID == Person.Author.ID ||
                notebookInfo.Notebook.Owner.IsStudent) // Load student's own submission history.
            {
                var submissions = LoadOwnSubmissionsForLoadedPages(notebookInfo);

                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(p => p.ID == page.ID).OrderBy(p => p.VersionIndex).ToList());
                }
            }
            else // Load all student submissions for Teacher Notebook.
            {
                var notebookInfos = GetNotebooksInCache(notebookInfo.Cache).Where(n => n.NameComposite.ID == notebookInfo.Notebook.ID && n.NameComposite.OwnerTypeTag == "S");
                var pageFilePathsToCheck = new List<string>();

                foreach (var info in notebookInfos)
                {
                    pageFilePathsToCheck.AddRange(Directory.EnumerateFiles(info.PagesFolderPath, "*.xml").ToList());
                }

                var submissions = LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePathsToCheck);

                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
                }
            }
        }

        public static List<string> GetAllPageIDsInNotebook(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new List<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public static List<string> GetPageIDsFromPageNumbers(NotebookInfo notebookInfo, List<int> pageNumbers)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new List<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 int pageNumber;
                                 var isPageNumber = int.TryParse(pageNameComposite.PageNumber, out pageNumber);
                                 if (!isPageNumber)
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = pageNumbers.Contains(pageNumber);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public static List<CLPPage> LoadOwnSubmissionsForLoadedPages(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            return LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePaths);
        }

        public static List<CLPPage> LoadGivenSubmissionsForLoadedPages(NotebookInfo notebookInfo, List<string> pageFilePathsToCheck)
        {
            var submissions = new List<CLPPage>();

            Parallel.ForEach(pageFilePathsToCheck,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex == "0")
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 var page = CLPPage.LoadFromXML(pageFilePath);
                                 if (page == null)
                                 {
                                     return;
                                 }

                                 submissions.Add(page);
                             });

            return submissions;
        }

        #endregion //Page Methods

        #region ClassPeriod Methods

        #endregion //ClassPeriod Methods

        #region ClassSubject Methods

        #endregion //ClassSubject Methods

        #endregion //Methods
    }
}