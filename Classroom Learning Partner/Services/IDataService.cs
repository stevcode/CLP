using System;
using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService
    {
        string CurrentCLPDataFolderPath { get; set; }
        string CurrentCacheFolderPath { get; }
        string CurrentTempCacheFolderPath { get; }
        string CurrentArchiveFolderPath { get; }
        NotebookSet CurrentNotebookSet { get; }
        ClassRoster CurrentClassRoster { get; }
        Notebook CurrentNotebook { get; }
        CLPPage CurrentPage { get; }

        event EventHandler<EventArgs> CurrentClassRosterChanged;
        event EventHandler<EventArgs> CurrentNotebookChanged;
        event EventHandler<EventArgs> CurrentPageChanged;

        void SetCurrentClassRoster(ClassRoster classRoster);

        void CreateAuthorNotebook(string notebookName);
        void SetCurrentNotebook(Notebook notebook);

        void SetCurrentPage(CLPPage page);
        void AddPage(Notebook notebook, CLPPage page);
        void InsertPageAt(Notebook notebook, CLPPage page, int index);
        void DeletePage(Notebook notebook, CLPPage page);
        void DeletePageAt(Notebook notebook, int index);
        void AutoSavePage(CLPPage page);

        void AddDisplay(Notebook notebook, IDisplay display);

        void CreateTestNotebookSet();










        CacheInfo CurrentCacheInfo { get; set; }
        List<NotebookInfo> LoadedNotebooksInfo { get; }
        
        void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true);
        void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        void LoadLocalSubmissions(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced);
        List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel);
    }
}