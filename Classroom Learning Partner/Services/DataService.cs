﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel;
using Catel.Reflection;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{

    #region Info Classes

    public class CacheInfo
    {
        public CacheInfo(string cacheFolderPath) { CacheFolderPath = cacheFolderPath; }

        public string CacheFolderPath { get; private set; }

        public string CacheName
        {
            get
            {
                var directoryInfo = new DirectoryInfo(CacheFolderPath);
                return string.Join(" ", directoryInfo.Name.Split('.').Where(s => !s.ToLower().Equals("cache")));
            }
        }

        public string ClassesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Classes"); }
        }

        public string ImagesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Images"); }
        }

        public string NotebooksFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Notebooks"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            if (!Directory.Exists(ClassesFolderPath))
            {
                Directory.CreateDirectory(ClassesFolderPath);
            }

            if (!Directory.Exists(ImagesFolderPath))
            {
                Directory.CreateDirectory(ImagesFolderPath);
            }

            if (!Directory.Exists(NotebooksFolderPath))
            {
                Directory.CreateDirectory(NotebooksFolderPath);
            }
        }
    }

    public class NotebookInfo
    {
        public NotebookInfo(CacheInfo cache, string notebookFolderPath)
        {
            Cache = cache;
            NotebookFolderPath = notebookFolderPath;
        }

        public CacheInfo Cache { get; set; }

        public string NotebookFolderPath { get; private set; }

        public Notebook Notebook { get; set; }

        public List<CLPPage> Pages { get; set; } //TODO: Rename to something like NetworkLoadedPages to avoid confusion.

        public string DisplaysFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Displays"); }
        }

        public string PagesFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Pages"); }
        }

        public string NotebookFilePath
        {
            get { return Path.Combine(NotebookFolderPath, "notebook.xml"); }
        }

        public string LastSavedTime
        {
            get { return !File.Exists(NotebookFilePath) ? string.Empty : File.GetLastWriteTime(NotebookFilePath).ToString("MM/dd/yy HH:mm:ss"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(NotebookFolderPath))
            {
                Directory.CreateDirectory(NotebookFolderPath);
            }

            if (!Directory.Exists(DisplaysFolderPath))
            {
                Directory.CreateDirectory(DisplaysFolderPath);
            }

            if (!Directory.Exists(PagesFolderPath))
            {
                Directory.CreateDirectory(PagesFolderPath);
            }
        }
    }

    #endregion //Info Classes

    public class DataService : IDataService
    {
        #region Constants

        private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLPData";
        private const string DEFAULT_CACHE_FOLDER_NAME = "Cache";
        private const string DEFAULT_TEMP_CACHE_FOLDER_NAME = "TempCache";
        private const string DEFAULT_CONFIG_FOLDER_NAME = "Config";  // Config Service?
        private const string DEFAULT_ARCHIVE_FOLDER_NAME = "Archive";
        private const string DEFAULT_LOGS_FOLDER_NAME = "Logs";

        #endregion // Constants

        public DataService()
        {
            CurrentCLPDataFolderPath = DefaultCLPDataFolderPath;
        }

        #region Static Properties

        #region Special Folder Paths

        public static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        public static string DesktopFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string CLPProgramFolderPath => typeof(MainWindowView).Assembly.GetDirectory();

        #endregion // Special Folder Paths

        #region Default Folder Paths

        public static string DefaultCLPDataFolderPath
        {
            get
            {
                var folderPath = Path.Combine(DesktopFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);  // TODO: Change to WindowsDriveFolderPath
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Default Folder Paths

        #endregion // Static Properties

        #region Methods

        private void GeneratePageNumbers(Notebook notebook)
        {
            var initialPageNumber = notebook.Pages.Any() ? notebook.Pages.First().PageNumber - 1 : 0;
            CLPPage lastPage = null;
            foreach (var page in notebook.Pages)
            {
                if (lastPage == null ||
                    page.ID != lastPage.ID)
                {
                    initialPageNumber++;
                }
                if (page.PageNumber != 999) // TODO: less stupid special case for exit tickets?
                {
                    page.PageNumber = initialPageNumber;
                }
                lastPage = page;
            }

            // TODO: Optimize into Increase/Decrease pageNumber after this page number?
        }

        private void AddEntryAndSave(string zipFilePath, string entryDirectory, string entryName, string entryContents)
        {
            entryName = ZipExtensions.CombineEntryDirectoryAndName(entryDirectory, entryName);
            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.AddEntry(entryName, entryContents);
                zip.Save();
            }
        }

        #endregion // Methods

        #region Static Methods

        public static string ValidateFileNameString(string name)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            return invalidFileNameCharacters.Aggregate(name, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static List<FileInfo> GetZipContainersInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return new List<FileInfo>();
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles("*.clp").ToList();
        }

        public static ClassRoster LoadClassRosterFromZipContainer(FileInfo fileInfo)
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

            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                if (!zip.ContainsEntry("classRoster.json"))
                {
                    return null;
                }

                var rosterEntry = zip.Entries.First(e => e.FileName == "classRoster.json");

                using (var memoryStream = new MemoryStream())
                {
                    rosterEntry.Extract(memoryStream);

                    var jsonString = Encoding.ASCII.GetString(memoryStream.ToArray());
                    return AEntityBase.FromJsonString<ClassRoster>(jsonString);
                }
            }
        }

        #endregion // Static Methods

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

        #region Program Properties

        /// <summary>ImagePool for the current CLP instance, populated by all open notebooks.</summary>
        public Dictionary<string, BitmapImage> ImagePool { get; } = new Dictionary<string, BitmapImage>();

        #endregion // Program Properties

        #region Cache Properties

        public List<FileInfo> AvailableZipContainerFileInfos
        {
            get
            {
                // ReSharper disable once ConvertPropertyToExpressionBody
                return GetZipContainersInFolder(CurrentCacheFolderPath);
            }
        }

        #endregion // Cache Properties

        #region Notebook Properties

        //FilePathPair
        //AvailableNotebookSets   
        //LoadedNotebookSets      //Differentiate between a .clp file and the individual notebooks within?
        //CurrentNotebookSet

        public NotebookSet CurrentNotebookSet { get; private set; }
        public ClassRoster CurrentClassRoster { get; private set; }
        public Notebook CurrentNotebook { get; private set; }
        public CLPPage CurrentPage { get; private set; }

        //CurrentMultiDisplay

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
        public event EventHandler<EventArgs> CurrentPageChanged;

        #endregion // Events

        #region Methods

        #region ClassRoster Methods

        public void SetCurrentClassRoster(ClassRoster classRoster)
        {
            CurrentClassRoster = classRoster;
            CurrentClassRosterChanged.SafeInvoke(this);
        }

        #endregion // ClassRoster Methods

        #region Notebook Methods

        public void CreateAuthorNotebook(string notebookName)
        {
            var notebook = new Notebook(notebookName, Person.Author);
            SetCurrentNotebook(notebook);
            AddPage(notebook, new CLPPage(Person.Author));
        }

        public void SetCurrentNotebook(Notebook notebook)
        {
            CurrentNotebook = notebook;
            CurrentNotebookChanged.SafeInvoke(this);
        }

        #endregion // Notebook Methods

        #region Page Methods

        public void SetCurrentPage(CLPPage page)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            var oldPage = CurrentNotebook.CurrentPage;
            ACLPPageBaseViewModel.ClearAdorners(oldPage);
            AutoSavePage(oldPage);
            CurrentNotebook.CurrentPage = page;
            CurrentPage = page;

            // TODO: Handle multiDisplays
            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if (notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //if (notebookWorkspaceViewModel.CurrentDisplay == null)
            //{
            //    //Take thumbnail of page before navigating away from it.
            //    ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
            //    ACLPPageBaseViewModel.ClearAdorners(CurrentPage);
            //    CurrentPage = page;
            //    return;
            //}

            //notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);


            CurrentPageChanged.SafeInvoke(this);
        }

        public void AddPage(Notebook notebook, CLPPage page)
        {
            page.PageNumber = notebook.Pages.Any() ? notebook.Pages.Last().PageNumber + 1 : 1;
            notebook.Pages.Add(page);
            SetCurrentPage(page);
        }

        public void InsertPageAt(Notebook notebook, CLPPage page, int index)
        {
            notebook.Pages.Insert(index, page);
            GeneratePageNumbers(notebook);
            SetCurrentPage(page);
        }

        public void DeletePage(Notebook notebook, CLPPage page)
        {
            var pageIndex = notebook.Pages.IndexOf(page);
            DeletePageAt(notebook, pageIndex);
        }

        public void DeletePageAt(Notebook notebook, int index)
        {
            //TODO: Delete page from notebook
            //delete page's json
            //renumber existing pages
            //function with full cache

            if (notebook.Pages.Count <= index ||
                index < 0)
            {
                return;
            }

            if (notebook.Pages.Count == 1)
            {
                var newPage = new CLPPage(Person.Author)
                {
                    PageNumber = notebook.Pages.Any() ? notebook.Pages.First().PageNumber : 1
                };

                notebook.Pages.Add(newPage);
            }

            int newIndex;
            if (index + 1 < notebook.Pages.Count)
            {
                newIndex = index + 1;
            }
            else
            {
                newIndex = index - 1;
            }

            var nextPage = notebook.Pages.ElementAt(newIndex);
            CurrentPage = nextPage;
            if (index == 0)
            {
                CurrentPage.PageNumber = notebook.Pages.First().PageNumber;
            }

            //_trashedPages.Add(notebook.Pages[index]);
            notebook.Pages.RemoveAt(index);
            GeneratePageNumbers(notebook);
        }

        public void AutoSavePage(CLPPage page)
        {
            //TODO: take screenshot of page if not already cached
            //set LastAutoSaveTime of notebook
            //save page locally, and to export folder
            //save page async to teacher machine, and partial cache folder
        }

        #endregion // Page Methods

        #region Display Methods

        public void AddDisplay(Notebook notebook, IDisplay display)
        {
            display.NotebookID = notebook.ID;
            display.DisplayNumber = notebook.Displays.Any(d => d.GetType() == display.GetType()) ? notebook.Displays.Last().DisplayNumber + 1 : 1;
            notebook.Displays.Add(display);
        }

        #endregion // Display Methods

        #region Save Methods

        private class ZipEntrySaver
        {
            public ZipEntrySaver(AInternalZipEntryFile entryFile, string parentNotebookName = "")
            {
                EntryFile = entryFile;
                InternalFilePath = entryFile.GetFullInternalFilePathWithExtension(parentNotebookName);
                JsonString = entryFile.ToJsonString();
            }

            public AInternalZipEntryFile EntryFile { get; set; }
            private string InternalFilePath { get; set; }
            private string JsonString { get; set; }

            public void UpdateEntry(ZipFile zip)
            {
                zip.UpdateEntry(InternalFilePath, JsonString);
            }
        }

        public void SaveLocal()
        {
            var cacheFolderPath = CurrentCacheFolderPath;
            var fileName = $"{ValidateFileNameString(CurrentClassRoster.DefaultContainerFileName)}.{AInternalZipEntryFile.CONTAINER_EXTENSION}";
            var fullFilePath = Path.Combine(cacheFolderPath, fileName);

            var parentNotebookName = CurrentNotebook.InternalZipFileDirectoryName;
            var entryList = new List<ZipEntrySaver>
                            {
                                new ZipEntrySaver(CurrentClassRoster, parentNotebookName),
                                new ZipEntrySaver(CurrentNotebook, parentNotebookName)
                            };

            foreach (var page in CurrentNotebook.Pages)
            {
                entryList.Add(new ZipEntrySaver(page, parentNotebookName));
            }

            if (File.Exists(fullFilePath))
            {
                //var readOptions = new ReadOptions
                //                  {
                //                      ReadProgress = Zip_ReadProgress
                //                  };

                //var zip = ZipFile.Read(fullFilePath, readOptions)

                using (var zip = ZipFile.Read(fullFilePath))
                {
                    // TODO: Test if needed. Won't work unless zip has been saved.
                    // Implied that entries are not added to zip.Entries until saved. Need to verify. Code definitely says added to internal _entries before save, so test this
                    //zip.SelectEntries("*.json");
                    //zip.SelectEntries("p;*.json", "blah/blah/pages/"); test this.

                    //zip.UpdateFile only applies to adding a file from the disc to the zip archive, N/A for clp unless we need it for images?
                    //          for images, probably zip.AddEntry(entryPath, memoryStream); also have byte[] byteArray for content

                    // Change all AddEntry to UpdateEntry

                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;  Only one that seems persistent, but need to test
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(fullFilePath);
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

                    zip.Save(fullFilePath);
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

                // TODO: Always auto save. Save as soon as creation in temp folder then move to normal cache if saved. On start, clear temp folder?

                var newFileName = $"{imageHashID};{Path.GetFileNameWithoutExtension(imageFilePath)}{Path.GetExtension(imageFilePath)}";
                var internalFilePath = ZipExtensions.CombineEntryDirectoryAndName(AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME, newFileName);
                var containerZipFilePath = page.ContainerZipFilePath;


                var newFilePath = Path.Combine(CurrentCacheInfo.ImagesFolderPath, newFileName);

                try
                {
                    File.Copy(imageFilePath, newFilePath);
                }
                catch (IOException)
                {
                    MessageBox.Show("Image already in ImagePool, using ImagePool instead.");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong copying the image to the ImagePool. See Error Log.");
                    Logger.Instance.WriteToLog("[IMAGEPOOL ERROR]: " + e.Message);
                    return null;
                }

                var bitmapImage = CLPImage.GetImageFromPath(newFilePath);
                if (bitmapImage == null)
                {
                    MessageBox.Show("Failed to load image from ImageCache by fileName.");
                    return null;
                }

                ImagePool.Add(imageHashID, bitmapImage);


            }
            catch (Exception)
            {
                MessageBox.Show("Error opening image file. Please try again.");
            }

            return null;
        }

        #endregion // Save Methods

        #region Load Methods

        public BitmapImage GetImage(string imageHashID, IPageObject pageObject)
        {
            if (ImagePool.ContainsKey(imageHashID))
            {
                return ImagePool[imageHashID];
            }
            return null;
            //var filePath = string.Empty;
            //var imageFilePaths = Directory.EnumerateFiles(dataService.CurrentCacheInfo.ImagesFolderPath);
            //foreach (var imageFilePath in from imageFilePath in imageFilePaths
            //                              let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
            //                              where imageHashID == image.ImageHashID
            //                              select imageFilePath)
            //{
            //    filePath = imageFilePath;
            //    break;
            //}

            //var bitmapImage = CLPImage.GetImageFromPath(filePath);
            //if (bitmapImage != null)
            //{
            //    SourceImage = bitmapImage;
            //    App.MainWindowViewModel.ImagePool.Add(image.ImageHashID, bitmapImage);
            //}
        }

        #endregion // Load Methods

        #endregion // Methods

        #endregion // IDataService Implementation

        #region Testing

        public void CreateTestNotebookSet()
        {
            var cacheFolderPath = CurrentCacheFolderPath;
            var fileName = "Test Notebook.clp";
            var fullFilePath = Path.Combine(cacheFolderPath, fileName);

            var classRoster = new ClassRoster
            {
                SubjectName = "Math",
                GradeLevel = "3",
                SchoolName = "Northeastern",
                City = "Waltham",
                State = "Massachusetts"
            };
            var teacher = new Person
            {
                FirstName = "Ann",
                Nickname = "Mrs.",
                LastName = "McNamara",
                IsStudent = false
            };

            var student1 = new Person
            {
                FirstName = "Steve",
                LastName = "Chapman",
                IsStudent = true
            };

            var student2 = new Person
            {
                FirstName = "Lily",
                LastName = "Ko",
                IsStudent = true
            };

            var student3 = new Person
            {
                FirstName = "Kimberle",
                LastName = "Koile",
                IsStudent = true
            };

            classRoster.ListOfTeachers.Add(teacher);

            classRoster.ListOfStudents.Add(student1);
            classRoster.ListOfStudents.Add(student2);
            classRoster.ListOfStudents.Add(student3);

            var rosterString = classRoster.ToJsonString(true);

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;

                zip.AddDirectoryByName("sessions");
                zip.AddDirectoryByName("images");
                zip.AddDirectoryByName("notebooks");

                zip.AddEntry("classRoster.json", rosterString);
                zip.AddEntry("testing/testRoster.json", rosterString);

                zip.Save(fullFilePath);
            }

            Console.WriteLine("Stopping Point");

            using (var zip = ZipFile.Read(fullFilePath))
            {
                foreach (var zipEntryFileName in zip.EntryFileNames)
                {
                    Console.WriteLine(zipEntryFileName);
                }

                var e = zip.GetEntryByNameInDirectory("testing/", "testRoster.json");
                e.MoveEntry(null);
                zip.Save();
            }

            Console.WriteLine("Stopping Point");
        }

        #endregion // Testing

        #region OBSOLETE

        #region Properties

        #region Cache Properties

        public CacheInfo CurrentCacheInfo { get; set; }

        #endregion //Cache Properties

        #region Notebook Properties

        public List<NotebookInfo> LoadedNotebooksInfo { get; } = new List<NotebookInfo>();

        public NotebookInfo CurrentNotebookInfo { get; set; }

        #endregion //Notebook Properties

        #endregion //Properties

        #region Methods

        #region Static Methods

        public static List<CacheInfo> GetCachesInFolder(string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return new List<CacheInfo>();
            }

            var directoryInfo = new DirectoryInfo(cachesFolderPath);
            return directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => new CacheInfo(directory.FullName)).OrderBy(c => c.CacheName).ToList();
        }

        public static List<NotebookInfo> GetNotebooksInCache(CacheInfo cache)
        {
            if (cache == null ||
                !Directory.Exists(cache.NotebooksFolderPath))
            {
                return new List<NotebookInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.NotebooksFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => new NotebookInfo(cache, directory.FullName))
                             .Where(nc => nc != null)
                             //.OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                             //.ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                             //.ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                             //.ThenBy(nc => nc.NameComposite.OwnerName)
                             .ToList();
        }

        public static List<string> GetAllPageIDsInNotebook(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new ConcurrentBag<string>();

            //Parallel.ForEach(pageFilePaths,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex != "0")
            //                     {
            //                         return;
            //                     }

            //                     pageIDs.Add(pageNameComposite.ID);
            //                 });

            return pageIDs.Distinct().ToList();
        }

        public static List<string> GetPageIDsFromPageNumbers(NotebookInfo notebookInfo, List<int> pageNumbers)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new ConcurrentBag<string>();

            //Parallel.ForEach(pageFilePaths,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex != "0")
            //                     {
            //                         return;
            //                     }

            //                     int pageNumber;
            //                     var isPageNumber = int.TryParse(pageNameComposite.PageNumber, out pageNumber);
            //                     if (!isPageNumber)
            //                     {
            //                         return;
            //                     }

            //                     var isPageToBeLoaded = pageNumbers.Contains(pageNumber);
            //                     if (!isPageToBeLoaded)
            //                     {
            //                         return;
            //                     }

            //                     pageIDs.Add(pageNameComposite.ID);
            //                 });

            return pageIDs.Distinct().ToList();
        }

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

        public static List<CLPPage> LoadOwnSubmissionsForLoadedPages(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            return LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePaths);
        }

        public static List<CLPPage> LoadGivenSubmissionsForLoadedPages(NotebookInfo notebookInfo, List<string> pageFilePathsToCheck)
        {
            var submissions = new ConcurrentBag<CLPPage>();

            //Parallel.ForEach(pageFilePathsToCheck,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex == "0")
            //                     {
            //                         return;
            //                     }

            //                     var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
            //                     if (!isPageToBeLoaded)
            //                     {
            //                         return;
            //                     }

            //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //                     if (page == null)
            //                     {
            //                         return;
            //                     }

            //                     submissions.Add(page);
            //                 });

            return submissions.ToList();
        }

        public static List<CLPPage> GetPagesToSave(NotebookInfo notebookInfo, bool isForcedFullSave)
        {
            var pagesToSave = new List<CLPPage>();
            //foreach (var page in notebookInfo.Notebook.Pages)
            //{
            //    if (isForcedFullSave || !page.IsCached)
            //    {
            //        pagesToSave.Add(page);
            //    }
            //    pagesToSave.AddRange(page.Submissions.Where(s => isForcedFullSave || !s.IsCached));
            //}

            return pagesToSave;
        }

        #endregion //Static Methods

        #region Load Methods

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true)
        {
            //// Is Notebook already loaded in memory?
            //var loadedNotebooks = LoadedNotebooksInfo.ToList();
            //var existingNotebookInfo = loadedNotebooks.FirstOrDefault(n => n.NameComposite.ToFolderName() == notebookInfo.NameComposite.ToFolderName());
            //if (existingNotebookInfo != null)
            //{
            //    if (isForcedOpen)
            //    {
            //        LoadedNotebooksInfo.Remove(existingNotebookInfo);
            //    }
            //    else
            //    {
            //        if (CurrentNotebookInfo == existingNotebookInfo)
            //        {
            //            App.MainWindowViewModel.IsBackStageVisible = false;
            //        }
            //        else if (isSetToNotebookCurrentNotebook)
            //        {
            //            SetCurrentNotebook(existingNotebookInfo);
            //        }
            //        return;
            //    }
            //}

            //// Guarantee folder structure.
            //notebookInfo.Cache.Initialize();
            //notebookInfo.Initialize();

            //// Is Notebook included in notebookInfo (e.g. send across the network instead of being loaded from the disk).
            //if (notebookInfo.Notebook != null)
            //{
            //    LoadedNotebooksInfo.Add(notebookInfo);
            //    if (isSetToNotebookCurrentNotebook)
            //    {
            //        SetCurrentNotebook(notebookInfo);
            //    }
            //    return;
            //}

            //// Load Notebook from disk.
            //var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFolderPath);
            //if (notebook == null)
            //{
            //    MessageBox.Show("Notebook couldn't be loaded.");
            //    return;
            //}

            //notebookInfo.Notebook = notebook;

            //LoadedNotebooksInfo.Add(notebookInfo);
            //if (isSetToNotebookCurrentNotebook)
            //{
            //    SetCurrentNotebook(notebookInfo);
            //}
        }

        public void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced)
        {
            //if (notebookInfo.Notebook == null)
            //{
            //    return;
            //}

            //if (isExistingPagesReplaced)
            //{
            //    notebookInfo.Notebook.Pages.Clear();
            //    notebookInfo.Notebook.CurrentPage = null;
            //}

            //var pagesToLoad = new List<CLPPage>();

            //if (notebookInfo.Pages != null &&
            //    notebookInfo.Pages.Any()) // Load pages included in notebookInfo (e.g. ones sent across the network).
            //{
            //    pagesToLoad = notebookInfo.Pages;
            //}
            //else // Load local pages.
            //{
            //    var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            //    foreach (var pageFilePath in pageFilePaths)
            //    {
            //        var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //        if (pageNameComposite == null ||
            //            pageNameComposite.VersionIndex != "0")
            //        {
            //            continue;
            //        }

            //        var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
            //        if (!isPageToBeLoaded)
            //        {
            //            continue;
            //        }

            //        var page = CLPPage.LoadFromXML(pageFilePath);
            //        if (page == null)
            //        {
            //            continue;
            //        }

            //        pagesToLoad.Add(page);
            //    }

            //    // BUG: Parellel calls invoke threading errors.
            //    //Parallel.ForEach(pageFilePaths,
            //    //                 pageFilePath =>
            //    //                 {
            //    //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //    //                     if (pageNameComposite == null ||
            //    //                         pageNameComposite.VersionIndex != "0")
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
            //    //                     if (!isPageToBeLoaded)
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //    //                     if (page == null)
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     pagesToLoad.Add(page);
            //    //                 });
            //}

            //foreach (var page in pagesToLoad)
            //{
            //    var index = notebookInfo.Notebook.Pages.ToList().BinarySearch(page, new PageNumberComparer());
            //    if (index < 0)
            //    {
            //        index = ~index;
            //    }
            //    notebookInfo.Notebook.Pages.Insert(index, page);

            //    if (notebookInfo.Notebook.CurrentPageID == page.ID &&
            //        notebookInfo.Notebook.CurrentPageOwnerID == page.OwnerID &&
            //        notebookInfo.Notebook.CurrentPageVersionIndex == page.VersionIndex)
            //    {
            //        notebookInfo.Notebook.CurrentPage = page;
            //    }
            //}

            //if (notebookInfo.Notebook.CurrentPage == null)
            //{
            //    notebookInfo.Notebook.CurrentPage = notebookInfo.Notebook.Pages.FirstOrDefault();
            //}

            //// Load submissions from disk.
            ////if ((App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher && App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector) ||
            ////    notebookInfo.Notebook.Owner.ID == Person.Author.ID ||
            ////    notebookInfo.Notebook.Owner.IsStudent) // Load student's own submission history.
            ////{
            ////    var submissions = LoadOwnSubmissionsForLoadedPages(notebookInfo);

            ////    foreach (var page in notebookInfo.Notebook.Pages)
            ////    {
            ////        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(p => p.ID == page.ID).OrderBy(p => p.VersionIndex).ToList());
            ////    }
            ////}
            ////else // Load all student submissions for Teacher Notebook.
            ////{
            ////    var notebookInfos = GetNotebooksInCache(notebookInfo.Cache).Where(n => n.NameComposite.ID == notebookInfo.Notebook.ID && n.NameComposite.OwnerTypeTag == "S");
            ////    var pageFilePathsToCheck = new List<string>();

            ////    foreach (var info in notebookInfos)
            ////    {
            ////        pageFilePathsToCheck.AddRange(Directory.EnumerateFiles(info.PagesFolderPath, "*.xml").ToList());
            ////    }

            ////    var submissions = LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePathsToCheck);

            ////    foreach (var page in notebookInfo.Notebook.Pages)
            ////    {
            ////        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            ////    }
            ////}
        }

        public void LoadLocalSubmissions(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (isExistingPagesReplaced)
            {
                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions.Clear();
                }
            }

            var submissions = new List<CLPPage>();

            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            foreach (var pageFilePath in pageFilePaths)
            {
                //var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                //if (pageNameComposite == null ||
                //    pageNameComposite.VersionIndex == "0")
                //{
                //    continue;
                //}

                //var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                //if (!isPageToBeLoaded)
                //{
                //    continue;
                //}

                //var page = CLPPage.LoadFromXML(pageFilePath);
                //if (page == null)
                //{
                //    continue;
                //}

                //submissions.Add(page);
            }

            foreach (var page in notebookInfo.Notebook.Pages)
            {
                page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            }

            // BUG: Parellel calls invoke threading errors. see concurrentBag, or PLINQ AsParallel()
            //Parallel.ForEach(pageFilePaths,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex == "0")
            //                     {
            //                         return;
            //                     }

            //                     var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
            //                     if (!isPageToBeLoaded)
            //                     {
            //                         return;
            //                     }

            //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //                     if (page == null)
            //                     {
            //                         return;
            //                     }

            //                     submissions.Add(page);
            //                 });

            //Parallel.ForEach(notebookInfo.Notebook.Pages,
            //                 page =>
            //                 {
            //                     page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            //                 });
        }

        #endregion //Load Methods

        public void SetCurrentNotebook(NotebookInfo notebookInfo)
        {
            //CurrentNotebookInfo = notebookInfo;

            //App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            //App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            //App.MainWindowViewModel.CurrentNotebookName = CurrentNotebookInfo.Notebook.Name;
            //App.MainWindowViewModel.CurrentUser = CurrentNotebookInfo.Notebook.Owner;
            //App.MainWindowViewModel.IsAuthoring = CurrentNotebookInfo.Notebook.OwnerID == Person.Author.ID;
            //App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #region Old ClassPeriod Methods

        //public void StartSoonestClassPeriod(string localCacheFolderPath)
        //{
        //    var classesFolderPath = Path.Combine(localCacheFolderPath, "Classes");
        //    var classPeriodFilePaths = Directory.GetFiles(classesFolderPath);
        //    ClassPeriodNameComposite closestClassPeriodNameComposite = null;
        //    var closestTimeSpan = TimeSpan.MaxValue;
        //    var now = DateTime.Now;
        //    foreach (var classPeriodFilePath in classPeriodFilePaths)
        //    {
        //        var classPeriodNameComposite = ClassPeriodNameComposite.ParseFilePath(classPeriodFilePath);
        //        if (classPeriodNameComposite == null)
        //        {
        //            continue;
        //        }
        //        var time = classPeriodNameComposite.StartTime;
        //        var timeParts = time.Split('.');
        //        var year = Int32.Parse(timeParts[0]);
        //        var month = Int32.Parse(timeParts[1]);
        //        var day = Int32.Parse(timeParts[2]);
        //        var hour = Int32.Parse(timeParts[3]);
        //        var minute = Int32.Parse(timeParts[4]);
        //        var dateTime = new DateTime(year, month, day, hour, minute, 0);

        //        var timeSpan = now - dateTime;
        //        var duration = timeSpan.Duration();
        //        var closestTimeSpanDuration = closestTimeSpan.Duration();
        //        if (duration >= closestTimeSpanDuration)
        //        {
        //            continue;
        //        }
        //        closestTimeSpan = timeSpan;
        //        closestClassPeriodNameComposite = classPeriodNameComposite;
        //    }

        //    if (closestClassPeriodNameComposite == null)
        //    {
        //        MessageBox.Show("ERROR: Could not find ClassPeriod .xml file.");
        //        return;
        //    }

        //    StartLocalClassPeriod(closestClassPeriodNameComposite, localCacheFolderPath);
        //}

        //public void StartLocalClassPeriod(ClassPeriodNameComposite classPeriodNameComposite, string localCacheFolderPath)
        //{
        //    var filePath = classPeriodNameComposite.FullClassPeriodFilePath;
        //    if (!File.Exists(filePath))
        //    {
        //        MessageBox.Show("Class Period doesn't exist!");
        //        return;
        //    }

        //    var classPeriod = ClassPeriod.LoadLocalClassPeriod(filePath);
        //    if (classPeriod == null)
        //    {
        //        MessageBox.Show("Class Period could not be opened. Check error log.");
        //        return;
        //    }

        //    CurrentLocalCacheDirectory = localCacheFolderPath;
        //    CurrentClassPeriod = classPeriod;
        //    var authoredNotebook = LoadClassPeriodNotebookForPerson(classPeriod, Person.Author.ID);
        //    if (authoredNotebook == null)
        //    {
        //        return;
        //    }
        //    //var authoredNotebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == Person.Author.ID);
        //    //var authoredPagesFolderPath = Path.Combine(authoredNotebookNameComposite.NotebookFolderPath, "Pages");
        //    //var pageIDs = GetPageIDsFromStartIDAndForwardRange(authoredPagesFolderPath, classPeriod.StartPageID, classPeriod.NumberOfPages);
        //    //if (!pageIDs.Contains(classPeriod.TitlePageID))
        //    //{
        //    //    pageIDs.Insert(0, classPeriod.TitlePageID);
        //    //}

        //    //var authoredPages = LoadOrCopyPagesForNotebook(authoredNotebook, null, pageIDs, false);
        //    //authoredNotebook.Pages = new ObservableCollection<CLPPage>(authoredPages);
        //    //authoredNotebook.CurrentPage = authoredNotebook.Pages.FirstOrDefault(); //HACK

        //    //var teacherNotebook = LoadClassPeriodNotebookForPerson(classPeriod, classPeriod.ClassInformation.TeacherID) ??
        //    //                      CopyNotebookForNewOwner(authoredNotebook, classPeriod.ClassInformation.Teacher);

        //    //var teacherPages = LoadOrCopyPagesForNotebook(teacherNotebook, authoredNotebook, pageIDs, true);
        //    //teacherNotebook.Pages = new ObservableCollection<CLPPage>(teacherPages);
        //    //teacherNotebook.CurrentPage = teacherNotebook.Pages.FirstOrDefault(); //HACK

        //    ////Generates pages in cache
        //    //foreach (var student in classPeriod.ClassInformation.StudentList)
        //    //{
        //    //    var studentNotebook = LoadClassPeriodNotebookForPerson(classPeriod, student.ID) ??
        //    //                      CopyNotebookForNewOwner(authoredNotebook, student);

        //    //    var studentPages = LoadOrCopyPagesForNotebook(studentNotebook, authoredNotebook, pageIDs, false);
        //    //    studentNotebook.Pages = new ObservableCollection<CLPPage>(studentPages);
        //    //    studentNotebook.CurrentPage = studentNotebook.Pages.FirstOrDefault(); //HACK
        //    //    SaveNotebookLocally(studentNotebook);
        //    //}

        //    //OpenNotebooks.Clear();
        //    //OpenNotebooks.Add(authoredNotebook);
        //    //OpenNotebooks.Add(teacherNotebook);
        //    //SetNotebookAsCurrentNotebook(teacherNotebook);
        //}

        //public Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID)
        //{
        //    var notebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == ownerID);
        //    if (notebookNameComposite == null)
        //    {
        //       // MessageBox.Show("Notebook for Class Period not found for " + ownerID + ".");
        //        return null;
        //    }

        //    Notebook notebook = null; // = Notebook.LoadLocalNotebook(notebookNameComposite.NotebookFolderPath);
        //    if (notebook == null)
        //    {
        //        //MessageBox.Show("Notebook for Class Period could not be loaded " + ownerID + ".");
        //        return null;
        //    }

        //    return notebook;
        //}

        //public Notebook CopyNotebookForNewOwner(Notebook originalNotebook, Person newOwner)
        //{
        //    var newNotebook = originalNotebook.Clone() as Notebook;
        //    if (newNotebook == null)
        //    {
        //        return null;
        //    }
        //    newNotebook.Owner = newOwner;
        //    newNotebook.CreationDate = DateTime.Now;
        //    newNotebook.LastSavedDate = null;

        //    return newNotebook;
        //}

        //public CLPPage CopyPageForNewOwner(CLPPage originalPage, Person newOwner)
        //{
        //    var newPage = originalPage.Clone() as CLPPage;
        //    if (newPage == null)
        //    {
        //        return null;
        //    }
        //    newPage.Owner = newOwner;
        //    newPage.CreationDate = DateTime.Now;

        //    foreach (var pageObject in newPage.PageObjects)
        //    {
        //        pageObject.ParentPage = newPage;
        //        if (pageObject.IsBackgroundInteractable)
        //        {
        //            pageObject.OwnerID = newOwner.ID;
        //        }
        //    }

        //    foreach (var tag in newPage.Tags)
        //    {
        //        tag.ParentPage = newPage;
        //    }

        //    newPage.AfterDeserialization();

        //    return newPage;
        //}

        //public List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, Notebook authoredNotebook, List<string> pageIDs, bool includeSubmissions)
        //{
        //    var pages = new List<CLPPage>();

        //    //var notebookNameComposite =
        //    //    GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory).FirstOrDefault(x => x.ID == notebook.ID && x.OwnerID == notebook.Owner.ID);

        //    //var pageNameComposites = new List<PageNameComposite>();
        //    //if (notebookNameComposite != null)
        //    //{
        //    //    var pagesFolderPath = Path.Combine(notebookNameComposite.NotebookFolderPath, "Pages");
        //    //    pageNameComposites = Directory.EnumerateFiles(pagesFolderPath, "*.xml").Select(PageNameComposite.ParseFilePath).Where(x => pageIDs.Contains(x.ID)).ToList();
        //    //}

        //    //foreach (var pageID in pageIDs)
        //    //{
        //    //    var pageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == "0");
        //    //    if (pageNameComposite == null)
        //    //    {
        //    //        if (authoredNotebook == null)
        //    //        {
        //    //            continue;
        //    //        }
        //    //        var authoredPage = authoredNotebook.Pages.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == 0);
        //    //        if (authoredPage == null)
        //    //        {
        //    //            continue;
        //    //        }

        //    //        var newPage = CopyPageForNewOwner(authoredPage, notebook.Owner);
        //    //        if (newPage == null)
        //    //        {
        //    //            continue;
        //    //        }

        //    //        pages.Add(newPage);
        //    //        continue;
        //    //    }

        //    //    var page = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
        //    //    if (page == null)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    if (includeSubmissions)
        //    //    {
        //    //        var id = pageID;
        //    //        foreach (var submissionComposite in pageNameComposites.Where(x => x.ID == id && x.VersionIndex != "0"))
        //    //        {
        //    //            var submission = CLPPage.LoadLocalPage(submissionComposite.FullPageFilePath);
        //    //            if (submission == null)
        //    //            {
        //    //                continue;
        //    //            }
        //    //            page.Submissions.Add(submission);
        //    //        }
        //    //    }

        //    //    pages.Add(page);
        //    //}

        //    return pages;
        //} 

        #endregion //Old ClassPeriod Methods

        #endregion //Methods

        #endregion // OBSOLETE
    }
}