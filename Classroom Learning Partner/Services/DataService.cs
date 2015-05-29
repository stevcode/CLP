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
            get
            {
                var path = Path.Combine(CacheFolderPath, "Classes");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string ImagesFolderPath
        {
            get
            {
                var path = Path.Combine(CacheFolderPath, "Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string NotebooksFolderPath
        {
            get
            {
                var path = Path.Combine(CacheFolderPath, "Notebooks");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
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
            get
            {
                var path = Path.Combine(NotebookFolderPath, "Displays");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string PagesFolderPath
        {
            get
            {
                var path = Path.Combine(NotebookFolderPath, "Pages");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string NotebookFilePath
        {
            get
            {
                var path = Path.Combine(NotebookFolderPath, "notebook.xml");
                return !File.Exists(path) ? string.Empty : path;
            }
        }

        public NotebookNameComposite NameComposite
        {
            get { return NotebookNameComposite.ParseFolderPath(NotebookFolderPath); }
        }

        public string LastSavedTime
        {
            get { return string.IsNullOrEmpty(NotebookFilePath) ? string.Empty : File.GetLastWriteTime(NotebookFilePath).ToString("MM/dd/yy HH:mm:ss"); }
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
            get { return GetNotebooksInFolder(CurrentCache.NotebooksFolderPath); }
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

        public static List<NotebookInfo> GetNotebooksInFolder(string notebooksFolderPath)
        {
            var directoryInfo = new DirectoryInfo(notebooksFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => new NotebookInfo(directory.FullName))
                             .Where(nc => nc != null)
                             .OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                             .ThenBy(nc => nc.NameComposite.OwnerName)
                             .ToList();
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, bool isNewNotebookCurrentNotebook = true)
        {
            return CreateNewNotebook(notebookName, curriculum, CurrentCache, isNewNotebookCurrentNotebook);
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache, bool isNewNotebookCurrentNotebook = true)
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

            // TODO: Reimplement when autosave returns
            //SaveNotebook(newNotebook);

            OpenNotebooksInfo.Add(notebookInfo);
            if (!isNewNotebookCurrentNotebook)
            {
                return notebookInfo;
            }

            CurrentNotebookInfo = notebookInfo;

            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            App.MainWindowViewModel.IsAuthoring = true;
            App.MainWindowViewModel.IsBackStageVisible = false;

            return CurrentNotebookInfo;
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