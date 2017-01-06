using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum AnswerDefinitions
    {
        Multiplication,
        Division,
        Addition,
        Equivalence,
        AllFactorsOfAProduct
    }

    public class AuthoringPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;

        #region Constructor

        public AuthoringPanelViewModel(IDataService dataService)
        {
            _dataService = dataService;

            Notebook = _dataService.CurrentNotebook;

            InitializedAsync += AuthoringPanelViewModel_InitializedAsync;
            ClosedAsync += AuthoringPanelViewModel_ClosedAsync;
            IsVisible = false;

            InitializeCommands();
        }

        private Task AuthoringPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
            Location = PanelLocations.Right;

            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private Task AuthoringPanelViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged -= _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentNotebookChanged(object sender, EventArgs e)
        {
            Notebook = _dataService.CurrentNotebook;
        }

        /// <summary>Initial Length of the Panel, before any resizing.</summary>
        public override double InitialLength => 175.0;

        #endregion //Constructor

        #region Model

        /// <summary>The Model for this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof (Notebook));

        /// <summary>Currently selected <see cref="CLPPage" /> of the <see cref="Notebook" />.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof (CLPPage));

        #endregion //Model

        #region Bindings

        /// <summary>Currently selected Answer Definition to add to the page.</summary>
        public AnswerDefinitions SelectedAnswerDefinition
        {
            get { return GetValue<AnswerDefinitions>(SelectedAnswerDefinitionProperty); }
            set { SetValue(SelectedAnswerDefinitionProperty, value); }
        }

        public static readonly PropertyData SelectedAnswerDefinitionProperty = RegisterProperty("SelectedAnswerDefinition", typeof (AnswerDefinitions), AnswerDefinitions.Multiplication);

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            AddPageCommand = new Command(OnAddPageCommandExecute, OnAddPageCanExecute);
            SwitchPageLayoutCommand = new Command(OnSwitchPageLayoutCommandExecute);
            MovePageUpCommand = new Command(OnMovePageUpCommandExecute, OnMovePageUpCanExecute);
            MovePageDownCommand = new Command(OnMovePageDownCommandExecute, OnMovePageDownCanExecute);
            MovePageToCommand = new Command(OnMovePageToCommandExecute, OnMovePageToCanExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute);
            TrimPageCommand = new Command(OnTrimPageCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
            DuplicatePageCommand = new Command(OnDuplicatePageCommandExecute);
            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            DifferentiatePageCommand = new Command(OnDifferentiatePageCommandExecute);
            AddAnswerDefinitionCommand = new Command(OnAddAnswerDefinitionCommandExecute);
            AddMetaDataTagsCommand = new Command(OnAddMetaDataTagsCommandExecute);
        }

        /// <summary>Adds a new page to the notebook.</summary>
        public Command AddPageCommand { get; private set; }

        private void OnAddPageCommandExecute()
        {
            var index = Notebook.Pages.IndexOf(CurrentPage);
            index++;
            var page = new CLPPage(App.MainWindowViewModel.CurrentUser);

            _dataService.InsertPageAt(Notebook, page, index);
        }

        private bool OnAddPageCanExecute()
        {
            if (CurrentPage == null ||
                CurrentPage.DifferentiationLevel == "0" ||
                !Notebook.Pages.Any())
            {
                return true;
            }

            var lastDifferentiatedPageOfCurrentPage = Notebook.Pages.LastOrDefault(p => p.ID == CurrentPage.ID);
            if (lastDifferentiatedPageOfCurrentPage == null)
            {
                return true;
            }

            return lastDifferentiatedPageOfCurrentPage.DifferentiationLevel == CurrentPage.DifferentiationLevel;
        }

        /// <summary>Converts current page between landscape and portrait.</summary>
        public Command SwitchPageLayoutCommand { get; private set; }

        private void OnSwitchPageLayoutCommandExecute()
        {
            var page = CurrentPage;

            if (Math.Abs(page.InitialAspectRatio - CLPPage.LANDSCAPE_WIDTH / CLPPage.LANDSCAPE_HEIGHT) < 0.01)
            {
                foreach (var pageObject in page.PageObjects)
                {
                    if (pageObject.XPosition + pageObject.Width > CLPPage.PORTRAIT_WIDTH)
                    {
                        pageObject.XPosition = CLPPage.PORTRAIT_WIDTH - pageObject.Width;
                    }
                    if (pageObject.YPosition + pageObject.Height > CLPPage.PORTRAIT_HEIGHT)
                    {
                        pageObject.YPosition = CLPPage.PORTRAIT_HEIGHT - pageObject.Height;
                    }
                }

                page.Width = CLPPage.PORTRAIT_WIDTH;
                page.Height = CLPPage.PORTRAIT_HEIGHT;
                page.InitialAspectRatio = page.Width / page.Height;
            }
            else if (Math.Abs(page.InitialAspectRatio - CLPPage.PORTRAIT_WIDTH / CLPPage.PORTRAIT_HEIGHT) < 0.01)
            {
                foreach (var pageObject in page.PageObjects)
                {
                    if (pageObject.XPosition + pageObject.Width > CLPPage.LANDSCAPE_WIDTH)
                    {
                        pageObject.XPosition = CLPPage.LANDSCAPE_WIDTH - pageObject.Width;
                    }
                    if (pageObject.YPosition + pageObject.Height > CLPPage.LANDSCAPE_HEIGHT)
                    {
                        pageObject.YPosition = CLPPage.LANDSCAPE_HEIGHT - pageObject.Height;
                    }
                }

                page.Width = CLPPage.LANDSCAPE_WIDTH;
                page.Height = CLPPage.LANDSCAPE_HEIGHT;
                page.InitialAspectRatio = page.Width / page.Height;
            }
        }

        /// <summary>Moves the CurrentPage Up in the notebook.</summary>
        public Command MovePageUpCommand { get; private set; }

        private void OnMovePageUpCommandExecute()
        {
            var currentPageIndex = Notebook.Pages.IndexOf(CurrentPage);
            if (currentPageIndex <= 0 ||
                currentPageIndex >= Notebook.Pages.Count)
            {
                return;
            }

            var previousPage = Notebook.Pages[currentPageIndex - 1];
            //var previousPageNumber = previousPage.PageNumber;
            //_dataService.MovePage(Notebook, CurrentPage, previousPageNumber);

            /////
            CurrentPage.PageNumber--;
            previousPage.PageNumber++;

            // TODO: Test if this messes up autosaving
            Notebook.Pages.MoveItemUp(CurrentPage);
            _dataService.AddPageToCurrentDisplay(Notebook.Pages[currentPageIndex - 1]);
            /////

            RaisePropertyChanged(nameof(CurrentPage));
        }

        private bool OnMovePageUpCanExecute()
        {
            return Notebook.Pages.CanMoveItemUp(CurrentPage);
        }

        /// <summary>Moves the CurrentPage Down in the notebook.</summary>
        public Command MovePageDownCommand { get; private set; }

        private void OnMovePageDownCommandExecute()
        {
            var currentPageIndex = Notebook.Pages.IndexOf(CurrentPage);
            if (currentPageIndex < 0 ||
                currentPageIndex >= Notebook.Pages.Count - 1)
            {
                return;
            }

            var nextPage = Notebook.Pages[currentPageIndex + 1];
            //var nextPageNumber = nextPage.PageNumber;
            //_dataService.MovePage(Notebook, CurrentPage, nextPageNumber);

            /////
            CurrentPage.PageNumber++;
            nextPage.PageNumber--;

            // TODO: Test if this messes up autosaving
            Notebook.Pages.MoveItemDown(CurrentPage);
            _dataService.AddPageToCurrentDisplay(Notebook.Pages[currentPageIndex + 1]);

            /////


            RaisePropertyChanged(nameof(CurrentPage));
        }

        private bool OnMovePageDownCanExecute()
        {
            return Notebook.Pages.CanMoveItemDown(CurrentPage);
        }

        /// <summary>Moves page to a specific location.</summary>
        public Command MovePageToCommand { get; private set; }

        private void OnMovePageToCommandExecute()
        {
            var newPageNumberPrompt = new KeypadWindowView("What will this page's new page number be?", Notebook.Pages.Count + 1)
                                     {
                                         Owner = Application.Current.MainWindow,
                                         NumbersEntered = {
                                                              Text = string.Empty
                                                          }
                                     };

            newPageNumberPrompt.ShowDialog();
            if (newPageNumberPrompt.DialogResult != true)
            {
                return;
            }

            var newPageNumber = Convert.ToInt32(newPageNumberPrompt.NumbersEntered.Text);

            _dataService.MovePage(Notebook, CurrentPage, newPageNumber);

            RaisePropertyChanged(nameof(CurrentPage));
        }

        private bool OnMovePageToCanExecute()
        {
            return Notebook.Pages.Count > 1;
        }

        /// <summary>Add 200 pixels to the height of the current page.</summary>
        public Command MakePageLongerCommand { get; private set; }

        private void OnMakePageLongerCommandExecute()
        {
            var initialHeight = CurrentPage.Width / CurrentPage.InitialAspectRatio;
            const int MAX_INCREASE_TIMES = 2;
            const double PAGE_INCREASE_AMOUNT = 200.0;
            if (CurrentPage.Height < initialHeight + PAGE_INCREASE_AMOUNT * MAX_INCREASE_TIMES)
            {
                CurrentPage.Height += PAGE_INCREASE_AMOUNT;
            }

            if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher ||
                App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                App.Network.ProjectorProxy.MakeCurrentPageLonger();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>Trims the current page's excess height if free of ink strokes and pageObjects.</summary>
        public Command TrimPageCommand { get; private set; }

        private void OnTrimPageCommandExecute()
        {
            CurrentPage.TrimPage();
        }

        /// <summary>Completely clears a page of ink strokes and pageObjects.</summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            CurrentPage.History.ClearHistory();
            CurrentPage.PageObjects.Clear();
            CurrentPage.InkStrokes.Clear();
            CurrentPage.SerializedStrokes.Clear();
        }

        /// <summary>Makes a duplicate of the current page.</summary>
        public Command DuplicatePageCommand { get; private set; }

        private void OnDuplicatePageCommandExecute()
        {
            var index = Notebook.Pages.IndexOf(CurrentPage);
            index++;

            var newPage = CurrentPage.DuplicatePage();
            _dataService.InsertPageAt(Notebook, newPage, index);
        }

        /// <summary>Deletes current page from the notebook.</summary>
        public Command DeletePageCommand { get; private set; }

        private void OnDeletePageCommandExecute()
        {
            _dataService.DeletePage(Notebook, CurrentPage);
        }

        public Command DifferentiatePageCommand { get; private set; }

        private void OnDifferentiatePageCommandExecute()
        {
            if (CurrentPage.DifferentiationLevel != "0")
            {
                MessageBox.Show("This page has already been differentiated.");
                return;
            }

            var numberPageVersions = new KeypadWindowView
                                     {
                                         Owner = Application.Current.MainWindow,
                                         QuestionText = {
                                                            Text = "How many versions of the page?"
                                                        },
                                         NumbersEntered = {
                                                              Text = "4"
                                                          }
                                     };

            numberPageVersions.ShowDialog();
            if (numberPageVersions.DialogResult == true)
            {
                Differentiate(Convert.ToInt32(numberPageVersions.NumbersEntered.Text));
            }
        }

        public void Differentiate(int groups)
        {
            var originalPage = CurrentPage;
            var index = Notebook.Pages.IndexOf(originalPage);
            _dataService.DeletePage(Notebook, originalPage, false, false);

            for (var i = 0; i < groups; i++)
            {
                var differentiatedPage = originalPage.DuplicatePage();
                differentiatedPage.ID = originalPage.ID;
                differentiatedPage.PageNumber = originalPage.PageNumber;
                differentiatedPage.DifferentiationLevel = "" + (char)('A' + i);

                _dataService.InsertPageAt(Notebook, differentiatedPage, index + 1, false, false);
            }

            var lastDifferentiatedPage = Notebook.Pages.LastOrDefault(p => p.ID == originalPage.ID);
            _dataService.SetCurrentPage(lastDifferentiatedPage, false);
        }

        /// <summary>Adds a Definiton Tag to the <see cref="CLPPage" />.</summary>
        public Command AddAnswerDefinitionCommand { get; private set; }

        private void OnAddAnswerDefinitionCommandExecute()
        {
            ITag answerDefinition;
            switch (SelectedAnswerDefinition)
            {
                case AnswerDefinitions.Multiplication:
                    var multiplicationDefinition = new MultiplicationRelationDefinitionTag(CurrentPage, Origin.Author);
                    var multiplicationViewModel = new MultiplicationRelationDefinitionTagViewModel(multiplicationDefinition);
                    var multiplicationResult = multiplicationViewModel.ShowWindowAsDialog();

                    if (multiplicationResult != true)
                    {
                        return;
                    }

                    answerDefinition = multiplicationDefinition;
                    break;
                case AnswerDefinitions.Division:
                    var divisionDefinition = new DivisionRelationDefinitionTag(CurrentPage, Origin.Author);
                    var divisionViewModel = new DivisionRelationDefinitionTagViewModel(divisionDefinition);
                    var divisionResult = divisionViewModel.ShowWindowAsDialog();

                    if (divisionResult != true)
                    {
                        return;
                    }

                    answerDefinition = divisionDefinition;
                    break;
                case AnswerDefinitions.Addition:
                    var additionDefinition = new AdditionRelationDefinitionTag(CurrentPage, Origin.Author);
                    var additionViewModel = new AdditionRelationDefinitionTagViewModel(additionDefinition);
                    var additionResult = additionViewModel.ShowWindowAsDialog();

                    if (additionResult != true)
                    {
                        return;
                    }

                    answerDefinition = additionDefinition;
                    break;
                case AnswerDefinitions.Equivalence:
                    var equivalenceDefinition = new EquivalenceRelationDefinitionTag(CurrentPage, Origin.Author);
                    var equivalenceViewModel = new EquivalenceRelationDefinitionTagViewModel(equivalenceDefinition);
                    var equivalenceResult = equivalenceViewModel.ShowWindowAsDialog();

                    if (equivalenceResult != true)
                    {
                        return;
                    }

                    answerDefinition = equivalenceDefinition;
                    break;
                default:
                    return;
            }

            if (answerDefinition == null)
            {
                return;
            }

            CurrentPage.AddTag(answerDefinition);
            if (CurrentPage.SubmissionType != SubmissionTypes.Unsubmitted)
            {
                return;
            }

            foreach (var submission in CurrentPage.Submissions)
            {
                submission.AddTag(answerDefinition);
            }
        }

        /// <summary>Creates Modal Window to edit the Meta Data Tags on the page.</summary>
        public Command AddMetaDataTagsCommand { get; private set; }

        private void OnAddMetaDataTagsCommandExecute()
        {
            var viewModel = new MetaDataTagsViewModel(CurrentPage);
            viewModel.ShowWindowAsDialog();
        }

        #endregion //Commands
    }
}