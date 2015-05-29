using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService {
        string CurrentCachesFolderPath { get; set; }
        List<CacheInfo> AvailableCaches { get; }
        CacheInfo CurrentCache { get; set; }
        List<NotebookInfo> NotebooksInCurrentCache { get; }
        bool CreateNewCache(string cacheName);
        bool CreateNewCache(string cacheName, string cachesFolderPath);
    }
}