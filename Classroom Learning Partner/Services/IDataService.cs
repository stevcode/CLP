using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService
    {
        string CurrentCLPDataFolderPath { get; set; }
        string CurrentCachesFolderPath { get; set; }
        List<CacheInfo> AvailableCaches { get; }
        CacheInfo CurrentCacheInfo { get; set; }
        List<NotebookInfo> NotebooksInCurrentCache { get; }
        List<NotebookInfo> LoadedNotebooksInfo { get; }
        NotebookInfo CurrentNotebookInfo { get; set; }
        Notebook CurrentNotebook { get; }

        CacheInfo CreateNewCache(string cacheName, bool isCacheCurrent = true);
        CacheInfo CreateNewCache(string cacheName, string cachesFolderPath, bool isCacheCurrent = true);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum, bool isNotebookCurrent = true);
        NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache, bool isNotebookCurrent = true);
        void AutoSavePage(CLPPage page);
        void SetCurrentPage(CLPPage page);
        void DeletePage(CLPPage page);
        void SaveNotebookLocally(NotebookInfo notebookInfo, bool isForcedFullSave = false);
        void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true);
        void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        void LoadLocalSubmissions(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        void SetCurrentNotebook(NotebookInfo notebookInfo);
        void MigrateCaches();
    }
}