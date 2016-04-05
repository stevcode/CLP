using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Catel.Collections;
using Catel.Data;
using Catel.IO;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using CLP.Entities.DomainModel.Utility.Extentions;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OpenNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OpenNotebookPaneViewModel()
        {
            InitializeCommands();
            AvailableCaches.AddRange(DataService.AvailableCaches);
            SelectedCache = AvailableCaches.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute, OnOpenNotebookCanExecute);
            OpenPageRangeCommand = new Command(OnOpenPageRangeCommandExecute, OnOpenNotebookCanExecute);
            StartClassPeriodCommand = new Command(OnStartClassPeriodCommandExecute);
            AnonymizeCacheCommand = new Command(OnAnonymizeCacheCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Open Notebook"; }
        }

        #region Cache Bindings

        /// <summary>List of available Caches.</summary>
        public ObservableCollection<CacheInfo> AvailableCaches
        {
            get { return GetValue<ObservableCollection<CacheInfo>>(AvailableCachesProperty); }
            set { SetValue(AvailableCachesProperty, value); }
        }

        public static readonly PropertyData AvailableCachesProperty = RegisterProperty("AvailableCaches", typeof (ObservableCollection<CacheInfo>), () => new ObservableCollection<CacheInfo>());

        /// <summary>Selected Cache.</summary>
        public CacheInfo SelectedCache
        {
            get { return GetValue<CacheInfo>(SelectedCacheProperty); }
            set { SetValue(SelectedCacheProperty, value); }
        }

        public static readonly PropertyData SelectedCacheProperty = RegisterProperty("SelectedCache", typeof (CacheInfo), null, OnSelectedCacheChanged);

        private static void OnSelectedCacheChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var openNotebookPaneViewModel = sender as OpenNotebookPaneViewModel;
            if (openNotebookPaneViewModel == null ||
                openNotebookPaneViewModel.SelectedCache == null)
            {
                return;
            }

            openNotebookPaneViewModel.DataService.CurrentCacheInfo = openNotebookPaneViewModel.SelectedCache;
            openNotebookPaneViewModel.AvailableNotebooks.Clear();
            openNotebookPaneViewModel.AvailableNotebooks.AddRange(Services.DataService.GetNotebooksInCache(openNotebookPaneViewModel.SelectedCache));
            openNotebookPaneViewModel.SelectedNotebook = openNotebookPaneViewModel.AvailableNotebooks.FirstOrDefault();
        }

        #endregion //Cache Bindings

        #region Notebook Bindings

        /// <summary>Available notebooks in the currently selected Cache.</summary>
        public ObservableCollection<NotebookInfo> AvailableNotebooks
        {
            get { return GetValue<ObservableCollection<NotebookInfo>>(AvailableNotebooksProperty); }
            set { SetValue(AvailableNotebooksProperty, value); }
        }

        public static readonly PropertyData AvailableNotebooksProperty = RegisterProperty("AvailableNotebooks",
                                                                                          typeof (ObservableCollection<NotebookInfo>),
                                                                                          () => new ObservableCollection<NotebookInfo>());

        /// <summary>Currently selected Notebook.</summary>
        public NotebookInfo SelectedNotebook
        {
            get { return GetValue<NotebookInfo>(SelectedNotebookProperty); }
            set { SetValue(SelectedNotebookProperty, value); }
        }

        public static readonly PropertyData SelectedNotebookProperty = RegisterProperty("SelectedNotebook", typeof (NotebookInfo));

        /// <summary>Toggles the loading of submissions when opening a notebook.</summary>
        public bool IsIncludeSubmissionsChecked
        {
            get { return GetValue<bool>(IsIncludeSubmissionsCheckedProperty); }
            set { SetValue(IsIncludeSubmissionsCheckedProperty, value); }
        }

        public static readonly PropertyData IsIncludeSubmissionsCheckedProperty = RegisterProperty("IsIncludeSubmissionsChecked", typeof (bool), true);

        #endregion //Notebook Bindings

        #endregion //Bindings

        #region Commands

        /// <summary>Opens selected notebook.</summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            PleaseWaitHelper.Show(() => DataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            var pageIDs = Services.DataService.GetAllPageIDsInNotebook(SelectedNotebook);
            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.LoadPages(SelectedNotebook, pageIDs, true);
                                      DataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, true);
                                      if ((App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher || App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector) && IsIncludeSubmissionsChecked && SelectedNotebook.NameComposite.OwnerTypeTag == "T")
                                      {
                                          Parallel.ForEach(AvailableNotebooks,
                                                           notebookInfo =>
                                                           {
                                                               if (notebookInfo.NameComposite.OwnerTypeTag == "A" ||
                                                                   notebookInfo.NameComposite.OwnerTypeTag == "T" ||
                                                                   notebookInfo == SelectedNotebook)
                                                               {
                                                                   return;
                                                               }

                                                               DataService.OpenNotebook(notebookInfo, false, false);
                                                               DataService.LoadPages(notebookInfo, pageIDs, true);
                                                               DataService.LoadLocalSubmissions(notebookInfo, pageIDs, true);
                                                           });
                                      }
                                  },
                                  null,
                                  "Loading Pages");

            //if (App.Network.InstructorProxy == null)
            //{
            //    return;
            //}

            //var connectionString = App.Network.InstructorProxy.StudentLogin(App.MainWindowViewModel.CurrentUser.FullName,
            //                                         App.MainWindowViewModel.CurrentUser.ID,
            //                                         App.Network.CurrentMachineName,
            //                                         App.Network.CurrentMachineAddress);

            //if (connectionString == "connected")
            //{
            //    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.LoggedIn;
            //}
        }

        /// <summary>Opens a range of pages in a notebook.</summary>
        public Command OpenPageRangeCommand { get; private set; }

        private void OnOpenPageRangeCommandExecute()
        {
            var textInputViewModel = new TextInputViewModel();
            var textInputView = new TextInputView(textInputViewModel);
            textInputView.ShowDialog();

            if (textInputView.DialogResult == null ||
                textInputView.DialogResult != true ||
                string.IsNullOrEmpty(textInputViewModel.InputText))
            {
                return;
            }

            var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(textInputViewModel.InputText);
            if (!pageNumbersToOpen.Any())
            {
                return;
            }

            PleaseWaitHelper.Show(() => DataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            var pageIDs = Services.DataService.GetPageIDsFromPageNumbers(SelectedNotebook, pageNumbersToOpen);
            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.LoadPages(SelectedNotebook, pageIDs, false);
                                      DataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, false);
                                      if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher && IsIncludeSubmissionsChecked && SelectedNotebook.NameComposite.OwnerTypeTag == "T")
                                      {
                                          Parallel.ForEach(AvailableNotebooks,
                                                           notebookInfo =>
                                                           {
                                                               if (notebookInfo.NameComposite.OwnerTypeTag == "A" ||
                                                                   notebookInfo.NameComposite.OwnerTypeTag == "T" ||
                                                                   notebookInfo == SelectedNotebook)
                                                               {
                                                                   return;
                                                               }

                                                               DataService.OpenNotebook(notebookInfo, false, false);
                                                               DataService.LoadPages(notebookInfo, pageIDs, true);
                                                               DataService.LoadLocalSubmissions(notebookInfo, pageIDs, true);
                                                           });
                                      }
                                  },
                                  null,
                                  "Loading Pages");
        }

        private bool OnOpenNotebookCanExecute() { return SelectedNotebook != null; }

        /// <summary>Starts the closest <see cref="ClassPeriod" />.</summary>
        public Command StartClassPeriodCommand { get; private set; }

        private void OnStartClassPeriodCommandExecute()
        {
            //LoadedNotebookService.StartSoonestClassPeriod(SelectedCacheDirectory);
            //    LoadedNotebookService.StartLocalClassPeriod(, SelectedCacheDirectory);
        }

        /// <summary>SUMMARY</summary>
        public Command AnonymizeCacheCommand { get; private set; }

        private void OnAnonymizeCacheCommandExecute()
        {
            const string TEXT_FILE_NAME = "AnonymousNames.txt";
            var anonymousTextFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), TEXT_FILE_NAME);
            if (!File.Exists(anonymousTextFilePath))
            {
                MessageBox.Show("You are missing AnonymousNames.txt on the Desktop.");
                return;
            }

            var names = new Dictionary<string, string>();
            var textFile = new StreamReader(anonymousTextFilePath);
            string line;
            while ((line = textFile.ReadLine()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length != 2)
                {
                    MessageBox.Show("AnonymousNames.txt is in the wrong format.");
                    textFile.Close();
                    return;
                }

                var oldName = parts[0];
                var newName = parts[1];
                newName = newName.Replace("\t", string.Empty);
                newName = newName.Replace("\n", string.Empty);
                newName = newName.Trim();

                if (!names.ContainsKey(oldName))
                {
                    names.Add(oldName, newName);
                }
            }

            textFile.Close();

            var cacheInfoToAnonymize = SelectedCache;
            var newCacheFolderPath = cacheInfoToAnonymize.CacheFolderPath + ".Anon";
            var newCacheInfo = new CacheInfo(newCacheFolderPath);
            newCacheInfo.Initialize();
            var notebooksFolderPathDirectoryInfo = new DirectoryInfo(newCacheInfo.NotebooksFolderPath);

            PleaseWaitHelper.Show(() =>
                                  {
                                      foreach (var file in notebooksFolderPathDirectoryInfo.GetFiles())
                                      {
                                          file.Delete();
                                      }
                                      foreach (var dir in notebooksFolderPathDirectoryInfo.GetDirectories())
                                      {
                                          dir.Delete(true);
                                      }

                                      var imagesFolderPathDirectoryInfo = new DirectoryInfo(cacheInfoToAnonymize.ImagesFolderPath);
                                      imagesFolderPathDirectoryInfo.Copy(newCacheFolderPath);

                                      var notebookInfosToCopy = Services.DataService.GetNotebooksInCache(cacheInfoToAnonymize);
                                      foreach (var notebookInfo in notebookInfosToCopy)
                                      {
                                          var notebookFolderPath = notebookInfo.NotebookFolderPath;
                                          var directoryInfo = new DirectoryInfo(notebookFolderPath);
                                          directoryInfo.Copy(newCacheInfo.NotebooksFolderPath);
                                      }
                                  },
                                  null,
                                  "Copying Data");

            var nonConvertedNames = new List<string>();
            PleaseWaitHelper.Show(() =>
                                  {
                                      var notebookInfosToAnonymize = Services.DataService.GetNotebooksInCache(newCacheInfo);
                                      foreach (var notebookInfo in notebookInfosToAnonymize.Where(ni => ni.NameComposite.OwnerTypeTag == "S"))
                                      {
                                          var nameComposite = notebookInfo.NameComposite;
                                          var oldName = nameComposite.OwnerName;
                                          if (!names.ContainsKey(oldName))
                                          {
                                              nonConvertedNames.Add(oldName);
                                              continue;
                                          }

                                          var newName = names[oldName];
                                          nameComposite.OwnerName = newName;
                                          var newFolderName = nameComposite.ToFolderName();
                                          var notebookFolderPath = notebookInfo.NotebookFolderPath;
                                          var directoryInfo = new DirectoryInfo(notebookFolderPath);
                                          var parentDirectory = directoryInfo.Parent.FullName;
                                          var newFolderPath = Path.Combine(parentDirectory, newFolderName);
                                          directoryInfo.MoveTo(newFolderPath);

                                          var notebookFilePath = Path.Combine(newFolderPath, "notebook.xml");
                                          if (!File.Exists(notebookFilePath))
                                          {
                                              MessageBox.Show("Problem with copied cache. Exiting.");
                                              return;
                                          }

                                          var doc = new XmlDocument();
                                          doc.Load(notebookFilePath);
                                          var node = doc.DocumentElement;
                                          foreach (XmlNode childNode in node.ChildNodes)
                                          {
                                              foreach (XmlNode secondChildNode in childNode)
                                              {
                                                  if (secondChildNode.Name == "FullName")
                                                  {
                                                      secondChildNode.InnerText = newName;
                                                      break;
                                                  }
                                              }
                                          }

                                          doc.Save(notebookFilePath);

                                          var pagesFolderPath = Path.Combine(newFolderPath, "Pages");
                                          var pagesDirectoryInfo = new DirectoryInfo(pagesFolderPath);
                                          foreach (var pageFileInfo in pagesDirectoryInfo.GetFiles("*.xml"))
                                          {
                                              var pageDoc = new XmlDocument();
                                              pageDoc.Load(pageFileInfo.FullName);
                                              var pageNode = pageDoc.DocumentElement;
                                              foreach (XmlNode childNode in pageNode.ChildNodes)
                                              {
                                                  foreach (XmlNode secondChildNode in childNode)
                                                  {
                                                      if (secondChildNode.Name == "FullName")
                                                      {
                                                          secondChildNode.InnerText = newName;
                                                          break;
                                                      }
                                                  }
                                              }

                                              pageDoc.Save(pageFileInfo.FullName);
                                          }
                                      }
                                  },
                                  null,
                                  "Anonymizing Data");

            if (nonConvertedNames.Any())
            {
                var namesToPrint = string.Join("\n", nonConvertedNames);
                MessageBox.Show("Names not Anonymized:\n" + namesToPrint);
            }
        }

        #endregion //Commands
    }
}