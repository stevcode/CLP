using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Runtime.Serialization;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner.Services
{
    public class NotebookService : INotebookService
    {
        public NotebookService()
        {
            //Warm up Serializer to make loading of notebook faster.
            var typesToWarmup = new[] { typeof (Notebook) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);
        }

        public List<string> AvailableLocalCacheNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                return
                    directoryInfo.GetDirectories()
                                 .Where(directory => directory.Name.StartsWith("Cache"))
                                 .Select(directory => directory.Name)
                                 .OrderBy(x => x)
                                 .ToList();
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

        public List<string> AvailableLocalNotebookNames
        {
            get
            {
                if (CurrentLocalCacheDirectory == null)
                {
                    return new List<string>();
                }

                var directoryInfo = new DirectoryInfo(CurrentNotebookCacheDirectory);
                return directoryInfo.GetDirectories().Select(directory => directory.Name).OrderBy(x => x).ToList();
            }
        }

        private readonly List<Notebook> _openNotebooks = new List<Notebook>();

        public List<Notebook> OpeNotebooks
        {
            get { return _openNotebooks; }
        }

        public Notebook CurrentNotebook { get; set; }

        public ClassPeriod CurrentClassPeriod { get; set; }

        public bool InitializeNewLocalCache(string cacheName)
        {
            return InitializeNewLocalCache(cacheName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        public bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath)
        {
            var directoryInfo = new DirectoryInfo(cacheDirectoryPath);
            var availableCacheNames =
                directoryInfo.GetDirectories()
                             .Where(directory => directory.Name.StartsWith("Cache"))
                             .Select(directory => directory.Name)
                             .OrderBy(x => x)
                             .ToList();

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
    }
}