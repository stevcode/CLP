using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService {
        string CurrentCachesFolderPath { get; set; }
        List<CacheInfo> AvailableCaches { get; }
        CacheInfo CurrentCache { get; set; }
        List<NotebookInfo> NotebooksInCurrentCache { get; }
        List<NotebookInfo> OpenNotebooksInfo { get; }
        NotebookInfo CurrentNotebookInfo { get; set; }
        Notebook CurrentNotebook { get; }
        CacheInfo CreateNewCache(string cacheName, bool isNewCacheCurrentCache = true);
        CacheInfo CreateNewCache(string cacheName, string cachesFolderPath, bool isNewCacheCurrentCache = true);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum, bool isNewNotebookCurrentNotebook = true);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache, bool isNewNotebookCurrentNotebook = true);
    }
}