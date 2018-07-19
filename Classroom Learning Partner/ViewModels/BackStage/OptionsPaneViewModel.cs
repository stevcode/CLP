﻿using System;
using System.Collections.Generic;
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
            ForceWordProblemTagsCommand = new Command(OnForceWordProblemTagsCommandExecute);
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
                    var existingTags = page.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag) && !(t is MetaDataTag)).ToList();
                    foreach (var existingTag in existingTags)
                    {
                        page.RemoveTag(existingTag);
                    }

                    var indexOfPass3Start =
                        page.History.SemanticEvents.IndexOf(page.History.SemanticEvents.First(e => e.CodedObjectID == "3" && e.EventInformation == "Ink Interpretation"));
                    var interpretedInkSemanticEvents = page.History.SemanticEvents.Skip(indexOfPass3Start + 1).ToList();
                    HistoryAnalysis.GenerateTags(page, interpretedInkSemanticEvents);
                    AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);

                    foreach (var submission in page.Submissions)
                    {
                        var existingTagsForSubmission =
                            submission.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag) && !(t is MetaDataTag)).ToList();
                        foreach (var existingTag in existingTagsForSubmission)
                        {
                            submission.RemoveTag(existingTag);
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