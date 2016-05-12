using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
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
            LargeCacheAnalysisCommand = new Command(OnLargeCacheAnalysisCommandExecute);
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
                                                      }
                                                      if (secondChildNode.Name == "Alias")
                                                      {
                                                          secondChildNode.InnerText = string.Empty;
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

        public Command LargeCacheAnalysisCommand { get; private set; }

        private void OnLargeCacheAnalysisCommandExecute()
        {
            #region Initialize TSV file and header columns

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "LargeCacheAnalysis");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, "BatchAnalysis.tsv");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");

            var columnHeaders = new List<string>
                                {
                                    "STUDENT NAME",
                                    "PAGE NUMBER",
                                    "SUBMISSION TIME",
                                    "ARR",
                                    "ARR cut",
                                    "ARR snap",
                                    "ARR divide",
                                    "NL",
                                    "NL used",
                                    "NLs w/ changed endpoints",
                                    "MR",
                                    "Ink Only",
                                    "Blank"
                                };
            var tabbedColumnHeaders = string.Join("\t", columnHeaders);
            File.AppendAllText(filePath, tabbedColumnHeaders);

            var fileRows = new List<List<string>>();

            #endregion // Initialize TSV file and header columns

            #region Generate Stats

            XNamespace typeNamespace = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace entityNamespace = "http://schemas.datacontract.org/2004/07/CLP.Entities";
            XNamespace serializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
            var typeName = typeNamespace + "type";
            var anyTypeName = serializationNamespace + "anyType";
            var stringTypeName = serializationNamespace + "string";
            var strokeName = entityNamespace + "StrokeDTO";
            var strokeOwnerIDName = entityNamespace + "_x003C_PersonID_x003E_k__BackingField";

            const string ARRAY_ENTITY = "d1p1:CLPArray";
            const string NUMBER_LINE_ENTITY = "d1p1:NumberLine";
            const string STAMP_ENTITY = "d1p1:Stamp";

            const string CUT_ENTITY = "d1p1:PageObjectCutHistoryItem";
            const string SNAP_ENTITY = "d1p1:CLPArraySnapHistoryItem";
            const string DIVIDE_ENTITY = "d1p1:CLPArrayDivisionsChangedHistoryItem";
            const string END_POINTS_CHANGED_ENTITY = "d1p1:NumberLineEndPointsChangedHistoryItem";

            var missingPages = new Dictionary<string,List<int>>();

            var cacheInfoToAnalyze = SelectedCache;
            var pagesToIgnore = new List<int>
                                {
                                    1, 2, 3, 4, 5, 6, 7, 8
                                };

            var notebookInfosToAnalyze = Services.DataService.GetNotebooksInCache(cacheInfoToAnalyze);
            foreach (var notebookInfo in notebookInfosToAnalyze.Where(ni => ni.NameComposite.OwnerTypeTag == "S"))
            {
                var allPageNumbers = Enumerable.Range(1, 386 - 1).ToList();

                var nameComposite = notebookInfo.NameComposite;
                var studentName = nameComposite.OwnerName;
                var studentOwnerID = nameComposite.OwnerID;

                var pagesDirectoryInfo = new DirectoryInfo(notebookInfo.PagesFolderPath);
                foreach (var pageFileInfo in pagesDirectoryInfo.GetFiles("*.xml"))
                {
                    var pageDoc = XElement.Load(pageFileInfo.FullName);

                    var pageNumber = pageDoc.Descendants("PageNumber").First().Value;
                    var pageNumberValue = int.Parse(pageNumber);
                    if (allPageNumbers.Contains(pageNumberValue))
                    {
                        allPageNumbers.Remove(pageNumberValue);
                    }
                    var versionIndex = pageDoc.Descendants("VersionIndex").First().Value;
                    if (versionIndex != "0" ||
                        pagesToIgnore.Contains(pageNumberValue))
                    {
                        continue;
                    }
                    var submissionTime = pageDoc.Descendants("SubmissionTime").First().Value;
                    if (string.IsNullOrEmpty(submissionTime) || string.IsNullOrWhiteSpace(submissionTime))
                    {
                        submissionTime = "UNSUBMITTED";
                    }

                    // PageObjects
                    var pageObjects = pageDoc.Descendants("PageObjects").First().Descendants(anyTypeName).Where(xe => xe.Descendants("CreatorID").First().Value == studentOwnerID).ToList();
                    var trashedPageObjects = pageDoc.Descendants("TrashedPageObjects").First().Descendants(anyTypeName).Where(xe => xe.Descendants("CreatorID").First().Value == studentOwnerID).ToList();

                    // Ink
                    var inkOnPage =
                        pageDoc.Descendants("SerializedStrokes")
                               .First()
                               .Descendants(strokeName)
                               .Where(xe => xe.Descendants(strokeOwnerIDName).First().Value == studentOwnerID)
                               .ToList();
                    var trashedInk =
                        pageDoc.Descendants("SerializedTrashedInkStrokes")
                               .First()
                               .Descendants(strokeName)
                               .Where(xe => xe.Descendants(strokeOwnerIDName).First().Value == studentOwnerID)
                               .ToList();

                    // History
                    var undoHistoryItems = pageDoc.Descendants("UndoItems").First().Descendants(anyTypeName);
                    var redoHistoryItems = pageDoc.Descendants("RedoItems").First().Descendants(anyTypeName);
                    var historyItems = undoHistoryItems.Concat(redoHistoryItems).Where(xe => xe.Descendants("OwnerID").First().Value == studentOwnerID).ToList();

                    // ARR
                    var arraysOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == ARRAY_ENTITY);
                    var trashedArrays = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == ARRAY_ENTITY);

                    var arraysOnPageIDs = arraysOnPage.Select(xe => xe.Descendants("ID").First().Value);
                    var trashedarraysIDs = trashedArrays.Select(xe => xe.Descendants("ID").First().Value);
                    var arraysIDs = arraysOnPageIDs.Concat(trashedarraysIDs);

                    var arraysOnPageCount = arraysOnPage.Count();
                    var trashedArraysCount = trashedArrays.Count();
                    var arraysUsedCount = arraysOnPageCount + trashedArraysCount;

                    var cutHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == CUT_ENTITY).ToList();
                    var arraysWithACutCount = arraysIDs.Count(arraysID => cutHistoryItems.Any(xe => xe.Descendants("CutPageObjectIDs").First().Descendants(stringTypeName).Any(e => e.Value == arraysID)));
                    var cutsOverArrayCount = cutHistoryItems.Count(xe => xe.Descendants("CutPageObjectIDs").First().Descendants(stringTypeName).Any(e => arraysIDs.Contains(e.Value)));

                    var snapHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == SNAP_ENTITY).ToList();
                    var twoArraysSnappedTogetherCount = snapHistoryItems.Count;

                    var divideHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == DIVIDE_ENTITY).ToList();
                    var arrayDividersChangedCount = divideHistoryItems.Count;

                    // NL
                    var numberLinesOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == NUMBER_LINE_ENTITY);
                    var trashedNumberLines = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == NUMBER_LINE_ENTITY);

                    var numberLinesOnPageIDs = numberLinesOnPage.Select(xe => xe.Descendants("ID").First().Value);
                    var trashedNumberLineIDs = trashedNumberLines.Select(xe => xe.Descendants("ID").First().Value);
                    var numberLineIDs = numberLinesOnPageIDs.Concat(trashedNumberLineIDs);

                    var numberLinesOnPageCount = numberLinesOnPage.Count();
                    var trashedNumberLinesCount = trashedNumberLines.Count();
                    var numberLinesUsedCount = numberLinesOnPageCount + trashedNumberLinesCount;

                    var numberLinesWithJumpsOnPageCount = numberLinesOnPage.Count(xe => xe.Descendants("JumpSizes").First().HasElements);
                    var trashedNumberLinesWithJumpsCount = trashedNumberLines.Count(xe => xe.Descendants("JumpSizes").First().HasElements);
                    var numberLinesWithJumpsCount = numberLinesWithJumpsOnPageCount + trashedNumberLinesWithJumpsCount;

                    var endPointsChangedHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == END_POINTS_CHANGED_ENTITY).ToList();
                    var numberLinesWithEndPointsChangedCount = numberLineIDs.Count(numberLineID => endPointsChangedHistoryItems.Any(xe => xe.Descendants("NumberLineID").First().Value == numberLineID));

                    // Sum Stats
                    var isArrayUsedCount = arraysUsedCount > 0 ? 1 : 0;
                    var isNumberLinesUsedCount = numberLinesUsedCount > 0 ? 1 : 0;
                    var isMultipleRepresentations = isArrayUsedCount + isNumberLinesUsedCount > 1 ? "Y" : "N";

                    var isInkOnlyInkOnPage = arraysUsedCount + numberLinesUsedCount == 0 && inkOnPage.Any() ? "Y" : "N";
                    var isInkOnlyCountingErasedInk = arraysUsedCount + numberLinesUsedCount == 0 && (inkOnPage.Any() || trashedInk.Any()) ? "Y" : "N";

                    var isBlank = isArrayUsedCount + isNumberLinesUsedCount == 0 && !inkOnPage.Any() && !trashedInk.Any() ? "Y" : "N";

                    Console.WriteLine($"Name: {studentName}, Page Number: {pageNumber}, Submission Time: {submissionTime}, " +
                                      $"ARR: {arraysUsedCount}, ARR cut: {cutsOverArrayCount}, ARR snap: {twoArraysSnappedTogetherCount}, ARR divide: {arrayDividersChangedCount}" +
                                      $"NL: {numberLinesUsedCount}, NL used: {numberLinesWithJumpsCount}, NLs w/ changed endpoints: {numberLinesWithEndPointsChangedCount}, " +
                                      $"MR: {isMultipleRepresentations}, Ink Only: {isInkOnlyInkOnPage}, Blank: {isBlank}");

                    var rowContents = new List<string>()
                                      {
                                          studentName,
                                          pageNumber,
                                          submissionTime,
                                          arraysUsedCount.ToString(),
                                          cutsOverArrayCount.ToString(),
                                          twoArraysSnappedTogetherCount.ToString(),
                                          arrayDividersChangedCount.ToString(),
                                          numberLinesUsedCount.ToString(),
                                          numberLinesWithJumpsCount.ToString(),
                                          numberLinesWithEndPointsChangedCount.ToString(),
                                          isMultipleRepresentations,
                                          isInkOnlyInkOnPage,
                                          isBlank
                                      };
                    fileRows.Add(rowContents);
                }

                if (allPageNumbers.Any())
                {
                    missingPages.Add(studentName, allPageNumbers);
                }
            }

            #endregion // Generate Stats

            #region Order rows and write to TSV file

            foreach (var studentName in missingPages.Keys)
            {
                var pagesMissing = string.Join(", ", missingPages[studentName]);
                Console.WriteLine("{0} is missing pages: {1}", studentName, pagesMissing);
            }

            var orderedFileRows = fileRows.OrderBy(r => r.First()).ThenBy(r => int.Parse(r[1])).ToList();
            foreach (var orderedFileRow in orderedFileRows)
            {
                var tabbedRow = string.Join("\t", orderedFileRow);
                File.AppendAllText(filePath, Environment.NewLine + tabbedRow);
            }

            #endregion // Order rows and write to TSV file
        }

        #endregion //Commands
    }
}