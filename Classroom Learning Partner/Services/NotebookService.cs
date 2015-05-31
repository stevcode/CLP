using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class NotebookService
    {
        #region Properties

        #region Notebook

        public List<NotebookNameComposite> AvailableLocalNotebookNameComposites
        {
            get { return new List<NotebookNameComposite>(); }
        }

        private readonly List<Notebook> _openNotebooks = new List<Notebook>();

        public List<Notebook> OpenNotebooks
        {
            get { return _openNotebooks; }
        }

        public Notebook CurrentNotebook { get; set; }

        public Notebook CurrentNotebooksAuthoredSource
        {
            get { return OpenNotebooks.FirstOrDefault(x => x.ID == CurrentNotebook.ID && x.OwnerID == Person.Author.ID); }
        }

        #endregion //Notebook

        #endregion //Properties

        #region Methods

        #region Class Period

        public void StartSoonestClassPeriod(string localCacheFolderPath)
        {
            var classesFolderPath = Path.Combine(localCacheFolderPath, "Classes");
            var classPeriodFilePaths = Directory.GetFiles(classesFolderPath);
            ClassPeriodNameComposite closestClassPeriodNameComposite = null;
            var closestTimeSpan = TimeSpan.MaxValue;
            var now = DateTime.Now;
            foreach (var classPeriodFilePath in classPeriodFilePaths)
            {
                var classPeriodNameComposite = ClassPeriodNameComposite.ParseFilePath(classPeriodFilePath);
                if (classPeriodNameComposite == null)
                {
                    continue;
                }
                var time = classPeriodNameComposite.StartTime;
                var timeParts = time.Split('.');
                var year = Int32.Parse(timeParts[0]);
                var month = Int32.Parse(timeParts[1]);
                var day = Int32.Parse(timeParts[2]);
                var hour = Int32.Parse(timeParts[3]);
                var minute = Int32.Parse(timeParts[4]);
                var dateTime = new DateTime(year, month, day, hour, minute, 0);

                var timeSpan = now - dateTime;
                var duration = timeSpan.Duration();
                var closestTimeSpanDuration = closestTimeSpan.Duration();
                if (duration >= closestTimeSpanDuration)
                {
                    continue;
                }
                closestTimeSpan = timeSpan;
                closestClassPeriodNameComposite = classPeriodNameComposite;
            }

            if (closestClassPeriodNameComposite == null)
            {
                MessageBox.Show("ERROR: Could not find ClassPeriod .xml file.");
                return;
            }

            StartLocalClassPeriod(closestClassPeriodNameComposite, localCacheFolderPath);
        }

        public void StartLocalClassPeriod(ClassPeriodNameComposite classPeriodNameComposite, string localCacheFolderPath)
        {
            var filePath = classPeriodNameComposite.FullClassPeriodFilePath;
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Class Period doesn't exist!");
                return;
            }

            var classPeriod = ClassPeriod.LoadLocalClassPeriod(filePath);
            if (classPeriod == null)
            {
                MessageBox.Show("Class Period could not be opened. Check error log.");
                return;
            }

            CurrentLocalCacheDirectory = localCacheFolderPath;
            CurrentClassPeriod = classPeriod;
            var authoredNotebook = LoadClassPeriodNotebookForPerson(classPeriod, Person.Author.ID);
            if (authoredNotebook == null)
            {
                return;
            }
            //var authoredNotebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == Person.Author.ID);
            //var authoredPagesFolderPath = Path.Combine(authoredNotebookNameComposite.NotebookFolderPath, "Pages");
            //var pageIDs = GetPageIDsFromStartIDAndForwardRange(authoredPagesFolderPath, classPeriod.StartPageID, classPeriod.NumberOfPages);
            //if (!pageIDs.Contains(classPeriod.TitlePageID))
            //{
            //    pageIDs.Insert(0, classPeriod.TitlePageID);
            //}

            //var authoredPages = LoadOrCopyPagesForNotebook(authoredNotebook, null, pageIDs, false);
            //authoredNotebook.Pages = new ObservableCollection<CLPPage>(authoredPages);
            //authoredNotebook.CurrentPage = authoredNotebook.Pages.FirstOrDefault(); //HACK

            //var teacherNotebook = LoadClassPeriodNotebookForPerson(classPeriod, classPeriod.ClassInformation.TeacherID) ??
            //                      CopyNotebookForNewOwner(authoredNotebook, classPeriod.ClassInformation.Teacher);

            //var teacherPages = LoadOrCopyPagesForNotebook(teacherNotebook, authoredNotebook, pageIDs, true);
            //teacherNotebook.Pages = new ObservableCollection<CLPPage>(teacherPages);
            //teacherNotebook.CurrentPage = teacherNotebook.Pages.FirstOrDefault(); //HACK

            ////Generates pages in cache
            //foreach (var student in classPeriod.ClassInformation.StudentList)
            //{
            //    var studentNotebook = LoadClassPeriodNotebookForPerson(classPeriod, student.ID) ??
            //                      CopyNotebookForNewOwner(authoredNotebook, student);

            //    var studentPages = LoadOrCopyPagesForNotebook(studentNotebook, authoredNotebook, pageIDs, false);
            //    studentNotebook.Pages = new ObservableCollection<CLPPage>(studentPages);
            //    studentNotebook.CurrentPage = studentNotebook.Pages.FirstOrDefault(); //HACK
            //    SaveNotebookLocally(studentNotebook);
            //}

            //OpenNotebooks.Clear();
            //OpenNotebooks.Add(authoredNotebook);
            //OpenNotebooks.Add(teacherNotebook);
            //SetNotebookAsCurrentNotebook(teacherNotebook);
        }

        public Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID)
        {
            var notebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == ownerID);
            if (notebookNameComposite == null)
            {
               // MessageBox.Show("Notebook for Class Period not found for " + ownerID + ".");
                return null;
            }

            Notebook notebook = null; // = Notebook.LoadLocalNotebook(notebookNameComposite.NotebookFolderPath);
            if (notebook == null)
            {
                //MessageBox.Show("Notebook for Class Period could not be loaded " + ownerID + ".");
                return null;
            }

            return notebook;
        }

        public Notebook CopyNotebookForNewOwner(Notebook originalNotebook, Person newOwner)
        {
            var newNotebook = originalNotebook.Clone() as Notebook;
            if (newNotebook == null)
            {
                return null;
            }
            newNotebook.Owner = newOwner;
            newNotebook.CreationDate = DateTime.Now;
            newNotebook.LastSavedDate = null;

            return newNotebook;
        }

        public CLPPage CopyPageForNewOwner(CLPPage originalPage, Person newOwner)
        {
            var newPage = originalPage.Clone() as CLPPage;
            if (newPage == null)
            {
                return null;
            }
            newPage.Owner = newOwner;
            newPage.CreationDate = DateTime.Now;

            foreach (var pageObject in newPage.PageObjects)
            {
                pageObject.ParentPage = newPage;
                if (pageObject.IsBackgroundInteractable)
                {
                    pageObject.OwnerID = newOwner.ID;
                }
            }

            foreach (var tag in newPage.Tags)
            {
                tag.ParentPage = newPage;
            }

            newPage.AfterDeserialization();

            return newPage;
        }

        public List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, Notebook authoredNotebook, List<string> pageIDs, bool includeSubmissions)
        {
            var pages = new List<CLPPage>();

            //var notebookNameComposite =
            //    GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory).FirstOrDefault(x => x.ID == notebook.ID && x.OwnerID == notebook.Owner.ID);

            //var pageNameComposites = new List<PageNameComposite>();
            //if (notebookNameComposite != null)
            //{
            //    var pagesFolderPath = Path.Combine(notebookNameComposite.NotebookFolderPath, "Pages");
            //    pageNameComposites = Directory.EnumerateFiles(pagesFolderPath, "*.xml").Select(PageNameComposite.ParseFilePath).Where(x => pageIDs.Contains(x.ID)).ToList();
            //}

            //foreach (var pageID in pageIDs)
            //{
            //    var pageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == "0");
            //    if (pageNameComposite == null)
            //    {
            //        if (authoredNotebook == null)
            //        {
            //            continue;
            //        }
            //        var authoredPage = authoredNotebook.Pages.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == 0);
            //        if (authoredPage == null)
            //        {
            //            continue;
            //        }

            //        var newPage = CopyPageForNewOwner(authoredPage, notebook.Owner);
            //        if (newPage == null)
            //        {
            //            continue;
            //        }

            //        pages.Add(newPage);
            //        continue;
            //    }

            //    var page = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
            //    if (page == null)
            //    {
            //        continue;
            //    }

            //    if (includeSubmissions)
            //    {
            //        var id = pageID;
            //        foreach (var submissionComposite in pageNameComposites.Where(x => x.ID == id && x.VersionIndex != "0"))
            //        {
            //            var submission = CLPPage.LoadLocalPage(submissionComposite.FullPageFilePath);
            //            if (submission == null)
            //            {
            //                continue;
            //            }
            //            page.Submissions.Add(submission);
            //        }
            //    }

            //    pages.Add(page);
            //}

            return pages;
        }

        #endregion //Class Period

        #region Notebook

        public void SaveNotebookLocally(Notebook notebook, string alternativeLocation = null)
        {
            var savePath = alternativeLocation ?? CurrentNotebookCacheDirectory;
            var folderPath = Path.Combine(savePath, NotebookNameComposite.ParseNotebook(notebook).ToFolderName());
            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if (App.MainWindowViewModel.CurrentUser.ID == Person.Author.ID)
            {
                if (Directory.Exists(pagesFolderPath))
                {
                    var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml").ToList();
                    foreach (var pageFilePath in pageFilePaths)
                    {
                        File.Delete(pageFilePath);
                    }
                }
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            notebook.SaveNotebook(folderPath, true);
            notebook.SaveSubmissions(pagesFolderPath);

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

        #endregion //Notebook

        #endregion //Methods

        #region Static Notebook Methods

        public static List<ClassPeriodNameComposite> GetAvailableClassPeriodNameCompositesInCache(string cachePath)
        {
            var classesCacheDirectory = Path.Combine(cachePath, "Classes");
            if (!Directory.Exists(classesCacheDirectory))
            {
                Directory.CreateDirectory(classesCacheDirectory);
            }
            var directoryInfo = new DirectoryInfo(classesCacheDirectory);
            return
                directoryInfo.GetFiles()
                             .Select(file => ClassPeriodNameComposite.ParseFilePath(file.FullName))
                             .Where(x => x != null)
                             .OrderByDescending(x => x.StartTime)
                             .ToList();
        }

        #endregion //Static Notebook Methods
    }
}