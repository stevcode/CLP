using System;
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
        CLPPage CurrentPage { get; }
        ClassRoster CurrentClassRoster { get; set; }
        NotebookSet CurrentNotebookSet { get; set; }

        event EventHandler<EventArgs> CurrentNotebookChanged;
        event EventHandler<EventArgs> CurrentPageChanged;

        void AutoSavePage(CLPPage page);
        void SetCurrentPage(CLPPage page);
        void DeletePage(CLPPage page);
        void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true);
        void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        void LoadLocalSubmissions(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        void SetCurrentNotebook(NotebookInfo notebookInfo);
        void GenerateSubmissionsFromModifiedStudentPages();
        List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel);

        void CreateAuthorNotebook(string notebookName);

        void CreateTestNotebookSet();
    }
}