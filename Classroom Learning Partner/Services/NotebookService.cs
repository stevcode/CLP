﻿using System;
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
    public class NotebookNameComposite
    {
        public string FullNotebookDirectoryPath { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string OwnerName { get; set; }
        public string OwnerID { get; set; }
        public string OwnerTypeTag { get; set; }
        public bool IsLocal { get; set; }
    }

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

        public List<string> AvailableLocalCacheNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                return
                    directoryInfo.GetDirectories()
                                 .Where(directory => directory.Name.StartsWith("Cache"))
                                 .Select(directory => directory.Name)
                                 .OrderBy(x => x)
                                 .ToList();
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

        public List<NotebookNameComposite> AvailableLocalNotebookNameComposites
        {
            get
            {
                return CurrentLocalCacheDirectory == null
                           ? new List<NotebookNameComposite>()
                           : GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory);
            }
        }

        private readonly List<Notebook> _openNotebooks = new List<Notebook>();

        public List<Notebook> OpeNotebooks //BUG: spelled wrong
        {
            get { return _openNotebooks; }
        }

        public Notebook CurrentNotebook { get; set; }

        public ClassPeriod CurrentClassPeriod { get; set; }

        #endregion //Properties

        #region Cache Methods

        public bool InitializeNewLocalCache(string cacheName)
        {
            return InitializeNewLocalCache(cacheName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        public bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath)
        {
            var directoryInfo = new DirectoryInfo(cacheDirectoryPath);
            var availableCacheNames =
                directoryInfo.GetDirectories()
                             .Where(directory => directory.Name.StartsWith("Cache"))
                             .Select(directory => directory.Name)
                             .OrderBy(x => x)
                             .ToList();

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

        #region Notebook Methods

        public void OpenNotebook(NotebookNameComposite notebookNameComposite, string localCacheFolderPath)
        {
            //TODO: find way to bypass this if partial notebook is currently open and you try to open full notebook (or vis versa).
            foreach (var otherNotebook in
                App.MainWindowViewModel.OpenNotebooks.Where(
                                                            otherNotebook =>
                                                            otherNotebook.ID == notebookNameComposite.ID &&
                                                            otherNotebook.OwnerID == notebookNameComposite.OwnerID))
            {
                App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(otherNotebook);
                return;
            }

            var folderPath = notebookNameComposite.FullNotebookDirectoryPath;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var notebook = Notebook.OpenNotebook(folderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            CurrentLocalCacheDirectory = localCacheFolderPath;
            OpeNotebooks.Add(notebook);
            CurrentNotebook = notebook;
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
            if (notebook.OwnerID == Person.Author.ID)
            {
                App.MainWindowViewModel.IsAuthoring = true;
            }
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        public void SaveCurrentNotebook() { SaveNotebook(CurrentNotebook); }

        public void SaveNotebook(Notebook notebook)
        {
            var folderPath = Path.Combine(CurrentNotebookCacheDirectory, NotebookToNotebookFolderName(notebook));

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

        #region Static Notebook Methods

        public static string NotebookToNotebookFolderName(Notebook notebook)
        {
            var ownerTypeTag = notebook.OwnerID == Person.Author.ID ? "A" : notebook.Owner.IsStudent ? "S" : "T";
            return notebook.Name + ";" + notebook.ID + ";" + notebook.Owner.FullName + ";" + notebook.OwnerID + ";" + ownerTypeTag;
        }

        public static NotebookNameComposite NotebookDirectoryToNotebookNameComposite(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var notebookDirectoryName = directoryInfo.Name;
            var notebookDirectoryParts = notebookDirectoryName.Split(';');
            if (notebookDirectoryParts.Length != 5)
            {
                return null;
            }

            var nameComposite = new NotebookNameComposite
                                {
                                    FullNotebookDirectoryPath = path,
                                    Name = notebookDirectoryParts[0],
                                    ID = notebookDirectoryParts[1],
                                    OwnerName = notebookDirectoryParts[2],
                                    OwnerID = notebookDirectoryParts[3],
                                    OwnerTypeTag = notebookDirectoryParts[4],
                                    IsLocal = true
                                };

            return nameComposite;
        }

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
                             .Select(directory => NotebookDirectoryToNotebookNameComposite(directory.FullName))
                             .Where(x => x != null)
                             .OrderByDescending(x => x.OwnerTypeTag)
                             .ThenBy(x => x.OwnerName)
                             .ToList();
        }

        #endregion //Static Notebook Methods
    }
}