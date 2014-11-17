using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface INotebookService {
        List<string> AvailableLocalCacheNames { get; }
        string CurrentLocalCacheDirectory { get; set; }
        string CurrentClassCacheDirectory { get; }
        string CurrentImageCacheDirectory { get; }
        string CurrentNotebookCacheDirectory { get; }
        List<ClassPeriodNameComposite> AvailableLocalClassPeriodNameComposites { get; }
        ClassPeriod CurrentClassPeriod { get; set; }
        List<NotebookNameComposite> AvailableLocalNotebookNameComposites { get; }
        List<Notebook> OpenNotebooks { get; }
        Notebook CurrentNotebook { get; set; }
        Notebook CurrentNotebooksAuthoredSource { get; }
        bool InitializeNewLocalCache(string cacheName);
        bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath);
        void ArchiveNotebookCache(string notebookCacheDirectory);
        void StartSoonestClassPeriod(string localCacheFolderPath);
        void StartLocalClassPeriod(ClassPeriodNameComposite classPeriodNameComposite, string localCacheFolderPath);
        Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID);
        Notebook CopyNotebookForNewOwner(Notebook originalNotebook, Person newOwner);
        CLPPage CopyPageForNewOwner(CLPPage originalPage, Person newOwner);
        List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, Notebook authoredNotebook, List<string> pageIDs, bool includeSubmissions);
        void OpenLocalNotebook(NotebookNameComposite notebookNameComposite, string localCacheFolderPath);
        void SaveCurrentNotebookLocally();
        void SaveNotebookLocally(Notebook notebook);
    }
}