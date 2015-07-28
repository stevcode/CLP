﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{

    #region Info Classes

    public class CacheInfo
    {
        public CacheInfo(string cacheFolderPath) { CacheFolderPath = cacheFolderPath; }

        public string CacheFolderPath { get; set; }

        public string CacheName
        {
            get
            {
                var directoryInfo = new DirectoryInfo(CacheFolderPath);
                return string.Join(" ", directoryInfo.Name.Split('.').Where(s => !s.ToLower().Contains("cache")));
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

    public class ClassPeriodInfo
    {
        public ClassPeriodInfo(CacheInfo cache, string classPeriodFilePath)
        {
            Cache = cache;
            ClassPeriodFilePath = classPeriodFilePath;
        }

        public CacheInfo Cache { get; set; }

        public string ClassPeriodFilePath { get; set; }

        public ClassPeriod ClassPeriod { get; set; }

        public ClassPeriodNameComposite NameComposite
        {
            get { return ClassPeriodNameComposite.ParseFilePath(ClassPeriodFilePath); }
        }

        public string StartTime
        {
            get { return NameComposite.StartTime; }
        }

        public string PageNumbers
        {
            get { return NameComposite.PageNumbers; }
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

        public string NotebookFolderPath { get; set; }

        public Notebook Notebook { get; set; }

        public List<CLPPage> Pages { get; set; }

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

        public NotebookNameComposite NameComposite
        {
            get { return NotebookNameComposite.ParseFolderPath(NotebookFolderPath); }
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
        public DataService()
        {
            //Warm up Serializer to make deserializing Notebooks, ClassPeriods, and ClassSubjects faster.
            var typesToWarmup = new[] { typeof (Notebook), typeof (ClassPeriod), typeof (ClassInformation) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);

            CurrentCachesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ArchivedCachesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        #region Properties

        #region Cache Properties

        public string CurrentCachesFolderPath { get; set; }

        public string ArchivedCachesFolderPath { get; set; }

        public List<CacheInfo> AvailableCaches
        {
            get { return GetCachesInFolder(CurrentCachesFolderPath); }
        }

        public CacheInfo CurrentCache { get; set; }

        #endregion //Cache Properties

        #region Notebook Properties

        public List<NotebookInfo> NotebooksInCurrentCache
        {
            get { return GetNotebooksInCache(CurrentCache); }
        }

        private readonly List<NotebookInfo> _openNotebooksInfo = new List<NotebookInfo>();

        public List<NotebookInfo> OpenNotebooksInfo
        {
            get { return _openNotebooksInfo; }
        }

        public NotebookInfo CurrentNotebookInfo { get; set; }

        public Notebook CurrentNotebook
        {
            get { return CurrentNotebookInfo == null ? null : CurrentNotebookInfo.Notebook; }
        }

        #endregion //Notebook Properties

        #endregion //Properties

        #region Methods

        #region Cache Methods

        public static List<CacheInfo> GetCachesInFolder(string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return new List<CacheInfo>();
            }

            var directoryInfo = new DirectoryInfo(cachesFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Where(directory => directory.Name.StartsWith("Cache"))
                             .Select(directory => new CacheInfo(directory.FullName))
                             .OrderBy(c => c.CacheName)
                             .ToList();
        }

        public CacheInfo CreateNewCache(string cacheName) { return CreateNewCache(cacheName, CurrentCachesFolderPath); }

        public CacheInfo CreateNewCache(string cacheName, string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return null;
            }

            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            cacheName = invalidFileNameCharacters.Aggregate(cacheName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var cacheFileName = "Cache." + cacheName;
            var cacheFolderPath = Path.Combine(cachesFolderPath, cacheFileName);
            var availableCaches = GetCachesInFolder(cachesFolderPath);

            var existingCache = availableCaches.FirstOrDefault(c => c.CacheFolderPath == cacheFolderPath);
            if (existingCache != null)
            {
                CurrentCache = existingCache;
                return existingCache;
            }

            //TODO: Necessary to create yet? Wait 'til save?
            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }

            var newCache = new CacheInfo(cacheFolderPath);
            CurrentCache = newCache;

            return newCache;
        }

        public void ArchiveCache(CacheInfo cacheInfo)
        {
            if (!Directory.Exists(cacheInfo.CacheFolderPath))
            {
                return;
            }

            var archiveDirectory = Path.Combine(ArchivedCachesFolderPath, "ArchivedCaches");
            var now = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            var newCacheDirectory = Path.Combine(archiveDirectory, "Cache-" + now);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            Directory.Move(cacheInfo.CacheFolderPath, newCacheDirectory);
        }

        #endregion //Cache Methods 

        #region Notebook Methods

        public static List<NotebookInfo> GetNotebooksInCache(CacheInfo cache)
        {
            if (!Directory.Exists(cache.NotebooksFolderPath))
            {
                return new List<NotebookInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.NotebooksFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => new NotebookInfo(cache, directory.FullName))
                             .Where(nc => nc != null)
                             .OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                             .ThenBy(nc => nc.NameComposite.OwnerName)
                             .ToList();
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum) { return CreateNewNotebook(notebookName, curriculum, CurrentCache); }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            notebookName = invalidFileNameCharacters.Aggregate(notebookName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var newNotebook = new Notebook(notebookName, Person.Author)
                              {
                                  Curriculum = curriculum
                              };

            var newPage = new CLPPage(Person.Author);
            newNotebook.AddPage(newPage);

            var notebookFolderName = NotebookNameComposite.ParseNotebook(newNotebook).ToFolderName();
            var notebookFolderPath = Path.Combine(cache.NotebooksFolderPath, notebookFolderName);
            if (Directory.Exists(notebookFolderPath))
            {
                return null;
            }

            var notebookInfo = new NotebookInfo(cache, notebookFolderPath);
            notebookInfo.Notebook = newNotebook;

            OpenNotebooksInfo.Add(notebookInfo);
            SetCurrentNotebook(notebookInfo);

            return notebookInfo;
        }

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isNotebookCurrentNotebook = true)
        {
            // Guarantee folder structure.
            notebookInfo.Cache.Initialize();
            notebookInfo.Initialize();

            // Is Notebook already loaded in memory?
            var existingNotebookInfo = OpenNotebooksInfo.FirstOrDefault(n => n.NotebookFilePath == notebookInfo.NotebookFilePath);
            if (existingNotebookInfo != null)
            {
                if (isForcedOpen)
                {
                    OpenNotebooksInfo.Remove(existingNotebookInfo);
                }
                else
                {
                    if (CurrentNotebookInfo == existingNotebookInfo)
                    {
                        notebookInfo.Notebook = CurrentNotebook;
                        App.MainWindowViewModel.IsBackStageVisible = false;
                        return;
                    }
                    SetCurrentNotebook(existingNotebookInfo);
                    return;
                }
            }

            // Is Notebook included in notebookInfo (e.g. send across the network instead of being loaded from the disk).
            if (notebookInfo.Notebook != null)
            {
                OpenNotebooksInfo.Add(notebookInfo);
                if (isNotebookCurrentNotebook)
                {
                    SetCurrentNotebook(notebookInfo);
                }
                return;
            }

            // Load Notebook from disk.
            var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFolderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook couldn't be loaded.");
                return;
            }

            notebookInfo.Notebook = notebook;

            OpenNotebooksInfo.Add(notebookInfo);
            if (isNotebookCurrentNotebook)
            {
                SetCurrentNotebook(notebookInfo);
            }
        }

        public void SaveNotebook(NotebookInfo notebookInfo, bool isForcedFullSave, bool isLocalSave, bool isExported)
        {
            if (isLocalSave)
            {
                notebookInfo.Cache.Initialize();
                if (isForcedFullSave)
                {
                    Directory.Delete(notebookInfo.NotebookFolderPath);
                }
                notebookInfo.Initialize();

                notebookInfo.Notebook.SaveToXML(notebookInfo.NotebookFolderPath);
            }

            if (isExported)
            {
                
            }


            //switch (App.MainWindowViewModel.CurrentProgramMode)
            //{
            //    case ProgramModes.Author:
            //    case ProgramModes.Database:
            //        break;
            //    case ProgramModes.Teacher:
            //     //   notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
            //        break;
            //    case ProgramModes.Projector:
            //      //  notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
            //        break;
            //    case ProgramModes.Student:
            //        var submissionsPath = Path.Combine(folderPath, "Pages");
            //        notebook.SaveSubmissions(submissionsPath);
            //        if (App.Network.InstructorProxy != null)
            //        {
            //            var sNotebook = ObjectSerializer.ToString(notebook);
            //            var zippedNotebook = CLPServiceAgent.Instance.Zip(sNotebook);
            //            App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
            //        }
            //        break;
            //}
        }

        public void PackageAndSendNotebook(NotebookInfo notebookInfo, bool isNotebookSaved = true)
        {
            if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Student ||
                App.Network.InstructorProxy == null)
            {
                return;
            }

            if (!isNotebookSaved)
            {
                
            }

            var sNotebook = ObjectSerializer.ToString(notebookInfo.Notebook);
            var zippedNotebook = CLPServiceAgent.Instance.Zip(sNotebook);
            App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
        }

        public void SetCurrentNotebook(NotebookInfo notebookInfo)
        {
            CurrentNotebookInfo = notebookInfo;
            
            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            App.MainWindowViewModel.CurrentNotebookName = CurrentNotebookInfo.Notebook.Name;
            App.MainWindowViewModel.CurrentUser = CurrentNotebookInfo.Notebook.Owner;
            App.MainWindowViewModel.IsAuthoring = CurrentNotebookInfo.Notebook.OwnerID == Person.Author.ID;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #endregion //Notebook Methods

        #region Page Methods

        public void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced, bool isLoadingSubmissions)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (isExistingPagesReplaced)
            {
                notebookInfo.Notebook.Pages.Clear();
            }

            var newNotebookPages = new List<CLPPage>();

            if (notebookInfo.Pages != null &&
                notebookInfo.Pages.Any()) // Load pages includeded in notebookInfo (e.g. ones sent across the network).
            {
                newNotebookPages = notebookInfo.Pages;
            }
            else // Load local pages.
            {
                var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

                Parallel.ForEach(pageFilePaths,
                                 pageFilePath =>
                                 {
                                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                     if (pageNameComposite == null ||
                                         pageNameComposite.VersionIndex != "0")
                                     {
                                         return;
                                     }

                                     var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
                                     if (!isPageToBeLoaded)
                                     {
                                         return;
                                     }

                                     var page = CLPPage.LoadFromXML(pageFilePath);
                                     if (page == null)
                                     {
                                         return;
                                     }

                                     newNotebookPages.Add(page);
                                 });
            }

            foreach (var page in newNotebookPages)
            {
                var index = notebookInfo.Notebook.Pages.ToList().BinarySearch(page, new PageNumberComparer());
                if (index < 0)
                {
                    index = ~index;
                }
                notebookInfo.Notebook.Pages.Insert(index, page);

                if (notebookInfo.Notebook.CurrentPageID == page.ID &&
                    notebookInfo.Notebook.CurrentPageOwnerID == page.OwnerID &&
                    notebookInfo.Notebook.CurrentPageVersionIndex == page.VersionIndex)
                {
                    notebookInfo.Notebook.CurrentPage = page;
                }
            }

            if (notebookInfo.Notebook.CurrentPage == null)
            {
                notebookInfo.Notebook.CurrentPage = notebookInfo.Notebook.Pages.FirstOrDefault();
            }

            if (!isLoadingSubmissions ||
                notebookInfo.Pages != null && 
                notebookInfo.Pages.Any())
            {
                return;
            }

            // Load submissions from disk.
            if ((App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher && 
                 App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector) ||
                notebookInfo.Notebook.Owner.ID == Person.Author.ID ||
                notebookInfo.Notebook.Owner.IsStudent) // Load student's own submission history.
            {
                var submissions = LoadOwnSubmissionsForLoadedPages(notebookInfo);

                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(p => p.ID == page.ID).OrderBy(p => p.VersionIndex).ToList());
                }
            }
            else // Load all student submissions for Teacher Notebook.
            {
                var notebookInfos = GetNotebooksInCache(notebookInfo.Cache).Where(n => n.NameComposite.ID == notebookInfo.Notebook.ID && n.NameComposite.OwnerTypeTag == "S");
                var pageFilePathsToCheck = new List<string>();

                foreach (var info in notebookInfos)
                {
                    pageFilePathsToCheck.AddRange(Directory.EnumerateFiles(info.PagesFolderPath, "*.xml").ToList());
                }

                var submissions = LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePathsToCheck);

                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
                }
            }
        }

        public void SaveNotebookPages(NotebookInfo notebookInfo, bool isForcedFullSave, bool serializeInkStrokes = true)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (!Directory.Exists(notebookInfo.PagesFolderPath))
            {
                Directory.CreateDirectory(notebookInfo.PagesFolderPath);
            }

            foreach (var page in notebookInfo.Notebook.Pages)
            {
                if (page.IsCached &&
                    !isForcedFullSave)
                {
                    continue;
                }

                page.SaveToXML(notebookInfo.PagesFolderPath, serializeInkStrokes);
            }

            //var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml").ToList();
            //foreach(var pageFilePath in from trashedPage in _trashedPages
            //                            from pageFilePath in pageFilePaths
            //                            where pageFilePath.Contains(trashedPage.ID)
            //                            select pageFilePath)
            //{
            //    File.Delete(pageFilePath);
            //}
            //_trashedPages.Clear();

            //foreach(var page in Pages)
            //{
            //    foreach(var pageFilePath in pageFilePaths)
            //    {
            //        if(pageFilePath.Contains(page.ID))
            //        {
            //            var pageNumberOfFile = Convert.ToInt32(Path.GetFileName(pageFilePath).Split(' ')[1]);
            //            if(page.PageNumber != pageNumberOfFile)
            //            {
            //                File.Delete(pageFilePath);
            //            }
            //        }
            //    }
            //}

            //foreach(var pageFilePath in from pageFilePath in pageFilePaths
            //                            let pageNumberOfFile = Convert.ToInt32(Path.GetFileName(pageFilePath).Split(' ')[1])
            //                            from page in Pages
            //                            where pageFilePath.Contains(page.ID) && page.PageNumber != pageNumberOfFile
            //                            select pageFilePath) 
            //{
            //    File.Delete(pageFilePath);
            //}
        }

        public static List<string> GetAllPageIDsInNotebook(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new List<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public static List<string> GetPageIDsFromPageNumbers(NotebookInfo notebookInfo, List<int> pageNumbers)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new List<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 int pageNumber;
                                 var isPageNumber = int.TryParse(pageNameComposite.PageNumber, out pageNumber);
                                 if (!isPageNumber)
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = pageNumbers.Contains(pageNumber);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public static List<CLPPage> LoadOwnSubmissionsForLoadedPages(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            return LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePaths);
        }

        public static List<CLPPage> LoadGivenSubmissionsForLoadedPages(NotebookInfo notebookInfo, List<string> pageFilePathsToCheck)
        {
            var submissions = new List<CLPPage>();

            Parallel.ForEach(pageFilePathsToCheck,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex == "0")
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 var page = CLPPage.LoadFromXML(pageFilePath);
                                 if (page == null)
                                 {
                                     return;
                                 }

                                 submissions.Add(page);
                             });

            return submissions;
        }

        #endregion //Page Methods

        #region ClassPeriod Methods

        public static List<ClassPeriodInfo> GetClassPeriodsInFolder(CacheInfo cache)
        {
            if (!Directory.Exists(cache.ClassesFolderPath))
            {
                return new List<ClassPeriodInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.ClassesFolderPath);
            return
                directoryInfo.GetFiles()
                             .Select(fileInfo => new ClassPeriodInfo(cache, fileInfo.FullName))
                             .Where(c => c != null)
                             .OrderBy(c => c.StartTime)
                             .ToList();
        }

        #endregion //ClassPeriod Methods

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
    }
}