using System;
using System.Threading.Tasks;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum AnswerDefinitions
    {
        Multiplication,
        Division,
        Addition,
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

        private void InitializeCommands()
        {
            AddPageCommand = new Command(OnAddPageCommandExecute);
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

        /// <summary>Adds a new page to the notebook.</summary>
        public Command AddPageCommand { get; private set; }

        private void OnAddPageCommandExecute()
        {
            var index = Notebook.Pages.IndexOf(CurrentPage);
            index++;
            var page = new CLPPage(App.MainWindowViewModel.CurrentUser);

            _dataService.InsertPageAt(Notebook, page, index);
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
            var previousPage = Notebook.Pages[currentPageIndex - 1];
            CurrentPage.PageNumber--;
            previousPage.PageNumber++;

            Notebook.Pages.MoveItemUp(CurrentPage);
            CurrentPage = Notebook.Pages[currentPageIndex - 1];
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
            var nextPage = Notebook.Pages[currentPageIndex + 1];
            CurrentPage.PageNumber++;
            nextPage.PageNumber--;

            Notebook.Pages.MoveItemDown(CurrentPage);
            CurrentPage = Notebook.Pages[currentPageIndex + 1];
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
            var currentPageIndex = Notebook.Pages.IndexOf(CurrentPage);

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

            if (newPageNumber == currentPageIndex + 1 ||
                newPageNumber == 0)
            {
                return;
            }

            if (newPageNumber > currentPageIndex + 1)
            {
                for (var i = currentPageIndex + 1; i < newPageNumber; i++)
                {
                    var page = Notebook.Pages[i];
                    page.PageNumber--;
                }

                CurrentPage.PageNumber = newPageNumber;
                Notebook.Pages.Move(currentPageIndex, newPageNumber - 1);
            }

            if (newPageNumber < currentPageIndex + 1)
            {
                for (var i = newPageNumber - 1; i < currentPageIndex; i++)
                {
                    var page = Notebook.Pages[i];
                    page.PageNumber++;
                }

                CurrentPage.PageNumber = newPageNumber;
                Notebook.Pages.Move(currentPageIndex, newPageNumber - 1);
            }

            RaisePropertyChanged("CurrentPage");
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
            catch (Exception) { }
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
            originalPage.DifferentiationLevel = "A";
            var index = Notebook.Pages.IndexOf(originalPage);
            Notebook.Pages.Remove(originalPage);
            Notebook.Pages.Insert(index, originalPage);

            foreach (var stroke in originalPage.InkStrokes)
            {
                stroke.SetStrokeDifferentiationGroup(originalPage.DifferentiationLevel);
            }

            for (var i = 1; i < groups; i++)
            {
                var differentiatedPage = originalPage.DuplicatePage();
                differentiatedPage.ID = originalPage.ID;
                differentiatedPage.PageNumber = originalPage.PageNumber;
                differentiatedPage.DifferentiationLevel = "" + (char)('A' + i);

                foreach (var stroke in differentiatedPage.InkStrokes)
                {
                    stroke.SetStrokeDifferentiationGroup(differentiatedPage.DifferentiationLevel);
                }
                Notebook.Pages.Insert(index + i, differentiatedPage);
            }
        }

        /// <summary>Adds a Definiton Tag to the <see cref="CLPPage" />.</summary>
        public Command AddAnswerDefinitionCommand { get; private set; }

        private void OnAddAnswerDefinitionCommandExecute()
        {
            ITag answerDefinition;
            switch (SelectedAnswerDefinition)
            {
                case AnswerDefinitions.Multiplication:
                    answerDefinition = new MultiplicationRelationDefinitionTag(CurrentPage, Origin.Author);

                    var multiplicationViewModel = new MultiplicationRelationDefinitionTagViewModel(answerDefinition as MultiplicationRelationDefinitionTag);
                    var multiplicationView = new MultiplicationRelationDefinitionTagView(multiplicationViewModel)
                                             {
                                                 Owner = Application.Current.MainWindow
                                             };
                    multiplicationView.ShowDialog();

                    if (multiplicationView.DialogResult != true)
                    {
                        return;
                    }

                    ((MultiplicationRelationDefinitionTag)answerDefinition).Factors.Clear();

                    foreach (var relationPart in multiplicationViewModel.Factors)
                    {
                        ((MultiplicationRelationDefinitionTag)answerDefinition).Factors.Add(relationPart);
                    }

                    break;
                case AnswerDefinitions.Division:
                    answerDefinition = new DivisionRelationDefinitionTag(CurrentPage, Origin.Author);

                    var divisionViewModel = new DivisionRelationDefinitionTagViewModel(answerDefinition as DivisionRelationDefinitionTag);
                    var divisionView = new DivisionRelationDefinitionTagView(divisionViewModel)
                                       {
                                           Owner = Application.Current.MainWindow
                                       };
                    divisionView.ShowDialog();

                    if (divisionView.DialogResult != true)
                    {
                        return;
                    }

                    break;
                case AnswerDefinitions.Addition:
                    answerDefinition = new AdditionRelationDefinitionTag(CurrentPage, Origin.Author);

                    var additionViewModel = new AdditionRelationDefinitionTagViewModel(answerDefinition as AdditionRelationDefinitionTag);
                    var additionView = new AdditionRelationDefinitionTagView(additionViewModel)
                                       {
                                           Owner = Application.Current.MainWindow
                                       };
                    additionView.ShowDialog();

                    if (additionView.DialogResult != true)
                    {
                        return;
                    }

                    ((AdditionRelationDefinitionTag)answerDefinition).Addends.Clear();

                    foreach (var relationPart in additionViewModel.Addends)
                    {
                        ((AdditionRelationDefinitionTag)answerDefinition).Addends.Add(relationPart);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        #endregion //Commands
    }
}