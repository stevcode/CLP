using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService
    {
        string CurrentCLPDataFolderPath { get; set; }
        string CurrentCacheFolderPath { get; }
        string CurrentTempCacheFolderPath { get; }
        string CurrentArchiveFolderPath { get; }
        Dictionary<string, BitmapImage> ImagePool { get; }
        List<FileInfo> AvailableZipContainerFileInfos { get; }
        NotebookSet CurrentNotebookSet { get; }
        ClassRoster CurrentClassRoster { get; }
        Notebook CurrentNotebook { get; }
        CLPPage CurrentPage { get; }

        event EventHandler<EventArgs> CurrentClassRosterChanged;
        event EventHandler<EventArgs> CurrentNotebookChanged;
        event EventHandler<EventArgs> CurrentPageChanged;

        void SetCurrentClassRoster(ClassRoster classRoster);

        void CreateAuthorNotebook(string notebookName, string zipContainerFilePath);
        void SetCurrentNotebook(Notebook notebook);

        void SetCurrentPage(CLPPage page);
        void AddPage(Notebook notebook, CLPPage page);
        void InsertPageAt(Notebook notebook, CLPPage page, int index);
        void DeletePage(Notebook notebook, CLPPage page);
        void DeletePageAt(Notebook notebook, int index);
        void AutoSavePage(Notebook notebook, CLPPage page);

        void AddDisplay(Notebook notebook, IDisplay display);

        void SaveLocal();

        string SaveImageToImagePool(string imageFilePath, CLPPage page);

        BitmapImage GetImage(string imageHashID, IPageObject pageObject);
        void LoadAllNotebookPages(Notebook notebook, bool isLoadingSubmissions = true);
        void LoadRangeOfNotebookPages(Notebook notebook, List<int> pageNumbers, bool isLoadingSubmissions = true);

        //Obsolete
        List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel);
    }
}