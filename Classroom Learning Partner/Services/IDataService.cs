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

        List<FileInfo> AvailableZipContainerFileInfos { get; }

        Dictionary<string, BitmapImage> ImagePool { get; }
        
        ClassRoster CurrentClassRoster { get; }
        NotebookSet CurrentNotebookSet { get; }
        Notebook CurrentNotebook { get; }
        IDisplay CurrentMultiDisplay { get; }
        CLPPage CurrentPage { get; }

        event EventHandler<EventArgs> CurrentClassRosterChanged;
        event EventHandler<EventArgs> CurrentNotebookChanged;
        event EventHandler<EventArgs> CurrentDisplayChanged;
        event EventHandler<EventArgs> CurrentPageChanged;

        // Cache
        void SaveLocal();

        // Images
        BitmapImage GetImage(string imageHashID, IPageObject pageObject);
        string SaveImageToImagePool(string imageFilePath, CLPPage page);

        // Class Roster
        void SetCurrentClassRoster(ClassRoster classRoster);

        // Session

        // Notebook
        void SetCurrentNotebook(Notebook notebook);
        void CreateAuthorNotebook(string notebookName, string zipContainerFilePath);
        void LoadAllNotebookPages(Notebook notebook, bool isLoadingSubmissions = true);
        void LoadRangeOfNotebookPages(Notebook notebook, List<int> pageNumbers, bool isLoadingSubmissions = true);

        // Display
        void AddDisplay(Notebook notebook, IDisplay display);

        // Page
        void SetCurrentPage(CLPPage page);
        void AddPage(Notebook notebook, CLPPage page);
        void InsertPageAt(Notebook notebook, CLPPage page, int index);
        void DeletePage(Notebook notebook, CLPPage page);
        void DeletePageAt(Notebook notebook, int index);
        void AutoSavePage(Notebook notebook, CLPPage page);
        
        //Obsolete
        List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel);
    }
}