using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner.Services
{
    public class NotebookService : INotebookService
    {
        public NotebookService()
        {
            //Warm up Serializer to make loading of notebook faster.
            var typesToWarmup = new[] { typeof (Notebook) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);
        }

        #region Properties

        #region Cache

        public List<string> AvailableLocalCacheNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                return directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => directory.Name).OrderBy(x => x).ToList();
            }
        }

        public string CurrentLocalCacheDirectory { get; set; }

        public string CurrentClassCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Classes");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string CurrentImageCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string CurrentNotebookCacheDirectory
        {
            get
            {
                var path = Path.Combine(CurrentLocalCacheDirectory, "Notebooks");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        #endregion //Cache

        #region Notebook

        public List<NotebookNameComposite> AvailableLocalNotebookNameComposites
        {
            get { return CurrentLocalCacheDirectory == null ? new List<NotebookNameComposite>() : GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory); }
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

        #region ClassPeriod

        public ClassPeriod CurrentClassPeriod { get; set; }

        #endregion //ClassPeriod

        #endregion //Properties

        #region Cache Methods

        public bool InitializeNewLocalCache(string cacheName) { return InitializeNewLocalCache(cacheName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop)); }

        public bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath)
        {
            var directoryInfo = new DirectoryInfo(cacheDirectoryPath);
            var availableCacheNames =
                directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => directory.Name).OrderBy(x => x).ToList();

            if (availableCacheNames.Contains(cacheName))
            {
                return false;
            }

            CurrentLocalCacheDirectory = Path.Combine(cacheDirectoryPath, cacheName);
            return true;
        }

        public void ArchiveNotebookCache(string notebookCacheDirectory)
        {
            if (!Directory.Exists(notebookCacheDirectory))
            {
                return;
            }

            var archiveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ArchivedCaches");
            var now = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            var newCacheDirectory = Path.Combine(archiveDirectory, "Cache-" + now);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            Directory.Move(notebookCacheDirectory, newCacheDirectory);
        }

        #endregion //Cache Methods

        #region Methods

        #region Notebook Methods

        public void OpenNotebook(NotebookNameComposite notebookNameComposite, string localCacheFolderPath)
        {
            var existingNotebook = OpenNotebooks.FirstOrDefault(x => x.ID == notebookNameComposite.ID && x.OwnerID == notebookNameComposite.OwnerID);
            if (existingNotebook != null)
            {
                if (CurrentNotebook == existingNotebook)
                {
                    return;
                }
                CurrentNotebook = existingNotebook;
                App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
                App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(existingNotebook);
                return;
            }

            var folderPath = notebookNameComposite.FullNotebookDirectoryPath;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var notebook = Notebook.OpenFullNotebook(folderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            CurrentLocalCacheDirectory = localCacheFolderPath;
            OpenNotebooks.Add(notebook);
            CurrentNotebook = notebook;
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
            if (notebook.OwnerID == Person.Author.ID)
            {
                App.MainWindowViewModel.IsAuthoring = true;
            }
            App.MainWindowViewModel.CurrentUser = notebook.Owner;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        public void SaveCurrentNotebookLocally() { SaveNotebookLocally(CurrentNotebook); }

        public void SaveNotebookLocally(Notebook notebook)
        {
            var folderPath = Path.Combine(CurrentNotebookCacheDirectory, notebook.NotebookToNotebookFolderName());

            if (App.MainWindowViewModel.CurrentUser.ID == Person.Author.ID)
            {
                var pagesFolderPath = Path.Combine(folderPath, "Pages");
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

            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramModes.Author:
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
                    break;
                case ProgramModes.Projector:
                    notebook.SaveOthersSubmissions(CurrentNotebookCacheDirectory);
                    break;
                case ProgramModes.Student:
                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    notebook.SaveSubmissions(submissionsPath);
                    if (App.Network.InstructorProxy != null)
                    {
                        var sNotebook = ObjectSerializer.ToString(notebook);
                        var zippedNotebook = CLPServiceAgent.Instance.Zip(sNotebook);
                        App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
                    }
                    break;
            }

            if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher &&
                App.MainWindowViewModel.CurrentClassPeriod != null &&
                App.MainWindowViewModel.CurrentClassPeriod.ClassSubject != null)
            {
                App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.SaveClassSubject(CurrentClassCacheDirectory);
            }
        }

        #endregion //Notebook Methods

        #endregion //Methods

        #region Static Notebook Methods

        public static List<NotebookNameComposite> GetAvailableNotebookNameCompositesInCache(string cachePath)
        {
            var notebookCacheDirectory = Path.Combine(cachePath, "Notebooks");
            if (!Directory.Exists(notebookCacheDirectory))
            {
                Directory.CreateDirectory(notebookCacheDirectory);
            }
            var directoryInfo = new DirectoryInfo(notebookCacheDirectory);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => Notebook.NotebookDirectoryToNotebookNameComposite(directory.FullName))
                             .Where(x => x != null)
                             .OrderByDescending(x => x.OwnerTypeTag)
                             .ThenBy(x => x.OwnerName)
                             .ToList();
        }

        #endregion //Static Notebook Methods
    }
}