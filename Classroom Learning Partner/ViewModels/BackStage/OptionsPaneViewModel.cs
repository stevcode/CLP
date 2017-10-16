using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

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
                    AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
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
                    AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
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
                    var existingTags = page.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag)).ToList();
                    foreach (var tempArraySkipCountingTag in existingTags)
                    {
                        page.RemoveTag(tempArraySkipCountingTag);
                    }

                    var indexOfPass3Start =
                        page.History.SemanticEvents.IndexOf(page.History.SemanticEvents.First(e => e.CodedObjectID == "3" && e.EventInformation == "Ink Interpretation"));
                    var interpretedInkSemanticEvents = page.History.SemanticEvents.Skip(indexOfPass3Start + 1).ToList();
                    HistoryAnalysis.GenerateTags(page, interpretedInkSemanticEvents);
                    AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);

                    foreach (var submission in page.Submissions)
                    {
                        var existingTagsForSubmission = submission.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag)).ToList();
                        foreach (var tempArraySkipCountingTag in existingTagsForSubmission)
                        {
                            submission.RemoveTag(tempArraySkipCountingTag);
                        }

                        var indexOfPass3StartForSubmission = submission.History.SemanticEvents.IndexOf(submission.History.SemanticEvents.First(e => e.CodedObjectID == "3" && e.EventInformation == "Ink Interpretation"));
                        var interpretedInkSemanticEventsForSubmission = submission.History.SemanticEvents.Skip(indexOfPass3StartForSubmission + 1).ToList();
                        HistoryAnalysis.GenerateTags(submission, interpretedInkSemanticEventsForSubmission);
                        AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);

                        AnalysisPanelViewModel.AnalyzeSkipCountingStatic(submission);
                    }
                }
            }

            MessageBox.Show("Tags Regenerated.");
        }

        #endregion //Commands
    }
}