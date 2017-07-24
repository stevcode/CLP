using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel;
using Catel.Collections;
using Catel.Reflection;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    // TODO: Message Service that shows/hides the MainViewModel PensDown screen and modifies the text as appropriate.
    // TODO: Have property called LastSelectedPage. It and CurrentPage show live viewModels, all others show thumbnails
    // SetSelectedPage() will immediate change Current to Last and New to Current, then it will screenshot Last and autosave it in the background.

    public class DataService : IDataService
    {
        #region Nested Classes

        public class ZipEntrySaver
        {
            public ZipEntrySaver(AInternalZipEntryFile entryFile, Notebook parentNotebook = null)
            {
                EntryFile = entryFile;
                InternalFilePath = entryFile.GetZipEntryFullPath(parentNotebook);
                JsonString = entryFile.ToJsonString();
            }

            public AInternalZipEntryFile EntryFile { get; set; }
            private string InternalFilePath { get; set; }
            private string JsonString { get; set; }

            public void UpdateEntry(ZipFile zip, bool isOverwriting = true)
            {
                if (zip.EntryFileNames.Contains(InternalFilePath) &&
                    !isOverwriting)
                {
                    return;
                }

                zip.UpdateEntry(InternalFilePath, JsonString);
            }
        }

        public class PageZipEntryLoader
        {
            public PageZipEntryLoader(string jsonString, int pageNumber)
            {
                JsonString = jsonString;
                PageNumber = pageNumber;
            }

            public string JsonString { get; set; }
            public int PageNumber { get; set; }
        }

        #endregion // Nested Classes

        #region Constants

        private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLPData";
        private const string DEFAULT_CACHE_FOLDER_NAME = "Cache";
        private const string DEFAULT_TEMP_CACHE_FOLDER_NAME = "TempCache";
        private const string DEFAULT_CONFIG_FOLDER_NAME = "Config"; // Config Service?
        private const string DEFAULT_ARCHIVE_FOLDER_NAME = "Archive";
        private const string DEFAULT_LOGS_FOLDER_NAME = "Logs";

        #endregion // Constants

        #region Constructors

        public DataService()
        {
            CurrentCLPDataFolderPath = DefaultCLPDataFolderPath;

            //ConversionService.Combine();
            //ConversionService.Stitch();
            //ConversionService.ConvertAnnCache();
            //ConvertEmilyCache();
            //AnalysisService.RunFullBatchAnalysis(AnalysisService.AllPageNumbersToAnalyze);
            //AnalysisService.RunFullBatchAnalysis(new List<int>{213});
            //AnalysisService.RunFullBatchAnalysisOnAlreadyConvertedPages();
        }

        #endregion // Constructors

        #region IDataService Implementation

        #region Properties

        #region Current Folder Paths

        public string CurrentCLPDataFolderPath { get; set; }

        public string CurrentCacheFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_CACHE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        public string CurrentTempCacheFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_TEMP_CACHE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        public string CurrentArchiveFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_ARCHIVE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Current Folder Paths

        #region Cache Properties

        public List<FileInfo> AvailableZipContainerFileInfos
        {
            get
            {
                // ReSharper disable once ConvertPropertyToExpressionBody
                return GetCLPContainersInFolder(CurrentCacheFolderPath);
            }
        }

        public bool IsAutoSaveOn { get; set; } = false;

        #endregion // Cache Properties

        #region Program Properties

        /// <summary>ImagePool for the current CLP instance, populated by all open notebooks.</summary>
        public Dictionary<string, BitmapImage> ImagePool { get; } = new Dictionary<string, BitmapImage>();

        #endregion // Program Properties

        #region Notebook Properties

        //AvailableNotebookSets   
        //LoadedNotebookSets      //Differentiate between a .clp file and the individual notebooks within?

        public List<Notebook> LoadedNotebooks { get; } = new List<Notebook>();

        public ClassRoster CurrentClassRoster { get; private set; }
        public NotebookSet CurrentNotebookSet { get; private set; }
        public Notebook CurrentNotebook { get; private set; }
        public IDisplay CurrentDisplay => CurrentNotebook?.CurrentDisplay;
        public CLPPage CurrentPage => CurrentNotebook?.CurrentPage;

        #endregion // Notebook Properties

        #region Page Properties

        //CurrentNotebookPage
        //CurrentSubmissionPage
        //CurrentSelectedPage

        #endregion // Page Properties

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> CurrentClassRosterChanged;
        public event EventHandler<EventArgs> CurrentNotebookChanged;
        public event EventHandler<EventArgs> CurrentDisplayChanged;
        public event EventHandler<EventArgs> CurrentPageChanged;

        #endregion // Events

        #region Methods

        #region Cache Methods

        public void SaveLocal()
        {
            var zipContainerFilePath = CurrentNotebook.ContainerZipFilePath;
            var entryList = new List<ZipEntrySaver>
                            {
                                new ZipEntrySaver(CurrentClassRoster, CurrentNotebook),
                                new ZipEntrySaver(CurrentNotebook, CurrentNotebook)
                            };

            foreach (var page in CurrentNotebook.Pages)
            {
                if (page.Owner.ID == Person.AUTHOR_ID)
                {
                    page.History.ClearNonAnimationHistory();
                }

                entryList.Add(new ZipEntrySaver(page, CurrentNotebook));
            }

            if (File.Exists(zipContainerFilePath))
            {
                //var readOptions = new ReadOptions
                //                  {
                //                      ReadProgress = Zip_ReadProgress
                //                  };

                //var zip = ZipFile.Read(fullFilePath, readOptions)

                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    // TODO: Test if needed. Won't work unless zip has been saved.
                    // Implied that entries are not added to zip.Entries until saved. Need to verify. Code definitely says added to internal _entries before save, so test this
                    //zip.SelectEntries("*.json");
                    //zip.SelectEntries("p;*.json", "blah/blah/pages/"); test this.

                    //zip.UpdateFile only applies to adding a file from the disc to the zip archive, N/A for clp unless we need it for images?
                    //          for images, probably zip.AddEntry(entryPath, memoryStream); also have byte[] byteArray for content

                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;  Only one that seems persistent, but need to test
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save();
                }
            }
            else
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(zipContainerFilePath);
                }
            }

            foreach (var zipEntrySaver in entryList)
            {
                zipEntrySaver.EntryFile.IsSavedLocally = true;
            }
        }

        private void Zip_ReadProgress(object sender, ReadProgressEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion // Cache Methods

        #region Image Methods

        private readonly object _zipLock = new object();

        public BitmapImage GetImage(string imageHashID, IPageObject pageObject)
        {
            if (string.IsNullOrWhiteSpace(imageHashID))
            {
                return null;
            }

            if (ImagePool.ContainsKey(imageHashID))
            {
                return ImagePool[imageHashID];
            }

            var parentPage = pageObject.ParentPage;
            if (parentPage == null)
            {
                return null;
            }

            lock (_zipLock)
            {
                var containerZipFilePath = parentPage.ContainerZipFilePath;

                using (var zip = ZipFile.Read(containerZipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    var entry = GetImageEntryFromImageHashID(zip, imageHashID);
                    if (entry == null)
                    {
                        return null;
                    }

                    using (var ms = new MemoryStream())
                    {
                        entry.Extract(ms);

                        var genBmpImage = new BitmapImage();

                        genBmpImage.BeginInit();
                        genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
                        genBmpImage.StreamSource = ms;
                        genBmpImage.EndInit();

                        genBmpImage.Freeze();

                        ImagePool.Add(imageHashID, genBmpImage);

                        return genBmpImage;
                    }
                }
            }
        }

        public string SaveImageToImagePool(string imageFilePath, CLPPage page)
        {
            try
            {
                var bytes = File.ReadAllBytes(imageFilePath);

                var md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(bytes);
                var imageHashID = Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Replace("=", "");
                if (ImagePool.ContainsKey(imageHashID))
                {
                    return imageHashID;
                }

                var bitmapImage = CLPImage.GetImageFromPath(imageFilePath);
                if (bitmapImage == null)
                {
                    MessageBox.Show("Failed to load image..");
                    return null;
                }

                var newFileName = $"{imageHashID};{Path.GetFileNameWithoutExtension(imageFilePath)}{Path.GetExtension(imageFilePath)}";
                var internalFilePath = ZipExtensions.CombineEntryDirectoryAndName(AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME, newFileName);
                var containerZipFilePath = page.ContainerZipFilePath;

                using (var zip = ZipFile.Read(containerZipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    if (!zip.ContainsEntry(internalFilePath))
                    {
                        var entry = zip.AddFile(imageFilePath);
                        entry.FileName = internalFilePath;
                        zip.Save();
                    }
                }

                ImagePool.Add(imageHashID, bitmapImage);
                return imageHashID;
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening image file. Please try again.");
            }

            return null;
        }

        #endregion // Image Methods

        #region ClassRoster Methods

        public void SetCurrentClassRoster(ClassRoster classRoster)
        {
            CurrentClassRoster = classRoster;
            CurrentClassRosterChanged.SafeInvoke(this);
        }

        #endregion // ClassRoster Methods

        #region Notebook Methods

        public void SetCurrentNotebook(Notebook notebook)
        {
            if (CurrentNotebook != null)
            {
                SavePage(CurrentNotebook, CurrentNotebook.CurrentPage);
            }

            CurrentNotebook = notebook;
            CurrentNotebookChanged.SafeInvoke(this);

            if (CurrentNotebook.CurrentPage == null)
            {
                return;
            }

            AddPageToCurrentDisplay(CurrentNotebook.CurrentPage);
        }

        public void CreateAuthorNotebook(string notebookName, string zipContainerFilePath)
        {
            var notebookSet = new NotebookSet(notebookName);
            var notebook = new Notebook(notebookSet, Person.Author)
                           {
                               ContainerZipFilePath = zipContainerFilePath
                           };

            if (!File.Exists(zipContainerFilePath))
            {
                CreateEmptyZipContainer(zipContainerFilePath);
                CurrentClassRoster = new ClassRoster();
            }
            else
            {
                CurrentClassRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            }

            CurrentClassRoster.ContainerZipFilePath = zipContainerFilePath;
            CurrentClassRoster.ListOfNotebookSets.Add(notebookSet);
            SaveClassRoster(CurrentClassRoster);

            LoadedNotebooks.Add(notebook);
            SetCurrentNotebook(notebook);
            AddPage(notebook, new CLPPage(Person.Author));
        }

        public void GenerateClassNotebooks(Notebook authorNotebook, ClassRoster classRoster)
        {
            var zipContainerFilePath = authorNotebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var entryList = new List<ZipEntrySaver>();

                var notebookEntries = zip.SelectEntries($"*{Notebook.DEFAULT_INTERNAL_FILE_NAME}.json");

                // TODO: Parallel?
                foreach (var teacher in classRoster.ListOfTeachers)
                {
                    var notebook = authorNotebook.CopyNotebookForNewOwner(teacher);
                    if (notebookEntries.Select(e => e.FileName).Contains(notebook.GetZipEntryFullPath(notebook)))
                    {
                        continue;
                    }

                    notebook.ContainerZipFilePath = authorNotebook.ContainerZipFilePath;
                    entryList.Add(new ZipEntrySaver(notebook, notebook));

                    foreach (var authorPage in authorNotebook.Pages)
                    {
                        var page = authorPage.CopyPageForNewOwner(teacher);
                        page.ContainerZipFilePath = authorNotebook.ContainerZipFilePath;
                        entryList.Add(new ZipEntrySaver(page, notebook));
                        notebook.Pages.Add(page);
                    }

                    notebook.Pages = notebook.Pages.OrderBy(p => p.PageNumber).ThenBy(p => p.DifferentiationLevel).ToList().ToObservableCollection();
                    LoadedNotebooks.Add(notebook);
                }

                // TODO: Parallel?
                foreach (var student in classRoster.ListOfStudents)
                {
                    var notebook = authorNotebook.CopyNotebookForNewOwner(student);
                    if (notebookEntries.Select(e => e.FileName).Contains(notebook.GetZipEntryFullPath(notebook)))
                    {
                        continue;
                    }

                    notebook.ContainerZipFilePath = authorNotebook.ContainerZipFilePath;
                    entryList.Add(new ZipEntrySaver(notebook, notebook));

                    var differentiatedPageIDs = new List<string>();
                    foreach (var authorPage in authorNotebook.Pages)
                    {
                        if (authorPage.DifferentiationLevel != "0")
                        {
                            differentiatedPageIDs.Add(authorPage.ID);
                            continue;
                        }

                        var page = authorPage.CopyPageForNewOwner(student);
                        page.ContainerZipFilePath = authorNotebook.ContainerZipFilePath;
                        entryList.Add(new ZipEntrySaver(page, notebook));
                        notebook.Pages.Add(page);
                    }

                    foreach (var differentiatedPageID in differentiatedPageIDs.Distinct().ToList())
                    {
                        var differentiatedInstances = authorNotebook.Pages.Where(p => p.ID == differentiatedPageID).OrderBy(p => p.DifferentiationLevel).ToList();
                        var authorPage = differentiatedInstances.FirstOrDefault(p => p.DifferentiationLevel == student.CurrentDifferentiationGroup) ?? differentiatedInstances.FirstOrDefault();
                        if (authorPage == null)
                        {
                            continue;
                        }

                        var page = authorPage.CopyPageForNewOwner(student);
                        page.ContainerZipFilePath = authorNotebook.ContainerZipFilePath;
                        entryList.Add(new ZipEntrySaver(page, notebook));
                        notebook.Pages.Add(page);
                    }

                    notebook.Pages = notebook.Pages.OrderBy(p => p.PageNumber).ThenBy(p => p.DifferentiationLevel).ToList().ToObservableCollection();
                    LoadedNotebooks.Add(notebook);
                }

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip, false);
                }

                zip.Save();
            }
        }

        public void LoadNotebook(Notebook notebook, List<int> pageNumbers, bool isLoadingStudentNotebooks = true, string overwrittenStartingPageID = "")
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            var classRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            SetCurrentClassRoster(classRoster);

            var existingNotebook = LoadedNotebooks.FirstOrDefault(n => n.ID == notebook.ID && n.Owner.ID == notebook.Owner.ID);
            if (existingNotebook == null)
            {
                LoadedNotebooks.Add(notebook);
                existingNotebook = notebook;
            }

            LoadPagesIntoNotebook(existingNotebook, pageNumbers, overwrittenStartingPageID);

            // Load Student Notebooks
            if ((!owner.IsStudent &&
                 isLoadingStudentNotebooks) ||
                owner.ID == Person.AUTHOR_ID)
            {
                var otherNotebooks = LoadAllNotebooksFromCLPContainer(zipContainerFilePath).Where(n => n.ID == notebook.ID);
                foreach (var studentNotebook in otherNotebooks.Where(n => n.Owner.IsStudent && classRoster.ListOfStudents.Any(p => n.Owner.DisplayName == p.DisplayName)))
                {
                    var existingStudentNotebook = LoadedNotebooks.FirstOrDefault(n => n.ID == studentNotebook.ID && n.Owner.ID == studentNotebook.Owner.ID);
                    if (existingStudentNotebook == null)
                    {
                        LoadedNotebooks.Add(studentNotebook);
                        existingStudentNotebook = studentNotebook;
                    }

                    LoadPagesIntoNotebook(existingStudentNotebook, pageNumbers, overwrittenStartingPageID);
                }

                // Also Load Teacher Notebooks for Editing
                if (owner.ID == Person.AUTHOR_ID)
                {
                    foreach (var teacherNotebook in otherNotebooks.Where(n => !n.Owner.IsStudent && classRoster.ListOfTeachers.Any(p => n.Owner.DisplayName == p.DisplayName)))
                    {
                        var existingTeacherNotebook = LoadedNotebooks.FirstOrDefault(n => n.ID == teacherNotebook.ID && n.Owner.ID == teacherNotebook.Owner.ID);
                        if (existingTeacherNotebook == null)
                        {
                            LoadedNotebooks.Add(teacherNotebook);
                            existingTeacherNotebook = teacherNotebook;
                        }

                        LoadPagesIntoNotebook(existingTeacherNotebook, pageNumbers, overwrittenStartingPageID);
                    }
                }
            }

            SetCurrentNotebook(existingNotebook);
        }

        #endregion // Notebook Methods

        #region Display Methods

        public void SetCurrentDisplay(IDisplay display)
        {
            CurrentNotebook.CurrentDisplay = display;

            CurrentDisplayChanged.SafeInvoke(this);
        }

        public void AddDisplay(Notebook notebook, IDisplay display)
        {
            display.NotebookID = notebook.ID;
            display.DisplayNumber = notebook.Displays.Any(d => d.GetType() == display.GetType()) ? notebook.Displays.Last().DisplayNumber + 1 : 1;
            notebook.Displays.Add(display);
        }

        public void AddPageToCurrentDisplay(CLPPage page, bool isSavingOldPage = true)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            CurrentDisplay?.Pages.Add(page);
            SetCurrentPage(page, isSavingOldPage);
        }

        public void RemovePageFromCurrentDisplay(CLPPage page, bool isSavingRemovedPage = true)
        {
            if (CurrentNotebook == null ||
                CurrentDisplay == null)
            {
                return;
            }

            // Save previously selected page, assuming it wasn't the page being removed.
            var oldPage = CurrentNotebook.CurrentPage;
            if (oldPage != null && isSavingRemovedPage)
            {
                AutoSavePage(CurrentNotebook, oldPage);
            }

            CurrentNotebook.CurrentPage = page;

            CurrentDisplay.Pages.Remove(page);
            var newSelectedPage = CurrentDisplay.Pages.FirstOrDefault();
            SetCurrentPage(newSelectedPage, isSavingRemovedPage);
        }

        #endregion // Display Methods

        #region Page Methods

        public void SetCurrentPage(CLPPage page, bool isSavingOldPage = true)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            var oldPage = CurrentNotebook.CurrentPage;
            CurrentNotebook.CurrentPage = page;

            // TODO: Handle multiDisplays
            //if (CurrentDisplay == null)
            //{
            //    //Take thumbnail of page before navigating away from it.
            //    ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
            //    ACLPPageBaseViewModel.ClearAdorners(CurrentPage);
            //    CurrentPage = page;
            //    return;
            //}

            //CurrentDisplay.AddPageToDisplay(page);

            CurrentPageChanged.SafeInvoke(this);
            if (oldPage != null && isSavingOldPage)
            {
                AutoSavePage(CurrentNotebook, oldPage);
            }
        }

        public void AddPage(Notebook notebook, CLPPage page, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true)
        {
            var pageIndex = notebook.Pages.Count;
            InsertPageAt(notebook, page, pageIndex, isChangingPageNumbers, isAddingNextPageToDisplay);
        }

        public void InsertPageAt(Notebook notebook, CLPPage page, int index, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, int numberOfDifferentiatedPages = -1)
        {
            if (isChangingPageNumbers)
            {
                if (index <= 0)
                {
                    index = 0;
                    page.PageNumber = 1;
                }
                else if (index >= notebook.Pages.Count)
                {
                    index = notebook.Pages.Count;
                    page.PageNumber = notebook.Pages.Last().PageNumber + 1;
                }
                else
                {
                    page.PageNumber = notebook.Pages[index].PageNumber;
                }

                ChangePageNumbersAfterGivenPage(notebook, LoadedNotebooks.ToList(), page.PageNumber - 1, true);
            }
            
            page.ContainerZipFilePath = notebook.ContainerZipFilePath;

            notebook.Pages.Insert(index, page);
            if (isAddingNextPageToDisplay)
            {
                AddPageToCurrentDisplay(page);
            }
            SavePage(notebook, page);

            var entryList = new List<ZipEntrySaver>();
            foreach (var loadedNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID && n.Owner.ID != notebook.Owner.ID))
            {
                var newOwner = loadedNotebook.Owner;

                // Dealing with differentiated pages and a student
                if (numberOfDifferentiatedPages != -1 &&
                    newOwner.IsStudent)
                {
                    // Dealing with a student without a group or a student who's group is larger than the number of differentiated pages
                    if (string.IsNullOrWhiteSpace(newOwner.CurrentDifferentiationGroup) ||
                        string.Compare(newOwner.CurrentDifferentiationGroup, numberOfDifferentiatedPages.ToLetter().ToUpper()) > 0)
                    {
                        if (page.DifferentiationLevel != "A")
                        {
                            continue;
                        }
                    }
                    else if (newOwner.CurrentDifferentiationGroup != page.DifferentiationLevel)
                    {
                        continue;
                    }
                }

                var newPage = page.CopyPageForNewOwner(newOwner);
                newPage.ContainerZipFilePath = notebook.ContainerZipFilePath;
                var previousPage = loadedNotebook.Pages.LastOrDefault(p => p.PageNumber < newPage.PageNumber);
                var insertionIndex = 0;
                if (previousPage != null)
                {
                    var previousIndex = loadedNotebook.Pages.IndexOf(previousPage);
                    insertionIndex = previousIndex + 1;
                }

                loadedNotebook.Pages.Insert(insertionIndex, newPage);
                entryList.Add(new ZipEntrySaver(newPage, loadedNotebook));
            }

            if (!entryList.Any())
            {
                return;
            }

            using (var zip = ZipFile.Read(notebook.ContainerZipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }
        }

        public void DeletePage(Notebook notebook, CLPPage page, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, bool isPreventingEmptyNotebook = true)
        {
            var pageIndex = notebook.Pages.IndexOf(page);
            DeletePageAt(notebook, pageIndex, isChangingPageNumbers, isAddingNextPageToDisplay, isPreventingEmptyNotebook);
        }

        public void DeletePageAt(Notebook notebook, int index, bool isChangingPageNumbers = true, bool isAddingNextPageToDisplay = true, bool isPreventingEmptyNotebook = true)
        {
            if (notebook.Pages.Count <= index ||
                index < 0)
            {
                return;
            }

            var pageToDelete = notebook.Pages[index];
            notebook.Pages.Remove(pageToDelete);
            // TODO: Also delete submissions

            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.RemoveEntry(pageToDelete.GetZipEntryFullPath(notebook));

                foreach (var loadedNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID && n.Owner.ID != notebook.Owner.ID))
                {
                    var otherPageToDelete = loadedNotebook.Pages.FirstOrDefault(p => p.IsEqualByCompositeID(pageToDelete));
                    if (otherPageToDelete == null)
                    {
                        continue;
                    }

                    loadedNotebook.Pages.Remove(otherPageToDelete);
                    zip.RemoveEntry(otherPageToDelete.GetZipEntryFullPath(loadedNotebook));
                    // TODO: Also delete submissions
                }

                zip.Save();
            }

            if (isChangingPageNumbers)
            {
                ChangePageNumbersAfterGivenPage(notebook, LoadedNotebooks.ToList(), index, false);
            }

            if (!notebook.Pages.Any() &&
                isPreventingEmptyNotebook)
            {
                var newPage = new CLPPage(Person.Author)
                              {
                                  PageNumber = notebook.Pages.Any() ? notebook.Pages.First().PageNumber : 1,
                                  ContainerZipFilePath = notebook.ContainerZipFilePath
                              };

                notebook.Pages.Add(newPage);
                SavePage(notebook, newPage);

                var entryList = new List<ZipEntrySaver>();
                foreach (var loadedNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID && n.Owner.ID != notebook.Owner.ID))
                {
                    var newOwner = loadedNotebook.Owner;
                    var generatedNewPage = newPage.CopyPageForNewOwner(newOwner);
                    generatedNewPage.ContainerZipFilePath = notebook.ContainerZipFilePath;
                    loadedNotebook.Pages.Insert(index, generatedNewPage);
                    entryList.Add(new ZipEntrySaver(generatedNewPage, loadedNotebook));
                }

                if (entryList.Any())
                {
                    using (var zip = ZipFile.Read(notebook.ContainerZipFilePath))
                    {
                        zip.CompressionMethod = CompressionMethod.None;
                        zip.CompressionLevel = CompressionLevel.None;
                        zip.UseZip64WhenSaving = Zip64Option.Always;
                        zip.CaseSensitiveRetrieval = true;

                        foreach (var zipEntrySaver in entryList)
                        {
                            zipEntrySaver.UpdateEntry(zip);
                        }

                        zip.Save();
                    }
                }

                AddPageToCurrentDisplay(newPage, false);
                return;
            }

            if (!isAddingNextPageToDisplay)
            {
                return;
            }

            if (index >= notebook.Pages.Count)
            {
                index = notebook.Pages.Count - 1;
            }

            var nextPage = notebook.Pages.ElementAt(index);
            AddPageToCurrentDisplay(nextPage, false);
        }

        public void MovePage(Notebook notebook, CLPPage page, int newPageNumber)
        {
            var currentPageNumber = page.PageNumber;
            if (newPageNumber == currentPageNumber ||
                newPageNumber <= 0)
            {
                return;
            }

            var isIntervalPageNumbersDecreasing = currentPageNumber < newPageNumber;
            var intervalPageNumbersToChange = isIntervalPageNumbersDecreasing
                                                  ? Enumerable.Range(currentPageNumber + 1, newPageNumber - currentPageNumber)
                                                  : Enumerable.Range(newPageNumber, currentPageNumber - newPageNumber);

            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                // Change interval numbers first.
                #region Interval Pages

                foreach (var otherNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID))
                {
                    var pageEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookPagesDirectoryPath);
                    var submissionEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookSubmissionsDirectoryPath);

                    foreach (var pageEntry in pageEntries)
                    {
                        var entryName = pageEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            !intervalPageNumbersToChange.Contains(pageNameComposite.PageNumber))
                        {
                            continue;
                        }

                        var pageToReNumber = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID && p.DifferentiationLevel == pageNameComposite.DifferentiationLevel);
                        if (isIntervalPageNumbersDecreasing)
                        {
                            pageNameComposite.PageNumber--;
                            if (pageToReNumber != null)
                            {
                                pageToReNumber.PageNumber--;
                            }
                        }
                        else
                        {
                            pageNameComposite.PageNumber++;
                            if (pageToReNumber != null)
                            {
                                pageToReNumber.PageNumber++;
                            }
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookPagesDirectoryPath}{newEntryName}.json";
                        pageEntry.FileName = newEntryFullPath;
                    }

                    foreach (var submissionEntry in submissionEntries)
                    {
                        var entryName = submissionEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            !intervalPageNumbersToChange.Contains(pageNameComposite.PageNumber))
                        {
                            continue;
                        }

                        var pageToReNumber = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID && p.DifferentiationLevel == pageNameComposite.DifferentiationLevel);
                        if (isIntervalPageNumbersDecreasing)
                        {
                            pageNameComposite.PageNumber--;
                            if (pageToReNumber != null)
                            {
                                foreach (var submission in pageToReNumber.Submissions)
                                {
                                    submission.PageNumber--;
                                }
                            }
                        }
                        else
                        {
                            pageNameComposite.PageNumber++;
                            if (pageToReNumber != null)
                            {
                                foreach (var submission in pageToReNumber.Submissions)
                                {
                                    submission.PageNumber++;
                                }
                            }
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookSubmissionsDirectoryPath}{newEntryName}.json";
                        submissionEntry.FileName = newEntryFullPath;
                    }
                }

                #endregion // Interval Pages

                // Then change currentPage's page number to new page number.
                #region Moved Page

                foreach (var otherNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID))
                {
                    var pageEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookPagesDirectoryPath);
                    var submissionEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookSubmissionsDirectoryPath);

                    foreach (var pageEntry in pageEntries)
                    {
                        var entryName = pageEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            pageNameComposite.ID != page.ID ||
                            pageNameComposite.PageNumber != currentPageNumber)
                        {
                            continue;
                        }

                        var pageToReNumber = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID && p.DifferentiationLevel == pageNameComposite.DifferentiationLevel);
                        pageNameComposite.PageNumber = newPageNumber;
                        if (pageToReNumber != null)
                        {
                            pageToReNumber.PageNumber = newPageNumber;
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookPagesDirectoryPath}{newEntryName}.json";
                        pageEntry.FileName = newEntryFullPath;
                    }

                    foreach (var submissionEntry in submissionEntries)
                    {
                        var entryName = submissionEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            pageNameComposite.ID != page.ID ||
                            pageNameComposite.PageNumber != currentPageNumber)
                        {
                            continue;
                        }

                        var pageToReNumber = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID && p.DifferentiationLevel == pageNameComposite.DifferentiationLevel);
                        pageNameComposite.PageNumber = newPageNumber;
                        if (pageToReNumber != null)
                        {
                            foreach (var submission in pageToReNumber.Submissions)
                            {
                                submission.PageNumber++;
                            }
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookSubmissionsDirectoryPath}{newEntryName}.json";
                        submissionEntry.FileName = newEntryFullPath;
                    }
                }

                #endregion // Moved Page

                zip.Save();
            }

            // Place moved page in the correct index location of the loaded pages.
            foreach (var loadedNotebook in LoadedNotebooks.Where(n => n.ID == notebook.ID))
            {
                var pageBeforeNewPageNumber = loadedNotebook.Pages.LastOrDefault(p => p.PageNumber < newPageNumber);
                var newIndex = 0;
                if (pageBeforeNewPageNumber != null)
                {
                    newIndex = loadedNotebook.Pages.IndexOf(pageBeforeNewPageNumber);
                    if (!isIntervalPageNumbersDecreasing)
                    {
                        newIndex++;
                    }
                }
                var pagesToMove = loadedNotebook.Pages.Where(p => p.ID == page.ID).ToList();
                foreach (var pageToMove in pagesToMove)
                {
                    var currentIndex = loadedNotebook.Pages.IndexOf(pageToMove);
                    loadedNotebook.Pages.Move(currentIndex, newIndex);
                    if (!isIntervalPageNumbersDecreasing)
                    {
                        newIndex++;
                    }
                }
            }

            AddPageToCurrentDisplay(page, false);
        }

        public void AutoSavePage(Notebook notebook, CLPPage page)
        {
            if (!IsAutoSaveOn)
            {
                return;
            }

            SavePage(notebook, page);
            if (notebook.Owner.ID == Person.AUTHOR_ID)
            {
                var otherNotebooks = LoadedNotebooks.ToList();
                PropogatePageChanges(notebook, otherNotebooks, page);
            }
            //TODO: take screenshot of page if not already cached
            //set LastAutoSaveTime of notebook
            //save page locally, and to export folder
            //save page async to teacher machine, and partial cache folder
        }

        #endregion // Page Methods

        #endregion // Methods

        #endregion // IDataService Implementation

        #region Static Properties

        #region Special Folder Paths

        public static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        public static string DesktopFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string CLPProgramFolderPath => typeof(DataService).Assembly.GetDirectory();

        #endregion // Special Folder Paths

        #region Default Folder Paths

        public static string DefaultCLPDataFolderPath
        {
            get
            {
                var folderPath = Path.Combine(WindowsDriveFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Default Folder Paths

        #endregion // Static Properties

        #region Static Methods

        #region Files

        public static string ValidateFileNameString(string name)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            return invalidFileNameCharacters.Aggregate(name, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static List<FileInfo> GetCLPContainersInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return new List<FileInfo>();
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles("*.clp").ToList();
        }

        public static void CreateEmptyZipContainer(string zipContainerFilePath)
        {
            if (File.Exists(zipContainerFilePath))
            {
                MessageBox.Show(".clp file with that name already exists.");
                return;
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                zip.Save(zipContainerFilePath);
            }
        }

        #endregion // Files

        #region Entry

        public static List<PageZipEntryLoader> GetPageZipEntryLoadersFromEntries(List<ZipEntry> entries)
        {
            var pageZipEntryLoaders = new List<PageZipEntryLoader>();
            foreach (var entry in entries)
            {
                var jsonString = entry.ExtractJsonString();
                var nameCompositeString = entry.GetEntryNameWithoutExtension();
                var pageNameComposite = CLPPage.NameComposite.ParseFromString(nameCompositeString);
                var pageNumber = pageNameComposite.PageNumber;
                var pageZipEntryLoader = new PageZipEntryLoader(jsonString, pageNumber);
                pageZipEntryLoaders.Add(pageZipEntryLoader);
            }

            return pageZipEntryLoaders;
        }

        public static T LoadJsonEntry<T>(string zipContainerFilePath, string entryPath) where T : AInternalZipEntryFile
        {
            try
            {
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var entry = zip.GetEntry(entryPath);
                    var zipEntryFile = entry.ExtractJsonEntity<T>();
                    zipEntryFile.ContainerZipFilePath = zipContainerFilePath;

                    return zipEntryFile;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void SaveAInternalZipEntryFile(AInternalZipEntryFile entryFile, Notebook parentNotebook = null)
        {
            if (!File.Exists(entryFile.ContainerZipFilePath))
            {
                return;
            }

            SaveZipEntry(entryFile.ContainerZipFilePath, new ZipEntrySaver(entryFile, parentNotebook));
        }

        public static void SaveAInternalZipEntryFiles(List<AInternalZipEntryFile> entryFiles, Notebook parentNotebook = null)
        {
            var groupedEntryFiles = entryFiles.GroupBy(e => e.ContainerZipFilePath);

            foreach (var entryFileGroup in groupedEntryFiles)
            {
                var zipContainerFilePath = entryFileGroup.Key;
                if (!File.Exists(zipContainerFilePath))
                {
                    continue;
                }

                var zipEntrySavers = entryFileGroup.Select(entryFile => new ZipEntrySaver(entryFile, parentNotebook)).ToList();
                SaveZipEntries(zipContainerFilePath, zipEntrySavers);
            }
        }

        public static void SaveZipEntry(string zipContainerFilePath, ZipEntrySaver zipEntrySaver)
        {
            SaveZipEntries(zipContainerFilePath,
                           new List<ZipEntrySaver>
                           {
                               zipEntrySaver
                           });
        }

        public static void SaveZipEntries(string zipContainerFilePath, List<ZipEntrySaver> zipEntrySavers)
        {
            try
            {
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in zipEntrySavers)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(zipContainerFilePath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion // Entry

        #region Images

        public static ZipEntry GetImageEntryFromImageHashID(ZipFile zip, string imageHashID)
        {
            try
            {
                var internalImagesDirectory = $"{AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME}/";
                var allImageEntries = zip.GetEntriesInDirectory(internalImagesDirectory).ToList();
                var imageEntry = (from entry in allImageEntries
                                  let hashID = entry.GetEntryNameWithoutExtension().Split(';')[0]
                                  where hashID == imageHashID
                                  select entry).FirstOrDefault();

                return imageEntry;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion // Images

        #region Class Roster

        public static ClassRoster LoadClassRosterFromCLPContainer(string fullFilePath)
        {
            var fileInfo = new FileInfo(fullFilePath);
            return LoadClassRosterFromCLPContainer(fileInfo);
        }

        public static ClassRoster LoadClassRosterFromCLPContainer(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return null;
            }

            if (!fileInfo.Exists ||
                fileInfo.Extension != ".clp")
            {
                return null;
            }

            var entryPath = $"{ClassRoster.DEFAULT_INTERNAL_FILE_NAME}.json";

            return LoadJsonEntry<ClassRoster>(fileInfo.FullName, entryPath);
        }

        public static void SaveClassRoster(ClassRoster classRoster)
        {
            SaveAInternalZipEntryFile(classRoster);
        }

        #endregion // Class Roster

        #region Session

        public static List<Session> LoadAllSessionsFromCLPContainer(string zipContainerFilePath)
        {
            var sessions = new List<Session>();
            try
            {
                var sessionStrings = new List<string>();
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var internalSessionsDirectory = $"{AInternalZipEntryFile.ZIP_SESSIONS_FOLDER_NAME}/";
                    var sessionEntries = zip.GetEntriesInDirectory(internalSessionsDirectory);
                    sessionStrings.AddRange(sessionEntries.Select(sessionEntry => sessionEntry.ExtractJsonString()));
                }

                foreach (var sessionString in sessionStrings)
                {
                    var session = AEntityBase.FromJsonString<Session>(sessionString);
                    session.ContainerZipFilePath = zipContainerFilePath;
                    sessions.Add(session);
                }

                return sessions.OrderByDescending(s => s.StartTime).ToList();
            }
            catch (Exception)
            {
                return sessions;
            }
        }

        public static void SaveSession(Session session)
        {
            SaveAInternalZipEntryFile(session);
        }

        #endregion // Session

        #region Notebook

        public static List<Notebook> LoadAllNotebooksFromCLPContainer(string zipContainerFilePath)
        {
            var notebooks = new List<Notebook>();
            try
            {
                var notebookStrings = new List<string>();
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var notebookEntries = zip.SelectEntries($"*{Notebook.DEFAULT_INTERNAL_FILE_NAME}.json");
                    notebookStrings.AddRange(notebookEntries.Select(notebookEntry => notebookEntry.ExtractJsonString()));
                }

                foreach (var notebookString in notebookStrings)
                {
                    var notebook = AEntityBase.FromJsonString<Notebook>(notebookString);
                    notebook.ContainerZipFilePath = zipContainerFilePath;
                    notebooks.Add(notebook);
                }

                return notebooks;
            }
            catch (Exception)
            {
                return notebooks;
            }
        }

        public static void LoadPagesIntoNotebook(Notebook notebook, List<int> pageNumbers, string overwrittenStartingPageID = "")
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;

            var pageZipEntryLoaders = new List<PageZipEntryLoader>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                List<ZipEntry> pageEntries;
                if (pageNumbers.Any())
                {
                    var pageIDs = GetPageIDsFromPageNumbers(zip, notebook, pageNumbers);
                    pageEntries = GetPageEntriesFromPageIDs(zip, notebook, pageIDs);
                }
                else
                {
                    pageEntries = GetAllPageEntriesInNotebook(zip, notebook);
                }

                var loadedPageNumbers = notebook.Pages.Select(p => p.PageNumber).ToList();
                pageZipEntryLoaders = GetPageZipEntryLoadersFromEntries(pageEntries).Where(zipEntryLoader => !loadedPageNumbers.Contains(zipEntryLoader.PageNumber)).ToList();
            }

            var pages = GetPagesFromPageZipEntryLoaders(pageZipEntryLoaders, zipContainerFilePath).ToList();

            if (owner.IsStudent)
            {
                var submissions = GetSubmissionsForPages(notebook, pages);
                foreach (var submission in submissions)
                {
                    var page = pages.FirstOrDefault(p => p.ID == submission.ID && p.DifferentiationLevel == submission.DifferentiationLevel && p.SubPageNumber == submission.SubPageNumber);
                    if (page != null)
                    {
                        page.Submissions.Add(submission);
                    }
                }
            }

            notebook.Pages.AddRange(pages);
            notebook.Pages = notebook.Pages.OrderBy(p => p.PageNumber).ThenBy(p => p.DifferentiationLevel).ThenBy(p => p.SubPageNumber).ToObservableCollection();
            notebook.CurrentPage = notebook.Pages.FirstOrDefault(p => p.ID == notebook.CurrentPageID) ?? notebook.Pages.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(overwrittenStartingPageID))
            {
                notebook.CurrentPage = notebook.Pages.FirstOrDefault(p => p.ID == overwrittenStartingPageID) ?? notebook.Pages.FirstOrDefault();
            }
        }

        public static void SaveNotebook(Notebook notebook)
        {
            SaveAInternalZipEntryFile(notebook);
        }

        #endregion // Notebook

        #region Page

        public static List<string> GetAllPageIDsInNotebook(ZipFile zip, Notebook notebook)
        {
            var internalPagesDirectory = notebook.NotebookPagesDirectoryPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var pageIDs = pageNameComposites.Select(nc => nc.ID).Distinct().ToList();

            return pageIDs;
        }

        public static List<string> GetPageIDsFromPageNumbers(ZipFile zip, Notebook notebook, List<int> pageNumbers)
        {
            var internalPagesDirectory = notebook.NotebookPagesDirectoryPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var pageIDs = pageNameComposites.Where(nc => pageNumbers.Contains(nc.PageNumber)).Select(nc => nc.ID).Distinct().ToList();

            return pageIDs;
        }

        public static Dictionary<string, int> GetPageNumbersFromPageIDs(ZipFile zip, Notebook notebook, List<string> pageIDs)
        {
            var internalAuthorPagesDirectory = notebook.NotebookPagesDirectoryPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalAuthorPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var mappedIDs = new Dictionary<string, int>();
            foreach (var pageNameComposite in pageNameComposites)
            {
                if (!pageIDs.Contains(pageNameComposite.ID))
                {
                    continue;
                }

                if (!mappedIDs.ContainsKey(pageNameComposite.ID))
                {
                    mappedIDs.Add(pageNameComposite.ID, pageNameComposite.PageNumber);
                }
            }

            return mappedIDs;
        }

        public static List<ZipEntry> GetAllPageEntriesInNotebook(ZipFile zip, Notebook notebook)
        {
            var internalPagesDirectory = notebook.NotebookPagesDirectoryPath;
            var pageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();

            return pageEntries;
        }

        public static List<ZipEntry> GetPageEntriesFromPageIDs(ZipFile zip, Notebook notebook, List<string> pageIDs, bool isSubmissions = false)
        {
            var internalPagesDirectory = isSubmissions ? notebook.NotebookSubmissionsDirectoryPath : notebook.NotebookPagesDirectoryPath;
            var allPageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();
            var pageEntries = (from pageEntry in allPageEntries
                               let nameComposite = CLPPage.NameComposite.ParseFromString(pageEntry.GetEntryNameWithoutExtension())
                               where pageIDs.Contains(nameComposite.ID)
                               select pageEntry).ToList();

            return pageEntries;
        }

        public static List<ZipEntry> GetPageEntriesFromPageNumbers(ZipFile zip, Notebook notebook, List<int> pageNumbers)
        {
            var pageIDs = GetPageIDsFromPageNumbers(zip, notebook, pageNumbers);
            var pageEntries = GetPageEntriesFromPageIDs(zip, notebook, pageIDs);

            return pageEntries;
        }

        public static List<CLPPage> GetPagesFromPageZipEntryLoaders(List<PageZipEntryLoader> pageZipEntryLoaders, string zipContainerFilePath)
        {
            var pages = new List<CLPPage>();
            //foreach (var pageZipEntryLoader in pageZipEntryLoaders)
            //{
            //    var page = AEntityBase.FromJsonString<CLPPage>(pageZipEntryLoader.JsonString);
            //    page.ContainerZipFilePath = zipContainerFilePath;
            //    page.PageNumber = pageZipEntryLoader.PageNumber;
            //    pages.Add(page);
            //}

            Parallel.ForEach(pageZipEntryLoaders,
                             pageZipEntryLoader =>
                             {
                                 var page = AEntityBase.FromJsonString<CLPPage>(pageZipEntryLoader.JsonString);
                                 page.ContainerZipFilePath = zipContainerFilePath;
                                 page.PageNumber = pageZipEntryLoader.PageNumber;
                                 pages.Add(page);
                             });

            return pages;
        }

        public static List<CLPPage> GetSubmissionsForPages(Notebook notebook, List<CLPPage> pages)
        {
            var zipContainerFilePath = notebook.ContainerZipFilePath;

            var pageIDs = pages.Select(p => p.ID).Distinct().ToList();

            var submissionZipEntryLoaders = new List<PageZipEntryLoader>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var submissionEntries = GetPageEntriesFromPageIDs(zip, notebook, pageIDs, true);
                submissionZipEntryLoaders = GetPageZipEntryLoadersFromEntries(submissionEntries);
            }

            var submissions = GetPagesFromPageZipEntryLoaders(submissionZipEntryLoaders, zipContainerFilePath);

            return submissions;
        }

        public static void ChangePageNumbersAfterGivenPage(Notebook notebook, List<Notebook> otherNotebooks, int pageNumber, bool isIncreasing)
        {
            // TODO: Does this handle the Author Notebook correctly?
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                foreach (var otherNotebook in otherNotebooks.Where(n => n.ID == notebook.ID))
                {
                    var pageEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookPagesDirectoryPath);
                    var submissionEntries = zip.GetEntriesInDirectory(otherNotebook.NotebookSubmissionsDirectoryPath);

                    foreach (var pageEntry in pageEntries)
                    {
                        var entryName = pageEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            pageNameComposite.PageNumber <= pageNumber)
                        {
                            continue;
                        }

                        var page = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID);

                        if (isIncreasing)
                        {
                            pageNameComposite.PageNumber++;
                            if (page != null)
                            {
                                page.PageNumber++;
                            }
                        }
                        else
                        {
                            pageNameComposite.PageNumber--;
                            if (page != null)
                            {
                                page.PageNumber--;
                            }
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookPagesDirectoryPath}{newEntryName}.json";
                        pageEntry.FileName = newEntryFullPath;
                    }

                    foreach (var submissionEntry in submissionEntries)
                    {
                        var entryName = submissionEntry.GetEntryNameWithoutExtension();
                        if (string.IsNullOrWhiteSpace(entryName))
                        {
                            continue;
                        }

                        var pageNameComposite = CLPPage.NameComposite.ParseFromString(entryName);
                        if (pageNameComposite == null ||
                            pageNameComposite.PageNumber <= pageNumber)
                        {
                            continue;
                        }

                        var page = otherNotebook.Pages.FirstOrDefault(p => p.ID == pageNameComposite.ID);

                        if (isIncreasing)
                        {
                            pageNameComposite.PageNumber++;
                            if (page != null)
                            {
                                foreach (var submission in page.Submissions)
                                {
                                    submission.PageNumber++;
                                }
                            }
                        }
                        else
                        {
                            pageNameComposite.PageNumber--;
                            if (page != null)
                            {
                                foreach (var submission in page.Submissions)
                                {
                                    submission.PageNumber--;
                                }
                            }
                        }

                        var newEntryName = pageNameComposite.ToNameCompositeString();
                        var newEntryFullPath = $"{otherNotebook.NotebookSubmissionsDirectoryPath}{newEntryName}.json";
                        submissionEntry.FileName = newEntryFullPath;
                    }
                }

                zip.Save();
            }
        }

        public static void SavePage(Notebook notebook, CLPPage page)
        {
            if (notebook == null || 
                page == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(page.ContainerZipFilePath))
            {
                page.ContainerZipFilePath = notebook.ContainerZipFilePath;
            }

            if (!File.Exists(page.ContainerZipFilePath))
            {
                return;
            }

            if (page.Owner.ID == Person.AUTHOR_ID)
            {
                page.History.ClearNonAnimationHistory();
            }

            var entires = new List<ZipEntrySaver>
                          {
                              new ZipEntrySaver(page, notebook),
                              new ZipEntrySaver(notebook, notebook)
                          };

            SaveZipEntries(page.ContainerZipFilePath, entires);
        }

        public static void PropogatePageChanges(Notebook authorNotebook, List<Notebook> otherNotebooks, CLPPage authorPage)
        {
            var entires = new List<ZipEntrySaver>();

            foreach (var otherNotebook in otherNotebooks.Where(n => n.ID == authorNotebook.ID && n.Owner.ID != Person.AUTHOR_ID))
            {
                var page = otherNotebook.Pages.FirstOrDefault(p => p.ID == authorPage.ID);
                if (page == null)
                {
                    continue;
                }

                PropogatePageProperties(authorPage, page);
                PropogatePageAuthoredPageObjects(authorPage, page);
                PropogatePageAuthoredInk(authorPage, page);
                PropogatePageTags(authorPage, page);

                entires.Add(new ZipEntrySaver(page, otherNotebook));
            }

            SaveZipEntries(authorPage.ContainerZipFilePath, entires);
        }

        public static void PropogatePageProperties(CLPPage authorPage, CLPPage otherPage)
        {
            otherPage.PageType = authorPage.PageType;
            otherPage.Height = authorPage.Height;
            otherPage.Width = authorPage.Width;
            otherPage.InitialAspectRatio = authorPage.InitialAspectRatio;
            otherPage.PageType = authorPage.PageType;
            otherPage.PageLineLength = authorPage.PageLineLength;
        }

        public static void PropogatePageAuthoredPageObjects(CLPPage authorPage, CLPPage otherPage)
        {
            var pageObjectsToDelete = otherPage.PageObjects.Where(p => p.CreatorID == Person.AUTHOR_ID).ToList();
            foreach (var pageObject in pageObjectsToDelete)
            {
                otherPage.PageObjects.Remove(pageObject);
            }

            var pageObjectsToAdd = authorPage.PageObjects.Where(p => p.CreatorID == Person.AUTHOR_ID).ToList();
            foreach (var pageObject in pageObjectsToAdd)
            {
                otherPage.PageObjects.Insert(0, pageObject);
            }
        }

        public static void PropogatePageAuthoredInk(CLPPage authorPage, CLPPage otherPage)
        {
            var strokesToDelete = otherPage.InkStrokes.Where(s => s.GetStrokeOwnerID() == Person.AUTHOR_ID).ToList();
            foreach (var stroke in strokesToDelete)
            {
                otherPage.InkStrokes.Remove(stroke);
            }

            var strokesToAdd = authorPage.InkStrokes.Where(s => s.GetStrokeOwnerID() == Person.AUTHOR_ID).ToList();
            foreach (var stroke in strokesToAdd)
            {
                otherPage.InkStrokes.Insert(0, stroke);
            }
        }

        public static void PropogatePageTags(CLPPage authorPage, CLPPage otherPage)
        {
            // TODO
        }

        public static void SavePagesInSameNotebook(Notebook notebook, List<CLPPage> pages)
        {
            if (!File.Exists(notebook.ContainerZipFilePath))
            {
                return;
            }

            var entries = pages.Select(page => new ZipEntrySaver(page, notebook)).ToList();
            entries.Add(new ZipEntrySaver(notebook, notebook));

            SaveZipEntries(notebook.ContainerZipFilePath, entries);
        }

        #endregion // Page

        #endregion // Static Methods

        #region OBSOLETE

        public List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel)
        {
            var submissions = new List<CLPPage>();
            //foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.NameComposite.ID == notebookID && n.Notebook.Owner.IsStudent))
            //{
            //    var pageSubmissions = notebookInfo.Notebook.Pages.Where(p => p.ID == pageID && p.DifferentiationLevel == differentiationLevel && p.VersionIndex == 0).Select(p => p.Submissions).ToList();
            //    foreach (var pageSubmission in pageSubmissions)
            //    {
            //        submissions.AddRange(pageSubmission);
            //    }
            //}

            return submissions;
        }

        #endregion // OBSOLETE

        #region Tests

        private void ConvertEmilyCache()
        {
            var dirInfo = new DirectoryInfo(ConversionService.EmilyNotebooksFolder);
            var notebooks = new List<Notebook>();
            Notebook authorNotebook = null;
            var students = new List<Person>();
            Person teacher = null;
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var notebookFolder = directory.FullName;
                CLogger.AppendToLog($"Notebook Folder: {notebookFolder}");
                var notebook = ConversionService.ConvertCacheEmilyNotebook(notebookFolder);
                notebooks.Add(notebook);

                if (notebook.OwnerID == Person.AUTHOR_ID)
                {
                    authorNotebook = notebook;
                }

                if (notebook.Owner.IsStudent)
                {
                    students.Add(notebook.Owner);
                }
                else
                {
                    teacher = notebook.Owner;
                }
            }

            ConversionService.SaveNotebooksToZip(ConversionService.EmilyZipFilePath, notebooks);

            //var subjectFilePath = Path.Combine(ConversionService.AnnClassesFolder, "subject;L6xDfDuP-kCMBjQ3-HdAPQ.xml");
            var subjectFilePath = Path.Combine(ConversionService.EmilyClassesFolder, "subject;AAAAABERAAAAAAAAAAAAAQ.xml");
            var classRoster = ConversionService.ConvertCacheEmilyClassSubject(subjectFilePath, authorNotebook);
            classRoster.ListOfTeachers.Clear();
            classRoster.ListOfTeachers.Add(teacher);
            classRoster.ListOfStudents.Clear();
            classRoster.ListOfStudents.AddRange(students);

            ConversionService.SaveClassRosterToZip(ConversionService.EmilyZipFilePath, classRoster);

            var classesDirInfo = new DirectoryInfo(ConversionService.EmilyClassesFolder);
            var sessions = classesDirInfo.EnumerateFiles("period;*.xml").Select(file => file.FullName).Select(ConversionService.ConvertCacheEmilyClassPeriod).OrderBy(s => s.StartTime).ToList();
            var i = 1;
            foreach (var session in sessions)
            {
                session.SessionTitle = $"Class {i}";
                i++;
            }

            ConversionService.SaveSessionsToZip(ConversionService.EmilyZipFilePath, sessions, authorNotebook);
        }

        #endregion // Tests
    }
}