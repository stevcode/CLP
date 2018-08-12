using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using Ionic.Zip;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OptionsPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OptionsPaneViewModel(IDataService dataService, IRoleService roleService)
            : base(dataService, roleService)
        {
            InitializeCommands();
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "Options";

        public string VersionText
        {
            get
            {
                var productVersion = Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                var versionText = productVersion?.InformationalVersion;
                var mode = _roleService.Role.ToString();

                return $"CLP Version: {versionText}, Program Mode: {mode}";
            }
        }

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            RunAnalysisCommand = new Command(OnRunAnalysisCommandExecute);
            ClearAuthorHistoryItemsCommand = new Command(OnClearAuthorHistoryItemsCommandExecute);
            AnalyzeAllLoadedPagesCommand = new Command(OnAnalyzeAllLoadedPagesCommandExecute);
            AnalyzeCurrentPageAndSubmissionsCommand = new Command(OnAnalyzeCurrentPageAndSubmissionsCommandExecute);
            RegenerateTagsCommand = new Command(OnRegenerateTagsCommandExecute);
            ForceWordProblemTagsCommand = new Command(OnForceWordProblemTagsCommandExecute);
            ToggleSubmissionModeCommand = new Command(OnToggleSubmissionModeCommandExecute);
            FindIllsCommand = new Command(OnFindIllsCommandExecute);
            DeletePagesCommand = new Command(OnDeletePagesCommandExecute);
        }

        public Command DeletePagesCommand { get; private set; }

        private void OnDeletePagesCommandExecute()
        {
            var pageNumbersToDelete = new List<int>
                                      {
                                          113,
                                          114,
                                          132,
                                          133,
                                          134,
                                          135,
                                          136,
                                          137,
                                          151,
                                          172,
                                          173,
                                          174,
                                          176,
                                          177,
                                          178,
                                          179,
                                          180,
                                          181,
                                          215,
                                          225,
                                          226,
                                          227,
                                          230,
                                          231,
                                          240,
                                          241,
                                          242,
                                          243,
                                          244,
                                          245,
                                          246,
                                          247,
                                          248,
                                          249,
                                          305,
                                          368
                                      };

            var zipEntryFullPaths = new List<string>();
            foreach (var notebook in _dataService.LoadedNotebooks)
            {
                var pagesToDelete = new List<CLPPage>();
                foreach (var page in notebook.Pages)
                {
                    if (pageNumbersToDelete.Contains(page.PageNumber))
                    {
                        pagesToDelete.Add(page);
                    }
                }

                foreach (var pageToDelete in pagesToDelete)
                {
                    notebook.Pages.Remove(pageToDelete);
                    zipEntryFullPaths.Add(pageToDelete.GetZipEntryFullPath(notebook));
                    foreach (var submission in pageToDelete.Submissions)
                    {
                        zipEntryFullPaths.Add(submission.GetZipEntryFullPath(notebook));
                    }
                }
            }

            var zipContainerFilePath = _dataService.LoadedNotebooks.First().ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                foreach (var zipEntryFullPath in zipEntryFullPaths)
                {
                    zip.RemoveEntry(zipEntryFullPath);
                }

                zip.Save();
            }

            MessageBox.Show("Finished deleting pages.");
        }

        public Command ToggleSubmissionModeCommand { get; private set; }

        private void OnToggleSubmissionModeCommandExecute()
        {
            StagingPanelViewModel.IsForcingVersionZeroAsSubmission = !StagingPanelViewModel.IsForcingVersionZeroAsSubmission;
            var text = StagingPanelViewModel.IsForcingVersionZeroAsSubmission
                           ? "Shown submissions will now display Version 0."
                           : "Shown submissions will now be actual submissions.";

            MessageBox.Show(text);
        }

        public Command FindIllsCommand { get; private set; }

        private void OnFindIllsCommandExecute()
        {
            const string FOLDER_NAME = "CLP Reports";
            var folderPath = Path.Combine(DataService.DesktopFolderPath, FOLDER_NAME);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            const string FILE_EXTENSION = "txt";
            var fileName = $"Pages with ILL in Semantic Events.{FILE_EXTENSION}";
            var filePath = Path.Combine(folderPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            foreach (var notebook in _dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent))
            {
                foreach (var page in notebook.Pages)
                {
                    if (page.History.SemanticEvents.Any(e => e.CodedValue.Contains("ILL")))
                    {
                        File.AppendAllText(filePath, $"{page.Owner.DisplayName}, Page {page.PageNumber}\n");
                    }
                }
            }
        }

        /// <summary>Sets the DynamicMainColor of the program to a random color.</summary>
        public Command GenerateRandomMainColorCommand { get; private set; }

        private void OnGenerateRandomMainColorCommandExecute()
        {
            var randomGen = new Random();
            var names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            var randomColorName = names[randomGen.Next(names.Length)];
            var color = Color.FromKnownColor(randomColorName);
            MainWindowViewModel.ChangeApplicationMainColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public Command RunAnalysisCommand { get; private set; }

        private void OnRunAnalysisCommandExecute()
        {
            AnalysisService.RunAnalysisOnLoadedStudentNotebooks(_dataService.LoadedNotebooks);

            MessageBox.Show("Analysis Finished.");
        }

        public Command ClearAuthorHistoryItemsCommand { get; private set; }

        private void OnClearAuthorHistoryItemsCommandExecute()
        {
            foreach (var notebook in _dataService.LoadedNotebooks)
            {
                foreach (var page in notebook.Pages)
                {
                    ClearAuthoredHistoryActions(page);
                    foreach (var submission in page.Submissions)
                    {
                        ClearAuthoredHistoryActions(submission);
                    }
                }
            }

            MessageBox.Show("Finished.");
        }

        private void ClearAuthoredHistoryActions(CLPPage page)
        {
            var undoActionsToRemove = page.History.UndoActions.Where(a => a.OwnerID == Person.AUTHOR_ID).ToList();
            foreach (var historyAction in undoActionsToRemove)
            {
                page.History.UndoActions.Remove(historyAction);
            }

            var redoActionsToRemove = page.History.RedoActions.Where(a => a.OwnerID == Person.AUTHOR_ID).ToList();
            foreach (var historyAction in redoActionsToRemove)
            {
                page.History.RedoActions.Remove(historyAction);
            }

            page.History.RefreshHistoryIndexes();
        }

        public Command AnalyzeAllLoadedPagesCommand { get; private set; }

        private void OnAnalyzeAllLoadedPagesCommandExecute()
        {
            foreach (var notebook in _dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent))
            {
                foreach (var page in notebook.Pages)
                {
                    HistoryAnalysis.GenerateSemanticEvents(page);
                    //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
                    }
                }
            }

            MessageBox.Show("Analysis Finished.");
        }

        public Command AnalyzeCurrentPageAndSubmissionsCommand { get; private set; }

        private void OnAnalyzeCurrentPageAndSubmissionsCommandExecute()
        {
            var currentPage = _dataService.CurrentPage;

            foreach (var notebook in _dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent))
            {
                foreach (var page in notebook.Pages.Where(p => p.PageNumber == currentPage.PageNumber))
                {
                    HistoryAnalysis.GenerateSemanticEvents(page);
                    //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
                    }
                }
            }

            MessageBox.Show("Analysis Finished.");
        }

        public Command RegenerateTagsCommand { get; private set; }

        private void OnRegenerateTagsCommandExecute()
        {
            foreach (var notebook in _dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent))
            {
                foreach (var page in notebook.Pages)
                {
                    var indexOfPass3Start =
                        page.History.SemanticEvents.IndexOf(page.History.SemanticEvents.First(e => e.CodedObjectID == "3" && e.EventInformation == "Ink Interpretation"));
                    var interpretedInkSemanticEvents = page.History.SemanticEvents.Skip(indexOfPass3Start + 1).ToList();
                    HistoryAnalysis.GenerateTags(page, interpretedInkSemanticEvents);
                    //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);

                    foreach (var submission in page.Submissions)
                    {
                        var indexOfPass3StartForSubmission = submission.History.SemanticEvents.IndexOf(submission.History.SemanticEvents.First(e => e.CodedObjectID == "3" && e.EventInformation == "Ink Interpretation"));
                        var interpretedInkSemanticEventsForSubmission = submission.History.SemanticEvents.Skip(indexOfPass3StartForSubmission + 1).ToList();
                        HistoryAnalysis.GenerateTags(submission, interpretedInkSemanticEventsForSubmission);
                        //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
                    }
                }
            }

            MessageBox.Show("Tags Regenerated.");
        }

        public Command ForceWordProblemTagsCommand { get; private set; }

        private void OnForceWordProblemTagsCommandExecute()
        {
            var wordProblemPages = new List<int>
                                   {
                                       5,
                                       6,
                                       10,
                                       11,
                                       12,
                                       13
                                   };

            foreach (var notebook in _dataService.LoadedNotebooks)
            {
                foreach (var page in notebook.Pages)
                {
                    if (page.PageNumber == 1)
                    {
                        continue;
                    }

                    var wordProblemValue = wordProblemPages.Contains(page.PageNumber) ? MetaDataTag.VALUE_TRUE : MetaDataTag.VALUE_FALSE;
                    var wordProblemTag = new MetaDataTag(page, Origin.Author, MetaDataTag.NAME_WORD_PROBLEM, wordProblemValue); 
                    
                    page.AddTag(wordProblemTag);

                    foreach (var submission in page.Submissions)
                    {
                        var wordProblemTag1 = new MetaDataTag(submission, Origin.Author, MetaDataTag.NAME_WORD_PROBLEM, wordProblemValue); 
                        submission.AddTag(wordProblemTag1);
                    }

                    if (page.PageNumber == 3)
                    {
                        var pageDef = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                                      {
                                          RelationType = MultiplicationRelationDefinitionTag
                                                         .RelationTypes.Commutativity,
                                          Product = 63.0
                                      };
                        var firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                          {
                                              NumericValue = 9.0
                                          };
                        var secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                           {
                                               NumericValue = 7.0
                                           };
                        pageDef.Factors.Add(firstFactor);
                        pageDef.Factors.Add(secondFactor);

                        page.AddTag(pageDef);

                        foreach (var submission in page.Submissions)
                        {
                            var pageDef1 = new MultiplicationRelationDefinitionTag(submission, Origin.Author)
                                          {
                                              RelationType = MultiplicationRelationDefinitionTag
                                                             .RelationTypes.Commutativity,
                                              Product = 63.0
                                          };
                            var firstFactor1 = new NumericValueDefinitionTag(submission, Origin.Author)
                                              {
                                                  NumericValue = 9.0
                                              };
                            var secondFactor1 = new NumericValueDefinitionTag(submission, Origin.Author)
                                               {
                                                   NumericValue = 7.0
                                               };
                            pageDef1.Factors.Add(firstFactor1);
                            pageDef1.Factors.Add(secondFactor1);

                            submission.AddTag(pageDef1);
                        }
                    }
                }
            }

            MessageBox.Show("Tags Added.");
        }

        #endregion //Commands
    }
}