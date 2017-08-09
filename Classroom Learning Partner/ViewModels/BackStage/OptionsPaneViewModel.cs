using System;
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

        public OptionsPaneViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            RunAnalysisCommand = new Command(OnRunAnalysisCommandExecute);
            ClearAuthorHistoryItemsCommand = new Command(OnClearAuthorHistoryItemsCommandExecute);
            AnalyzeAllLoadedPagesCommand = new Command(OnAnalyzeAllLoadedPagesCommandExecute);
            AnalyzeCurrentPageAndSubmissionsCommand = new Command(OnAnalyzeCurrentPageAndSubmissionsCommandExecute);
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
                return $"CLP Version: {versionText}";
            }
        }

        #endregion //Bindings

        #region Commands

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

        /// <summary>SUMMARY</summary>
        public Command RunAnalysisCommand { get; private set; }

        private void OnRunAnalysisCommandExecute()
        {
            AnalysisService.RunAnalysisOnLoadedStudentNotebooks(_dataService.LoadedNotebooks);

            MessageBox.Show("Analysis Finished.");
        }

        /// <summary>SUMMARY</summary>
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
                    PageInformationPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        PageInformationPanelViewModel.AnalyzeSkipCountingStatic(submission);
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
                    PageInformationPanelViewModel.AnalyzeSkipCountingStatic(page);
                    foreach (var submission in page.Submissions)
                    {
                        HistoryAnalysis.GenerateSemanticEvents(submission);
                        PageInformationPanelViewModel.AnalyzeSkipCountingStatic(submission);
                    }
                }
            }

            MessageBox.Show("Analysis Finished.");
        }

        #endregion //Commands
    }
}