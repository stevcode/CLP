using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using ServiceModelEx;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum AnswerDefinitions
    {
        Multiplication,
        Division,
        Addition,
        AllFactorsOfAProduct
    }

    public enum ManualTags
    {
        TroubleWithRemainders,
        FailedSnap
    }

    public class PageInformationPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        public PageInformationPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            SortedTags.Source = Tags;
            SortedTags.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            SortedTags.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));

            Initialized += PageInformationPanelViewModel_Initialized;
            IsVisible = false;

            PageOrientations.Add("Default - Landscape");
            PageOrientations.Add("Default - Portrait");
            PageOrientations.Add("Animation - Landscape");
            PageOrientations.Add("Animation - Portrait");
            SelectedPageOrientation = PageOrientations.First();

            AddPageCommand = new Command(OnAddPageCommandExecute);
            MovePageUpCommand = new Command(OnMovePageUpCommandExecute, OnMovePageUpCanExecute);
            MovePageDownCommand = new Command(OnMovePageDownCommandExecute, OnMovePageDownCanExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute);
            TrimPageCommand = new Command(OnTrimPageCommandExecute);
            SwitchPageLayoutCommand = new Command(OnSwitchPageLayoutCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute);
            DifferentiatePageCommand = new Command(OnDifferentiatePageCommandExecute);
            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            PageScreenshotCommand = new Command(OnPageScreenshotCommandExecute);
            EditTagCommand = new Command<ITag>(OnEditTagCommandExecute);
            DeleteTagCommand = new Command<ITag>(OnDeleteTagCommandExecute);
            AddAnswerDefinitionCommand = new Command(OnAddAnswerDefinitionCommandExecute);
            AddTagCommand = new Command(OnAddTagCommandExecute);
            AnalyzePageCommand = new Command(OnAnalyzePageCommandExecute);
            AnalyzePageHistoryCommand = new Command(OnAnalyzePageHistoryCommandExecute);
            AnalyzeSkipCountingCommand = new Command(OnAnalyzeSkipCountingCommandExecute);

            //TEMP
            InterpretArrayDividersCommand = new Command(OnInterpretArrayDividersCommandExecute);
            PrintAllHistoryItemsCommand = new Command(OnPrintAllHistoryItemsCommandExecute);
            GenerateStampGroupingsCommand = new Command(OnGenerateStampGroupingsCommandExecute);
            FixCommand = new Command(OnFixCommandExecute);
        }

        private void PageInformationPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
            Location = PanelLocations.Right;
        }

        /// <summary>Initial Length of the Panel, before any resizing.</summary>
        public override double InitialLength
        {
            get { return 350.0; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>The Model for this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>Currently selected <see cref="CLPPage" /> of the <see cref="Notebook" />.</summary>
        [ViewModelToModel("Notebook")]
        [Model(SupportIEditableObject = false)]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage), propertyChangedEventHandler: CurrentPageChanged);

        private static void CurrentPageChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var viewModel = sender as PageInformationPanelViewModel;
            if (!advancedPropertyChangedEventArgs.IsNewValueMeaningful ||
                viewModel == null ||
                viewModel.CurrentPage == null)
            {
                return;
            }

            viewModel.SortedTags.Source = viewModel.CurrentPage.Tags;
            viewModel.RaisePropertyChanged("StandardDeviationZScore");
            viewModel.RaisePropertyChanged("AnimationStandardDeviationZScore");
            viewModel.RaisePropertyChanged("FormattedMinMaxAverageHistoryLength");
            viewModel.RaisePropertyChanged("FormattedMinMaxAverageAnimationLength");
        }

        /// <summary>Unique Identifier for the <see cref="CLPPage" />.</summary>
        [ViewModelToModel("CurrentPage")]
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>Version Index of the <see cref="CLPPage" />.</summary>
        [ViewModelToModel("CurrentPage")]
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint));

        /// <summary>DifferentiationLevel of the <see cref="CLPPage" />.</summary>
        [ViewModelToModel("CurrentPage")]
        public string DifferentiationLevel
        {
            get { return GetValue<string>(DifferentiationLevelProperty); }
            set { SetValue(DifferentiationLevelProperty, value); }
        }

        public static readonly PropertyData DifferentiationLevelProperty = RegisterProperty("DifferentiationLevel", typeof(string));

        /// <summary>Page Number of the <see cref="CLPPage" /> within the <see cref="Notebook" />.</summary>
        [ViewModelToModel("CurrentPage")]
        public decimal PageNumber
        {
            get { return GetValue<decimal>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(decimal), 1);

        /// <summary>
        ///     <see cref="ATagBase" />s for the <see cref="CLPPage" />.
        /// </summary>
        [ViewModelToModel("CurrentPage")]
        public ObservableCollection<ITag> Tags
        {
            get { return GetValue<ObservableCollection<ITag>>(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof(ObservableCollection<ITag>), propertyChangedEventHandler: TagsChanged);

        private static void TagsChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var viewModel = sender as PageInformationPanelViewModel;
            if (!advancedPropertyChangedEventArgs.IsNewValueMeaningful ||
                viewModel == null)
            {
                return;
            }

            viewModel.SortedTags.Source = viewModel.Tags;
        }

        #endregion //Model

        #region Bindings

        /// <summary>List of possible page orientations for page creation.</summary>
        public ObservableCollection<string> PageOrientations
        {
            get { return GetValue<ObservableCollection<string>>(PageOrientationsProperty); }
            set { SetValue(PageOrientationsProperty, value); }
        }

        public static readonly PropertyData PageOrientationsProperty = RegisterProperty("PageOrientations",
                                                                                        typeof(ObservableCollection<string>),
                                                                                        () => new ObservableCollection<string>());

        /// <summary>The currently selected Page Orientation.</summary>
        public string SelectedPageOrientation
        {
            get { return GetValue<string>(SelectedPageOrientationProperty); }
            set { SetValue(SelectedPageOrientationProperty, value); }
        }

        public static readonly PropertyData SelectedPageOrientationProperty = RegisterProperty("SelectedPageOrientation", typeof(string));

        /// <summary>Currently selected Answer Definition to add to the page.</summary>
        public AnswerDefinitions SelectedAnswerDefinition
        {
            get { return GetValue<AnswerDefinitions>(SelectedAnswerDefinitionProperty); }
            set { SetValue(SelectedAnswerDefinitionProperty, value); }
        }

        public static readonly PropertyData SelectedAnswerDefinitionProperty = RegisterProperty("SelectedAnswerDefinition", typeof(AnswerDefinitions));

        /// <summary>Currently selected Tag to add to the page.</summary>
        public ManualTags SelectedTag
        {
            get { return GetValue<ManualTags>(SelectedTagProperty); }
            set { SetValue(SelectedTagProperty, value); }
        }

        public static readonly PropertyData SelectedTagProperty = RegisterProperty("SelectedTag", typeof(ManualTags));

        /// <summary>Sorted list of <see cref="ITag" />s by category.</summary>
        public CollectionViewSource SortedTags
        {
            get { return GetValue<CollectionViewSource>(SortedTagsProperty); }
            set { SetValue(SortedTagsProperty, value); }
        }

        public static readonly PropertyData SortedTagsProperty = RegisterProperty("SortedTags", typeof(CollectionViewSource), () => new CollectionViewSource());

        public string FormattedMinMaxAverageHistoryLength
        {
            get
            {
                var parentPage = CurrentPage.SubmissionType == SubmissionTypes.Unsubmitted
                                     ? CurrentPage
                                     : Notebook.Pages.FirstOrDefault(
                                                                     x =>
                                                                     x.ID == CurrentPage.ID && x.DifferentiationLevel == CurrentPage.DifferentiationLevel && x.VersionIndex == 0);

                return string.Format("{0} / {1} / {2}", parentPage.MinSubmissionHistoryLength, parentPage.MaxSubmissionHistoryLength, parentPage.AverageSubmissionHistoryLength);
            }
        }

        public double StandardDeviationZScore
        {
            get
            {
                if (CurrentPage.SubmissionType == SubmissionTypes.Unsubmitted)
                {
                    return Double.NaN;
                }

                var parentPage = Notebook.Pages.FirstOrDefault(x => x.ID == CurrentPage.ID && x.DifferentiationLevel == CurrentPage.DifferentiationLevel && x.VersionIndex == 0);
                if (parentPage == null)
                {
                    return Double.NaN;
                }

                var mean = parentPage.AverageSubmissionHistoryLength;
                var standardDeviation = Math.Sqrt(parentPage.Submissions.Select(x => (double)x.History.HistoryLength).Average(x => Math.Pow(x - mean, 2)));
                var zScore = (CurrentPage.History.HistoryLength - mean) / standardDeviation;
                return Math.Round(zScore, 4, MidpointRounding.AwayFromZero);
            }
        }

        public string FormattedMinMaxAverageAnimationLength
        {
            get
            {
                var parentPage = CurrentPage.SubmissionType == SubmissionTypes.Unsubmitted
                                     ? CurrentPage
                                     : Notebook.Pages.FirstOrDefault(
                                                                     x =>
                                                                     x.ID == CurrentPage.ID && x.DifferentiationLevel == CurrentPage.DifferentiationLevel && x.VersionIndex == 0);

                return string.Format("{0} / {1} / {2}",
                                     parentPage.MinSubmissionAnimationLength,
                                     parentPage.MaxSubmissionAnimationLength,
                                     parentPage.AverageSubmissionAnimationLength);
            }
        }

        public double AnimationStandardDeviationZScore
        {
            get
            {
                if (CurrentPage.SubmissionType == SubmissionTypes.Unsubmitted)
                {
                    return Double.NaN;
                }

                var parentPage = Notebook.Pages.FirstOrDefault(x => x.ID == CurrentPage.ID && x.DifferentiationLevel == CurrentPage.DifferentiationLevel && x.VersionIndex == 0);
                if (parentPage == null)
                {
                    return Double.NaN;
                }

                var mean = parentPage.AverageSubmissionAnimationLength;
                var standardDeviation = Math.Sqrt(parentPage.Submissions.Select(x => x.History.AnimationLength).Average(x => Math.Pow(x - mean, 2)));
                var zScore = (CurrentPage.History.AnimationLength - mean) / standardDeviation;
                return Math.Round(zScore, 4, MidpointRounding.AwayFromZero);
            }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Adds a new page to the notebook.</summary>
        public Command AddPageCommand { get; private set; }

        private void OnAddPageCommandExecute()
        {
            var isPortrait = false;
            var isAnimation = false;
            switch (SelectedPageOrientation)
            {
                case "Default - Landscape":
                    break;
                case "Default - Portrait":
                    isPortrait = true;
                    break;
                case "Animation - Landscape":
                    isAnimation = true;
                    break;
                case "Animation - Portrait":
                    isAnimation = true;
                    isPortrait = true;
                    break;
            }

            var index = Notebook.Pages.IndexOf(CurrentPage);
            index++;
            var page = new CLPPage(App.MainWindowViewModel.CurrentUser);
            if (isPortrait)
            {
                page.Height = CLPPage.PORTRAIT_HEIGHT;
                page.Width = CLPPage.PORTRAIT_WIDTH;
                page.InitialAspectRatio = page.Width / page.Height;
            }
            if (isAnimation)
            {
                page.PageType = PageTypes.Animation;
            }
            Notebook.InsertPageAt(index, page);
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
        }

        private bool OnMovePageUpCanExecute() { return Notebook.Pages.CanMoveItemUp(CurrentPage); }

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
        }

        private bool OnMovePageDownCanExecute() { return Notebook.Pages.CanMoveItemDown(CurrentPage); }

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

        private void OnTrimPageCommandExecute() { CurrentPage.TrimPage(); }

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
        public Command CopyPageCommand { get; private set; }

        private void OnCopyPageCommandExecute()
        {
            var index = Notebook.Pages.IndexOf(CurrentPage);
            index++;

            var newPage = CurrentPage.DuplicatePage();
            Notebook.InsertPageAt(index, newPage);
        }

        public Command DifferentiatePageCommand { get; private set; }

        private void OnDifferentiatePageCommandExecute()
        {
            var numberPageVersions = new KeypadWindowView
                                     {
                                         Owner = Application.Current.MainWindow,
                                         QuestionText =
                                         {
                                             Text = "How many versions of the page?"
                                         },
                                         NumbersEntered =
                                         {
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
            foreach (var pageObject in originalPage.PageObjects)
            {
                pageObject.DifferentiationLevel = originalPage.DifferentiationLevel;
            }
            foreach (var historyItem in originalPage.History.UndoItems)
            {
                historyItem.DifferentiationGroup = originalPage.DifferentiationLevel;
            }
            foreach (var historyItem in originalPage.History.RedoItems)
            {
                historyItem.DifferentiationGroup = originalPage.DifferentiationLevel;
            }
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
                foreach (var pageObject in differentiatedPage.PageObjects)
                {
                    pageObject.DifferentiationLevel = differentiatedPage.DifferentiationLevel;
                }
                foreach (var historyItem in differentiatedPage.History.UndoItems)
                {
                    historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
                }
                foreach (var historyItem in differentiatedPage.History.RedoItems)
                {
                    historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
                }
                foreach (var stroke in differentiatedPage.InkStrokes)
                {
                    stroke.SetStrokeDifferentiationGroup(differentiatedPage.DifferentiationLevel);
                }
                Notebook.Pages.Insert(index + i, differentiatedPage);
            }
        }

        /// <summary>Deletes current page from the notebook.</summary>
        public Command DeletePageCommand { get; private set; }

        private void OnDeletePageCommandExecute()
        {
            var index = Notebook.Pages.IndexOf(CurrentPage);
            if (index == -1)
            {
                return;
            }
            Notebook.RemovePageAt(index);
        }

        /// <summary>Takes and saves a hi-res screenshot of the current page.</summary>
        public Command PageScreenshotCommand { get; private set; }

        private void OnPageScreenshotCommandExecute()
        {
            var pageViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(CurrentPage).First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview);

            var viewManager = Catel.IoC.ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null)
            {
                return;
            }

            var thumbnail = CLPServiceAgent.Instance.UIElementToImageByteArray(pageView, CurrentPage.Width, dpi: 300);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.StreamSource = new MemoryStream(thumbnail);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            var thumbnailsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Thumbnails");
            var thumbnailFilePath = Path.Combine(thumbnailsFolderPath,
                                                 "Page - " + CurrentPage.PageNumber + ";" + CurrentPage.DifferentiationLevel + ";" + CurrentPage.VersionIndex + ";" +
                                                 DateTime.Now.ToString("yyyy-M-d,hh.mm.ss") + ".png");

            if (!Directory.Exists(thumbnailsFolderPath))
            {
                Directory.CreateDirectory(thumbnailsFolderPath);
            }

            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (var outputStream = new MemoryStream())
            {
                pngEncoder.Save(outputStream);
                File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
            }
        }

        /// <summary>Edits an <see cref="ITag" /> on the <see cref="CLPPage" />.</summary>
        public Command<ITag> EditTagCommand { get; private set; }

        private void OnEditTagCommandExecute(ITag tag)
        {
            //var troubleWithRemaindersTag = tag as DivisionTemplateRemainderErrorsTag;
            //if (troubleWithRemaindersTag != null)
            //{
            //    var troubleWithRemaindersVM = new DivisionTemplateTroubleWithRemaindersTagViewModel(troubleWithRemaindersTag);
            //    var troubleWithRemaindersView = new DivisionTemplateTroubleWithRemaindersTagView(troubleWithRemaindersVM)
            //                                    {
            //                                        Owner = Application.Current.MainWindow
            //                                    };
            //    troubleWithRemaindersView.ShowDialog();
            //    return;
            //}
        }

        /// <summary>Deletes an <see cref="ITag" /> from the <see cref="CLPPage" />.</summary>
        public Command<ITag> DeleteTagCommand { get; private set; }

        private void OnDeleteTagCommandExecute(ITag tag)
        {
            if (!CurrentPage.Tags.Contains(tag))
            {
                return;
            }

            CurrentPage.RemoveTag(tag);
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

        /// <summary>Manually adds a Tag to the page.</summary>
        public Command AddTagCommand { get; private set; }

        private void OnAddTagCommandExecute()
        {
            ITag tag = null;
            switch (SelectedTag)
            {
                case ManualTags.TroubleWithRemainders:
                    break;
                case ManualTags.FailedSnap:
                    //var newTag = new DivisionTemplateFailedSnapTag(CurrentPage,
                    //                                               Origin.StudentPageObjectGenerated,
                    //                                               DivisionTemplateFailedSnapTag.AcceptedValues.SnappedArrayTooLarge,
                    //                                               0);
                    //var failedSnapTagVM = new DivisionTemplateFailedSnapTagViewModel(newTag);
                    //var failedSnapView = new DivisionTemplateFailedSnapTagView(failedSnapTagVM)
                    //                     {
                    //                         Owner = Application.Current.MainWindow
                    //                     };
                    //failedSnapView.ShowDialog();

                    //var existingTag = CurrentPage.Tags.OfType<DivisionTemplateFailedSnapTag>().FirstOrDefault(x => x.Value == newTag.Value);
                    //if (existingTag != null)
                    //{
                    //    CurrentPage.RemoveTag(existingTag);
                    //}

                    //CurrentPage.AddTag(newTag);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tag == null)
            {
                return;
            }

            CurrentPage.AddTag(tag);
        }

        /// <summary>Runs analysis routines on the page.</summary>
        public Command AnalyzePageCommand { get; private set; }

        private void OnAnalyzePageCommandExecute()
        {
            PageAnalysis.Analyze(CurrentPage);

            var definitionTags = CurrentPage.Tags.Where(t => t.Category == Category.Definition).ToList();

            if (definitionTags.Any(t => t is AdditionRelationDefinitionTag))
            {
                AdditionRelationAnalysis.Analyze(CurrentPage);
            }
            if (definitionTags.Any(t => t is MultiplicationRelationDefinitionTag))
            {
                MultiplicationRelationAnalysis.Analyze(CurrentPage);
            }
            if (definitionTags.Any(t => t is DivisionRelationDefinitionTag))
            {
                DivisionRelationAnalysis.Analyze(CurrentPage);
            }
            if (definitionTags.Any(t => t is FactorsOfProductDefinitionTag))
            {
                FactorsOfProductAnalysis.Analyze(CurrentPage);
            }

            ArrayAnalysis.Analyze(CurrentPage);
            DivisionTemplateAnalysis.Analyze(CurrentPage);
            StampAnalysis.Analyze(CurrentPage);
            NumberLineAnalysis.Analyze(CurrentPage);
            ApplyInterpretedCorrectness(CurrentPage);

            if (CurrentPage.SubmissionType != SubmissionTypes.Unsubmitted)
            {
                return;
            }

            foreach (var submission in CurrentPage.Submissions)
            {
                PageAnalysis.Analyze(submission);
                ArrayAnalysis.Analyze(submission);
                DivisionTemplateAnalysis.Analyze(submission);
                StampAnalysis.Analyze(submission);
                NumberLineAnalysis.Analyze(CurrentPage);
                ApplyInterpretedCorrectness(submission);
            }
        }

        public static void ApplyInterpretedCorrectness(CLPPage page)
        {
            var correctnessTag = page.Tags.FirstOrDefault(x => x is CorrectnessTag) as CorrectnessTag;
            if (correctnessTag != null &&
                correctnessTag.IsCorrectnessManuallySet)
            {
                return;
            }

            var correctnessTags =
                page.Tags.OfType<DivisionTemplateRepresentationCorrectnessTag>()
                    .Select(divisionTemplateCorrectnessTag => new CorrectnessTag(page, Origin.StudentPageGenerated, divisionTemplateCorrectnessTag.Correctness, true))
                    .ToList();
            correctnessTags.AddRange(
                                     page.Tags.OfType<ArrayCorrectnessSummaryTag>()
                                         .Select(arrayCorrectnessTag => new CorrectnessTag(page, Origin.StudentPageGenerated, arrayCorrectnessTag.Correctness, true)));

            if (!correctnessTags.Any())
            {
                return;
            }

            var correctnessSum = Correctness.Unknown;
            foreach (var tag in correctnessTags)
            {
                if (correctnessSum == tag.Correctness)
                {
                    continue;
                }

                if (correctnessSum == Correctness.Unknown)
                {
                    correctnessSum = tag.Correctness;
                    continue;
                }

                if (correctnessSum == Correctness.Correct &&
                    (tag.Correctness == Correctness.Incorrect || tag.Correctness == Correctness.PartiallyCorrect))
                {
                    correctnessSum = Correctness.PartiallyCorrect;
                    break;
                }

                if (tag.Correctness == Correctness.Correct &&
                    (correctnessSum == Correctness.Incorrect || correctnessSum == Correctness.PartiallyCorrect))
                {
                    correctnessSum = Correctness.PartiallyCorrect;
                    break;
                }
            }

            page.AddTag(new CorrectnessTag(page, Origin.StudentPageGenerated, correctnessSum, true));
        }

        /// <summary>Analyzes the history of the <see cref="CLPPage" /> to determine potential <see cref="ITag" />s.</summary>
        public Command AnalyzePageHistoryCommand { get; private set; }

        private void OnAnalyzePageHistoryCommandExecute()
        {
            var savedTags = CurrentPage.Tags.Where(tag => tag is StarredTag || tag is DottedTag || tag is CorrectnessTag).ToList();
            CurrentPage.Tags = null;
            CurrentPage.Tags = new ObservableCollection<ITag>(savedTags);
            //     SortedTags.Source = CurrentPage.Tags;

            ArrayAnalysis.AnalyzeHistory(CurrentPage);
            DivisionTemplateAnalysis.AnalyzeHistory(CurrentPage);

            if (CurrentPage.SubmissionType != SubmissionTypes.Unsubmitted)
            {
                return;
            }

            foreach (var submission in CurrentPage.Submissions)
            {
                var savedSubmissionTags = submission.Tags.Where(tag => tag is StarredTag || tag is DottedTag || tag is CorrectnessTag).ToList();
                submission.Tags = null;
                submission.Tags = new ObservableCollection<ITag>(savedSubmissionTags);

                ArrayAnalysis.AnalyzeHistory(submission);
                DivisionTemplateAnalysis.AnalyzeHistory(submission);
            }
        }

        /// <summary>
        /// Analyzes ink strokes near array objects to determine if skip counting was used
        /// </summary>
        public Command AnalyzeSkipCountingCommand { get; private set; }

        private void OnAnalyzeSkipCountingCommandExecute()
        {
            var arraysOnPage = CurrentPage.PageObjects.OfType<CLPArray>().ToList();
            var inkOnPage = CurrentPage.InkStrokes;

            foreach (var array in arraysOnPage)
            {
                foreach (var inkStroke in inkOnPage)
                {
                    var skipCountStrokes = new Dictionary<int, StrokeCollection>();

                    //Checks if strokes fall inside array grid
                    var xpos = array.XPosition + array.LabelLength + array.ArrayWidth;
                    var width = 2 * array.LabelLength;
                    var height = array.GridSquareSize;
                    var arrBound = new Rect(array.XPosition, array.YPosition, array.ArrayWidth, array.ArrayHeight);
                    CurrentPage.AddBoundary(arrBound);
                    PageHistory.UISleep(1000);

                    //Thickens ink stroke in consideration
                    inkStroke.DrawingAttributes.Height *= 2;
                    inkStroke.DrawingAttributes.Width *= 2;
                    PageHistory.UISleep(1000);

                    if (inkStroke.HitTest(arrBound, 80))
                    {
                        if (!skipCountStrokes.ContainsKey(-1))
                        {
                            skipCountStrokes.Add(-1, new StrokeCollection());
                        }
                        skipCountStrokes[-1].Add(inkStroke);
                        inkStroke.DrawingAttributes.Height *= 2;
                        inkStroke.DrawingAttributes.Width *= 2;
                        Console.WriteLine("{0},{1}", inkStroke.GetStrokeID(), -1);
                        PageHistory.UISleep(1500);
                        inkStroke.DrawingAttributes.Height /= 2;
                        inkStroke.DrawingAttributes.Width /= 2;
                    }
                    CurrentPage.ClearBoundaries();

                    //Checks if strokes lie next to array rows
                    else
                    {
                        for (int i = 0; i < array.Rows; i++)
                        {
                            var ypos = array.YPosition + array.LabelLength + (array.GridSquareSize * i);
                            var rectBound = new Rect(xpos, ypos - 0.1 * height, 1.2 * width, 1.2 * height);
                            CurrentPage.AddBoundary(rectBound);
                            PageHistory.UISleep(1000);
                            if (inkStroke.HitTest(rectBound, 80))
                            {
                                if (!skipCountStrokes.ContainsKey(i))
                                {
                                    skipCountStrokes.Add(i, new StrokeCollection());
                                }
                                skipCountStrokes[i].Add(inkStroke);

                                inkStroke.DrawingAttributes.Height *= 2;
                                inkStroke.DrawingAttributes.Width *= 2;
                                Console.WriteLine("{0}, {1}", inkStroke.GetStrokeID(), i);
                                PageHistory.UISleep(1500);
                                inkStroke.DrawingAttributes.Height /= 2;
                                inkStroke.DrawingAttributes.Width /= 2;
                            }
                            CurrentPage.ClearBoundaries();
                        }
                    }

                    inkStroke.DrawingAttributes.Height /= 2;
                    inkStroke.DrawingAttributes.Width /= 2;
                }
            }
            //foreach var row in skipCountStrokes.Keys
            //{
            //  var interpretation = CLPPageViewModel.InterpretStrokes(skipCountStrokes[row]);
            //  Console.WriteLin("{0},{1}", row, interpretation);
            //}
        }

        #endregion //Commands

        /// <summary>TEMP</summary>
        public Command InterpretArrayDividersCommand { get; private set; }

        private void OnInterpretArrayDividersCommandExecute()
        {
            var arraysOnPage = CurrentPage.PageObjects.OfType<CLPArray>().ToList();

            foreach (var array in arraysOnPage)
            {
                if (array.ArrayType != ArrayTypes.Array ||
                        !array.IsGridOn)
                {
                    continue;
                }

                var verticalDividers = new List<int> { 0 };
                var horizontalDividers = new List<int> { 0 };
                var cuttableTop = array.YPosition + array.LabelLength;
                var cuttableBottom = cuttableTop + array.ArrayHeight;
                var cuttableLeft = array.XPosition + array.LabelLength;
                var cuttableRight = cuttableLeft + array.ArrayWidth;
                foreach (var stroke in CurrentPage.InkStrokes)
                {
                    var strokeTop = stroke.GetBounds().Top;
                    var strokeBottom = stroke.GetBounds().Bottom;
                    var strokeLeft = stroke.GetBounds().Left;
                    var strokeRight = stroke.GetBounds().Right;

                    const double SMALL_THRESHOLD = 5.0;
                    const double LARGE_THRESHOLD = 15.0;

                    if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                        strokeRight <= cuttableRight &&
                        strokeLeft >= cuttableLeft &&
                        (strokeTop - cuttableTop <= SMALL_THRESHOLD ||
                        cuttableBottom - strokeBottom <= SMALL_THRESHOLD) &&
                        (strokeTop - cuttableTop <= LARGE_THRESHOLD &&
                        cuttableBottom - strokeBottom <= LARGE_THRESHOLD) &&
                        strokeBottom - strokeTop >= cuttableBottom - cuttableTop - LARGE_THRESHOLD &&
                        array.Columns > 1) //Vertical Stroke. Stroke must be within the bounds of the pageObject
                    {
                        var average = (strokeRight + strokeLeft) / 2;
                        var relativeAverage = average - array.LabelLength - array.XPosition;
                        var dividerValue = (int)Math.Round(relativeAverage / array.GridSquareSize);
                        if (dividerValue == 0 ||
                            dividerValue == array.Columns)
                        {
                            continue;
                        }
                        verticalDividers.Add(dividerValue);
                    }

                    if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                             strokeBottom <= cuttableBottom &&
                             strokeTop >= cuttableTop &&
                             (cuttableRight - strokeRight <= SMALL_THRESHOLD ||
                             strokeLeft - cuttableLeft <= SMALL_THRESHOLD) &&
                             (cuttableRight - strokeRight <= LARGE_THRESHOLD &&
                             strokeLeft - cuttableLeft <= LARGE_THRESHOLD) &&
                             strokeRight - strokeLeft >= cuttableRight - cuttableLeft - LARGE_THRESHOLD &&
                             array.Rows > 1) //Horizontal Stroke. Stroke must be within the bounds of the pageObject
                    {
                        var average = (strokeTop + strokeBottom) / 2;
                        var relativeAverage = average - array.LabelLength - array.YPosition;
                        var dividerValue = (int)Math.Round(relativeAverage / array.GridSquareSize);
                        if (dividerValue == 0 ||
                            dividerValue == array.Rows)
                        {
                            continue;
                        }
                        horizontalDividers.Add(dividerValue);
                    }
                }

                verticalDividers.Add(array.Columns);
                verticalDividers = verticalDividers.Distinct().Sort().ToList();
                var verticalDivisions = verticalDividers.Zip(verticalDividers.Skip(1), (x, y) => y - x).ToList();

                horizontalDividers.Add(array.Rows);
                horizontalDividers = horizontalDividers.Distinct().Sort().ToList();
                var horizontalDivisions = horizontalDividers.Zip(horizontalDividers.Skip(1), (x, y) => y - x).ToList();

                if (verticalDivisions.Count > 1 ||
                    horizontalDivisions.Count > 1)
                {
                    var tag = new TempArrayDividersTag(CurrentPage, Origin.StudentPageGenerated);
                    tag.ArrayName = array.Rows + "x" + array.Columns;
                    if (horizontalDivisions.Count > 1)
                        tag.VerticalDividers = horizontalDivisions;
                    if (verticalDivisions.Count > 1)
                        tag.HorizontalDividers = verticalDivisions;
                    CurrentPage.AddTag(tag);
                }
            }
        }

        public Command PrintAllHistoryItemsCommand { get; private set; }

        private void OnPrintAllHistoryItemsCommandExecute()
        {
            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "HistoryLogs");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, PageNameComposite.ParsePageToNameComposite(CurrentPage).ToFileName() + ".txt");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            var historyItems = CurrentPage.History.CompleteOrderedHistoryItems;

            foreach (var item in historyItems)
            {
                File.AppendAllText(filePath, item.FormattedValue + "\n");
            }
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        public Command GenerateStampGroupingsCommand { get; private set; }

        private void OnGenerateStampGroupingsCommandExecute()
        {
            var stampGroups = new Dictionary<Tuple<string,int>,List<string>>();  //<ParentStampID,List of StampedObject IDs>
            foreach (var stampedObject in CurrentPage.PageObjects.OfType<StampedObject>())
            {
                var parentID = stampedObject.ParentStampID;
                var parts = stampedObject.Parts;
                var key = new Tuple<string, int>(parentID, parts);
                if (!stampGroups.ContainsKey(key))
                {
                    stampGroups.Add(key, new List<string>());
                }

                stampGroups[key].Add(stampedObject.ID);
            }

            foreach (var stampGroup in stampGroups)
            {
                var tag = new StampGroupTag(CurrentPage, Origin.StudentPageGenerated, stampGroup.Key.Item1, stampGroup.Key.Item2, stampGroup.Value);
                CurrentPage.AddTag(tag);
            }
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        public Command FixCommand { get; private set; }

        private void OnFixCommandExecute()
        {
            foreach (var dt in CurrentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                var gridSize = dt.ArrayHeight / dt.Rows;


                dt.SizeArrayToGridLevel(gridSize, false);

                var position = 0.0;
                foreach (var division in dt.VerticalDivisions)
                {
                    division.Position = position;
                    division.Length = dt.GridSquareSize * division.Value;
                    position = division.Position + division.Length;
                }

                dt.RaiseAllPropertiesChanged();
            }
        }
    }
}