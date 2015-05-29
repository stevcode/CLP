using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Runtime.Serialization;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
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

        public string CurrentCachesFolderPath { get; set; }

        public List<CacheInfo> AvailableCaches
        {
            get { return GetAvailableCaches(CurrentCachesFolderPath); }
        }

        public CacheInfo CurrentCache { get; set; }

        public List<NotebookInfo> NotebooksInCurrentCache
        {
            get { return GetNotebooksInFolder(CurrentCache.NotebooksFolderPath); }
        }

        #endregion //Properties

        #region Methods

        #region Cache Methods

        public static List<CacheInfo> GetAvailableCaches(string cachesFolderPath)
        {
            var directoryInfo = new DirectoryInfo(cachesFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Where(directory => directory.Name.StartsWith("Cache"))
                             .Select(directory => new CacheInfo(directory.FullName))
                             .OrderBy(c => c.CacheName)
                             .ToList();
        }

        public bool CreateNewCache(string cacheName) { return CreateNewCache(cacheName, CurrentCachesFolderPath); }

        public bool CreateNewCache(string cacheName, string cachesFolderPath)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            cacheName = invalidFileNameCharacters.Aggregate(cacheName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var cacheFileName = "Cache." + cacheName;
            var cacheFolderPath = Path.Combine(cachesFolderPath, cacheFileName);
            var availableCaches = GetAvailableCaches(cachesFolderPath);

            var existingCache = availableCaches.FirstOrDefault(c => c.CacheFolderPath == cacheFolderPath);
            if (existingCache != null)
            {
                CurrentCache = existingCache;
                return false;
            }

            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }

            var newCache = new CacheInfo(cacheFolderPath);
            CurrentCache = newCache;

            return true;
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