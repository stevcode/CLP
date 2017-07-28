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
        bool IsAutoSaveOn { get; set; }

        Dictionary<string, BitmapImage> ImagePool { get; }

        List<Notebook> LoadedNotebooks { get; }

        ClassRoster CurrentClassRoster { get; }
        NotebookSet CurrentNotebookSet { get; }
        Notebook CurrentNotebook { get; }
        IDisplay CurrentDisplay { get; }
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
        void GenerateClassNotebooks(Notebook authorNotebook, ClassRoster classRoster);
        void LoadNotebook(Notebook notebook, List<int> pageNumbers, bool isLoadingStudentNotebooks = true, string overwrittenStartingPageID = "");

        // Display
        void SetCurrentDisplay(IDisplay display);
        void AddDisplay(Notebook notebook, IDisplay display);
        void AddPageToCurrentDisplay(CLPPage page, bool isSavingOldPage = true);
        void RemovePageFromCurrentDisplay(CLPPage page, bool isSavingRemovedPage = true);

        // Page
        void SetCurrentPage(CLPPage page, bool isSavingOldPage = true);
        void AddPage(Notebook notebook, CLPPage page, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true);
        void InsertPageAt(Notebook notebook, CLPPage page, int index, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, int numberOfDifferentiatedPages = -1);
        void DeletePage(Notebook notebook, CLPPage page, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, bool isPreventingEmptyNotebook = true);
        void DeletePageAt(Notebook notebook, int index, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, bool isPreventingEmptyNotebook = true);
        void MovePage(Notebook notebook, CLPPage page, int newPageNumber);
        void AutoSavePage(Notebook notebook, CLPPage page);
        List<CLPPage> GetLoadedSubmissionsForPage(CLPPage page);
    }
}