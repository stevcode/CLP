using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OptionsPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OptionsPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands()
        {
            BatchTagAnalysisCommand = new Command(OnBatchTagAnalysisCommandExecute);
            BatchRepresentationsUsedTagCommand = new Command(OnBatchRepresentationsUsedTagCommandExecute);
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            ApplyRenameToCacheCommand = new Command(OnApplyRenameToCacheCommandExecute);
            ToggleBindingStyleCommand = new Command(OnToggleBindingStyleCommandExecute);
            ReplayHistoryCommand = new Command(OnReplayHistoryCommandExecute);
            GenerateSubmissionsCommand = new Command(OnGenerateSubmissionsCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Options"; }
        }

        #endregion //Bindings

        #region Commands

        public Command BatchTagAnalysisCommand { get; private set; }

        private void OnBatchTagAnalysisCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            dataService.Analyze();
        }

        /// <summary>SUMMARY</summary>
        public Command BatchRepresentationsUsedTagCommand { get; private set; }

        private void OnBatchRepresentationsUsedTagCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "Batch Results");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, "Batch Representations Used Tags.txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "*****Representations Used Tags*****" + "\n\n");

            var totalARR = 0;
            var totalARRdeleted = 0;
            var totalARRfinal = 0;
            var totalSKIP = 0;
            var totalNL = 0;
            var totalNLdeleted = 0;
            var totalNLfinal = 0;
            var totalStampedImages = 0;
            var totalStampedImagesDeleted = 0;
            var totalStampedImagesFinal = 0;
            var pagesWithARR = 0;
            var pagesWithARRDeleted = 0;
            var pagesWithARRFinal = 0;
            var pagesWithSKIP = 0;
            var pagesWithNL = 0;
            var pagesWithNLDeleted = 0;
            var pagesWithNLFinal = 0;
            var pagesWithStampedImages = 0;
            var pagesWithStampedImagesDeleted = 0;
            var pagesWithStampedImagesFinal = 0;
            var totalPages = 0;
            var totalPagesWithReps = 0;
            var totalPagesWithRepsDeleted = 0;
            var totalPagesWithRepsFinal = 0;

            foreach (var notebookInfo in dataService.LoadedNotebooksInfo)
            {
                var notebook = notebookInfo.Notebook;
                if (notebook.OwnerID == Person.Author.ID ||
                    !notebook.Owner.IsStudent)
                {
                    continue;
                }

                foreach (var page in notebook.Pages)
                {
                    var lastSubmission = page.Submissions.LastOrDefault();
                    if (lastSubmission == null)
                    {
                        continue;
                    }

                    totalPages++;

                    Console.WriteLine("Generating Representations Used Tag for page {0}, for {1}", page.PageNumber, page.Owner.FullName);
                    HistoryAnalysis.GenerateHistoryActions(lastSubmission);
                    Console.WriteLine("Finished generating Representations Used Tag.\n");

                    var tag = lastSubmission.Tags.FirstOrDefault(t => t is RepresentationsUsedTag) as RepresentationsUsedTag;
                    if (tag == null)
                    {
                        continue;
                    }

                    if (tag.AllRepresentations.Any())
                    {
                        totalPagesWithReps++;
                    }
                    if (tag.DeletedCodedRepresentations.Any())
                    {
                        totalPagesWithRepsDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any())
                    {
                        totalPagesWithRepsFinal++;
                    }

                    totalARRdeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("ARR"));
                    totalARRfinal += tag.FinalCodedRepresentations.Count(c => c.Contains("ARR"));
                    totalARR += totalARRdeleted + totalARRfinal;
                    totalSKIP += tag.FinalCodedRepresentations.Count(c => c.Contains("skip"));
                    totalNLdeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("NL"));
                    totalNLfinal += tag.FinalCodedRepresentations.Count(c => c.Contains("NL"));
                    totalNL += totalARRdeleted + totalARRfinal;
                    totalStampedImagesDeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("STAMP"));
                    totalStampedImagesFinal += tag.FinalCodedRepresentations.Count(c => c.Contains("STAMP"));
                    totalStampedImages += totalStampedImagesDeleted + totalStampedImagesFinal;

                    if (tag.AllRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARR++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARRDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARRFinal++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("skip")))
                    {
                        pagesWithSKIP++;
                    }
                    if (tag.AllRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNL++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNLDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNLFinal++;
                    }
                    if (tag.AllRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImages++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImagesDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImagesFinal++;
                    }

                    var tagLine = string.Format("{0}, p {1}:\n{2}", page.Owner.FullName, page.PageNumber, tag.FormattedValue);

                    File.AppendAllText(filePath, tagLine + "\n\n");
                }

                File.AppendAllText(filePath, "*****\n");
            }


            // return total instances, deleted instances, and final instances counts instead of just total
            // same 3 states for final (only report FINAL for stamps)
            File.AppendAllText(filePath, "\n*****Page Stats*****\n\n");

            File.AppendAllText(filePath, string.Format("Total Pages: {0}\n", totalPages));
            File.AppendAllText(filePath, string.Format("Total Pages with Representations Used: {0}\n", totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total Pages with (deleted) Representations Used: {0}\n", totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total Pages with (final) Representations Used: {0}\n", totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total Pages with Ink Only: {0}\n", totalPages - totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total ARR instances: {0}\n", totalARR));
            File.AppendAllText(filePath, string.Format("Total ARR instances (deleted): {0}\n", totalARRdeleted));
            File.AppendAllText(filePath, string.Format("Total ARR instances (final): {0}\n", totalARRfinal));
            File.AppendAllText(filePath, string.Format("Total ARR skip instances (in Final Representations): {0}\n", totalSKIP));
            File.AppendAllText(filePath, string.Format("Total NL instances: {0}\n", totalNL));
            File.AppendAllText(filePath, string.Format("Total NL instances (deleted): {0}\n", totalNLdeleted));
            File.AppendAllText(filePath, string.Format("Total NL instances (final): {0}\n", totalNLfinal));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (probably inaccurate): {0}\n", totalStampedImages));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (deleted, probably inaccurate): {0}\n", totalStampedImagesDeleted));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (final): {0}\n", totalStampedImagesFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances: {0}\n", pagesWithARR));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances (deleted): {0}\n", pagesWithARRDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances (final): {0}\n", pagesWithARRFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR skip instances (in Final Representations): {0}\n", pagesWithSKIP));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances: {0}\n", pagesWithNL));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances (deleted): {0}\n", pagesWithNLDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances (final): {0}\n", pagesWithNLFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (probably inaccurate: {0}\n", pagesWithStampedImages));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (deleted, probably inaccurate): {0}\n", pagesWithStampedImagesDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (final): {0}\n", pagesWithStampedImagesFinal));
        }

        /// <summary>Sets the DynamicMainColor of the program to a random color.</summary>
        public Command GenerateRandomMainColorCommand { get; private set; }

        private void OnGenerateRandomMainColorCommandExecute()
        {
            var randomGen = new Random();
            var names = (KnownColor[])Enum.GetValues(typeof (KnownColor));
            var randomColorName = names[randomGen.Next(names.Length)];
            var color = Color.FromKnownColor(randomColorName);
            MainWindowViewModel.ChangeApplicationMainColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)); 
        }

        /// <summary>
        /// Completely renames a Person throughout the entire cache.
        /// </summary>
        public Command ApplyRenameToCacheCommand { get; private set; }

        private void OnApplyRenameToCacheCommandExecute()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            const string NEW_NAME = "Elvis Garzona";
            App.MainWindowViewModel.CurrentUser.FullName = NEW_NAME;
            var newPerson = App.MainWindowViewModel.CurrentUser;
            var notebook = notebookService.CurrentNotebook;
            notebook.Owner = newPerson;
            foreach (var page in notebook.Pages)
            {
                page.Owner = newPerson;
                foreach (var submission in page.Submissions)
                {
                    submission.Owner = newPerson;
                }
            }
        }

        /// <summary>
        /// Toggles the style used by pageObjects for their boundary.
        /// </summary>
        public Command ToggleBindingStyleCommand { get; private set; }

        private void OnToggleBindingStyleCommandExecute() { App.MainWindowViewModel.IsUsingOldPageObjectBoundary = !App.MainWindowViewModel.IsUsingOldPageObjectBoundary; }

        /// <summary>Replays the interaction history of the page on the Grid Display.</summary>
        public Command ReplayHistoryCommand { get; private set; }

        private void OnReplayHistoryCommandExecute()
        {
            var animationControlRibbon = NotebookWorkspaceViewModel.GetAnimationControlRibbon();
            if (animationControlRibbon == null)
            {
                return;
            }

            animationControlRibbon.IsNonAnimationPlaybackEnabled = !animationControlRibbon.IsNonAnimationPlaybackEnabled;
        }

        /// <summary>Generates submissions for pages with no submissions.</summary>
        public Command GenerateSubmissionsCommand { get; private set; }

        private void OnGenerateSubmissionsCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.GenerateSubmissionsFromModifiedStudentPages();
                                  },
                                  null,
                                  "Generating Submissions");
        }

        #endregion //Commands
    }
}