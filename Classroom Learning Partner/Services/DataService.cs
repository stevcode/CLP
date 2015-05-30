using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public NotebookInfo(string notebookFolderPath) { NotebookFolderPath = notebookFolderPath; }

        public CacheInfo Cache { get; set; }

        public Notebook Notebook { get; set; }

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

        public CacheInfo CreateNewCache(string cacheName, bool isNewCacheCurrentCache = true) { return CreateNewCache(cacheName, CurrentCachesFolderPath, isNewCacheCurrentCache); }

        public CacheInfo CreateNewCache(string cacheName, string cachesFolderPath, bool isNewCacheCurrentCache = true)
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
                if (!isNewCacheCurrentCache)
                {
                    return null;
                }

                CurrentCache = existingCache;
                return existingCache;
            }

            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }

            var newCache = new CacheInfo(cacheFolderPath);
            if (isNewCacheCurrentCache)
            {
                CurrentCache = newCache;
            }

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
            return directoryInfo.GetDirectories().Select(directory => new NotebookInfo(directory.FullName)
                                                                      {
                                                                          Cache = cache
                                                                      })
                                .Where(nc => nc != null)
                                .OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                                .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                                .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                                .ThenBy(nc => nc.NameComposite.OwnerName)
                                .ToList();
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum)
        {
            return CreateNewNotebook(notebookName, curriculum, CurrentCache, isNewNotebookCurrentNotebook);
        }

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

            var notebookInfo = new NotebookInfo(notebookFolderPath)
                               {
                                   Notebook = newNotebook,
                                   Cache = cache
                               };

            OpenNotebooksInfo.Add(notebookInfo);
            SetCurrentNotebook(notebookInfo);

            return notebookInfo;
        }

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isNotebookCurrentNotebook = true)
        {
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
            var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFilePath);
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

        public void LoadPages(NotebookInfo notebookInfo)
        {
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

        public void SetCurrentNotebook(NotebookInfo notebookInfo)
        {
            CurrentNotebookInfo = notebookInfo;

            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            App.MainWindowViewModel.IsAuthoring = notebookInfo.Notebook.OwnerID == Person.Author.ID;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #endregion //Notebook Methods

        #region Page Methods

        #endregion //Page Methods

        #region ClassPeriod Methods

        #endregion //ClassPeriod Methods

        #region ClassSubject Methods

        #endregion //ClassSubject Methods

        #endregion //Methods
    }
}