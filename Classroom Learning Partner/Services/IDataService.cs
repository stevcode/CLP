using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService
    {
        string CurrentCachesFolderPath { get; set; }
        List<CacheInfo> AvailableCaches { get; }
        CacheInfo CurrentCacheInfo { get; set; }
        List<NotebookInfo> NotebooksInCurrentCache { get; }
        List<NotebookInfo> OpenNotebooksInfo { get; }
        NotebookInfo CurrentNotebookInfo { get; set; }
        Notebook CurrentNotebook { get; }

        CacheInfo CreateNewCache(string cacheName);
        CacheInfo CreateNewCache(string cacheName, string cachesFolderPath);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache);
        void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isNotebookCurrentNotebook = true);
        void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced, bool isLoadingSubmissions);
        void SetCurrentNotebook(NotebookInfo notebookInfo);
    }
}