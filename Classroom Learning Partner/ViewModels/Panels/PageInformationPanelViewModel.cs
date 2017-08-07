using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using CLP.InkInterpretation;
using CLP.MachineAnalysis;
using ServiceModelEx;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum ManualTags
    {
        TroubleWithRemainders,
        FailedSnap
    }

    public enum HistoryAnalysisSteps
    {
        Tags,
        SemanticEvents,
        HistoryActions
    }

    public class PageInformationPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;

        #region Constructor

        public PageInformationPanelViewModel(Notebook notebook, IDataService dataService)
        {
            _dataService = dataService;

            Notebook = notebook;
            SortedTags.Source = Tags;
            SortedTags.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            SortedTags.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));

            InitializedAsync += PageInformationPanelViewModel_InitializedAsync;
            // IsVisible = false;

            PageOrientations.Add("Default - Landscape");
            PageOrientations.Add("Default - Portrait");
            PageOrientations.Add("Animation - Landscape");
            PageOrientations.Add("Animation - Portrait");
            SelectedPageOrientation = PageOrientations.First();

            InitializeCommands();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task PageInformationPanelViewModel_InitializedAsync(object sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Length = InitialLength;
            Location = PanelLocations.Right;
        }

        /// <summary>Initial Length of the Panel, before any resizing.</summary>
        public override double InitialLength => 350.0;

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
            viewModel.RaisePropertyChanged(nameof(StandardDeviationZScore));
            viewModel.RaisePropertyChanged(nameof(AnimationStandardDeviationZScore));
            viewModel.RaisePropertyChanged(nameof(FormattedMinMaxAverageHistoryLength));
            viewModel.RaisePropertyChanged(nameof(FormattedMinMaxAverageAnimationLength));
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
        public int PageNumber
        {
            get { return GetValue<int>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int));

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

        public static readonly PropertyData PageOrientationsProperty = RegisterProperty("PageOrientations", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

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
                if (CurrentPage == null)
                {
                    return string.Empty;
                }

                var submissions = _dataService.GetLoadedSubmissionsForPage(CurrentPage);

                var minSubmissionHistoryLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.HistoryLength).Concat(new[] { int.MaxValue }).Min();
                var maxSubmissionHistoryLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.HistoryLength).Concat(new[] { 0 }).Max();
                var averageSubmissionHistoryLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.HistoryLength).Average());

                return $"{minSubmissionHistoryLength} / {maxSubmissionHistoryLength} / {averageSubmissionHistoryLength}";
            }
        }

        public double StandardDeviationZScore
        {
            get
            {
                if (CurrentPage == null)
                {
                    return double.NaN;
                }

                var submissions = _dataService.GetLoadedSubmissionsForPage(CurrentPage);
                if (!submissions.Any())
                {
                    return double.NaN;
                }

                var averageSubmissionHistoryLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.HistoryLength).Average());
                var standardDeviation = Math.Sqrt(submissions.Select(x => (double)x.History.HistoryLength).Average(x => Math.Pow(x - averageSubmissionHistoryLength, 2)));
                var zScore = (CurrentPage.History.HistoryLength - averageSubmissionHistoryLength) / standardDeviation;
                return Math.Round(zScore, 4, MidpointRounding.AwayFromZero);
            }
        }

        public string FormattedMinMaxAverageAnimationLength
        {
            get
            {
                if (CurrentPage == null)
                {
                    return string.Empty;
                }

                var submissions = _dataService.GetLoadedSubmissionsForPage(CurrentPage);

                var minSubmissionAnimationLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.AnimationLength).Concat(new[] { int.MaxValue }).Min();
                var maxSubmissionAnimationLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.AnimationLength).Concat(new[] { 0 }).Max();
                var averageSubmissionAnimationLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.AnimationLength).Average());

                return $"{minSubmissionAnimationLength} / {maxSubmissionAnimationLength} / {averageSubmissionAnimationLength}";
            }
        }

        public double AnimationStandardDeviationZScore
        {
            get
            {
                if (CurrentPage == null)
                {
                    return double.NaN;
                }

                var submissions = _dataService.GetLoadedSubmissionsForPage(CurrentPage);
                if (!submissions.Any())
                {
                    return double.NaN;
                }

                var averageSubmissionAnimationLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.AnimationLength).Average());
                var standardDeviation = Math.Sqrt(submissions.Select(x => x.History.AnimationLength).Average(x => Math.Pow(x - averageSubmissionAnimationLength, 2)));
                var zScore = (CurrentPage.History.AnimationLength - averageSubmissionAnimationLength) / standardDeviation;
                return Math.Round(zScore, 4, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>Switches bottom list between analysis steps.</summary>
        public HistoryAnalysisSteps CurrentAnalysisStep
        {
            get { return GetValue<HistoryAnalysisSteps>(CurrentAnalysisStepProperty); }
            set { SetValue(CurrentAnalysisStepProperty, value); }
        }

        public static readonly PropertyData CurrentAnalysisStepProperty = RegisterProperty("CurrentAnalysisStep", typeof(HistoryAnalysisSteps), HistoryAnalysisSteps.Tags);

        public bool IsDebuggingFlag
        {
            get { return GetValue<bool>(IsDebuggingFlagProperty); }
            set { SetValue(IsDebuggingFlagProperty, value); }
        }

        public static readonly PropertyData IsDebuggingFlagProperty = RegisterProperty("IsDebuggingFlag", typeof(bool), false);

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            #region Analysis Commands

            GenerateSemanticEventsCommand = new Command(OnGenerateSemanticEventsCommandExecute);
            ShowAnalysisClustersCommand = new Command(OnShowAnalysisClustersCommandExecute);
            ClusterTestCommand = new Command<string>(OnClusterTestCommandExecute);
            ClearTempBoundariesCommand = new Command(OnClearTempBoundariesCommandExecute);
            AnalyzeSkipCountingCommand = new Command(OnAnalyzeSkipCountingCommandExecute);
            AnalyzeSkipCountingWithDebugCommand = new Command(OnAnalyzeSkipCountingWithDebugCommandExecute);
            AnalyzeBottomSkipCountingCommand = new Command(OnAnalyzeBottomSkipCountingCommandExecute);
            ShowInitialBoundariesCommand = new Command(OnShowInitialBoundariesCommandExecute);

            #region Obsolete Commands

            FixCommand = new Command(OnFixCommandExecute);

            #endregion // Obsolete Commands

            #endregion // Analysis Commands

            DeleteTagCommand = new Command<ITag>(OnDeleteTagCommandExecute);
        }

        #region Analysis Commands

        public Command GenerateSemanticEventsCommand { get; private set; }

        private void OnGenerateSemanticEventsCommandExecute()
        {
            var existingTags = CurrentPage.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag)).ToList();
            foreach (var tempArraySkipCountingTag in existingTags)
            {
                CurrentPage.RemoveTag(tempArraySkipCountingTag);
            }

            HistoryAnalysis.GenerateSemanticEvents(CurrentPage);
        }

        public Command ShowAnalysisClustersCommand { get; private set; }

        private void OnShowAnalysisClustersCommandExecute()
        {
            foreach (var cluster in InkSemanticEvents.InkClusters.Where(c => c.ClusterType != InkCluster.ClusterTypes.Ignore))
            {
                var clusterBounds = cluster.Strokes.GetBounds();
                var tempBoundary = new TemporaryBoundary(CurrentPage, clusterBounds.X, clusterBounds.Y, clusterBounds.Height, clusterBounds.Width)
                                   {
                                       RegionText = string.Format("{0}: {1}", cluster.ClusterName, cluster.ClusterType)
                                   };
                CurrentPage.PageObjects.Add(tempBoundary);
            }
        }

        public List<string> ClusterTypes
        {
            get
            {
                return new List<string>
                       {
                           "PointDensity",
                           "CenterDistance",
                           "WeightedCenterDistance",
                           "ClosestPoint",
                           "AveragePointDistance",
                           "StrokeHalves"
                       };
            }
        }

        public Command<string> ClusterTestCommand { get; private set; }

        private void OnClusterTestCommandExecute(string clusterEquation)
        {
            List<StrokeCollection> clusteredStrokes;
            var strokesToCluster = CurrentPage.InkStrokes.Where(s => !s.IsInvisiblySmall()).ToList();
            switch (clusterEquation)
            {
                case "PointDensity":
                    clusteredStrokes = InkClustering.ClusterStrokes(CurrentPage.InkStrokes);
                    break;
                case "CenterDistance":
                    clusteredStrokes = Cluster(strokesToCluster, clusterEquation);
                    break;
                case "WeightedCenterDistance":
                    clusteredStrokes = Cluster(strokesToCluster, clusterEquation);
                    break;
                case "ClosestPoint":
                    clusteredStrokes = Cluster(strokesToCluster, clusterEquation);
                    break;
                case "AveragePointDistance":
                    clusteredStrokes = Cluster(strokesToCluster, clusterEquation);
                    break;
                case "StrokeHalves":
                    clusteredStrokes = Cluster(strokesToCluster, clusterEquation);
                    break;
                default:
                    return;
            }

            if (clusteredStrokes == null ||
                !clusteredStrokes.Any())
            {
                return;
            }

            var tempBoundaries = CurrentPage.PageObjects.OfType<TemporaryBoundary>().ToList();
            foreach (var temporaryBoundary in tempBoundaries)
            {
                CurrentPage.PageObjects.Remove(temporaryBoundary);
            }

            var regionCount = 1;
            foreach (var strokes in clusteredStrokes)
            {
                var clusterBounds = strokes.GetBounds();
                var tempBoundary = new TemporaryBoundary(CurrentPage, clusterBounds.X, clusterBounds.Y, clusterBounds.Height, clusterBounds.Width)
                                   {
                                       RegionText = regionCount.ToString()
                                   };
                regionCount++;
                CurrentPage.PageObjects.Add(tempBoundary);
            }

            // Screenshot Clusters
            PageHistory.UISleep(1000);
            var pageViewModel = CurrentPage.GetAllViewModels().First(x => (x is ACLPPageBaseViewModel) && !((ACLPPageBaseViewModel)x).IsPagePreview);

            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null)
            {
                return;
            }

            var bitmapImage = pageView.ToBitmapImage(CurrentPage.Width, dpi: 300);

            var thumbnailsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Cluster Screenshots");
            var thumbnailFileName = string.Format("{0}, Page {1};{2} - Cluster {3}.png", CurrentPage.Owner.FullName, CurrentPage.PageNumber, CurrentPage.VersionIndex, clusterEquation);
            var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, thumbnailFileName);

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

        public static List<StrokeCollection> Cluster(List<Stroke> strokes, string clusterEquation)
        {
            Func<Stroke, Stroke, double> distanceEquation;
            switch (clusterEquation)
            {
                case "CenterDistance":
                    distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByCenter(s2));
                    break;
                case "WeightedCenterDistance":
                    distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByWeightedCenter(s2));
                    break;
                case "ClosestPoint":
                    distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByClosestPoint(s2));
                    break;
                case "AveragePointDistance":
                    distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByAveragePointDistance(s2));
                    break;
                case "StrokeHalves":
                    distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByAveragePointDistanceOfStrokeHalves(s2));
                    break;
                default:
                    return null;
            }

            var maxEpsilon = 1000;
            var minimumStrokesInCluster = 1;
            var optics = new OPTICS<Stroke>(maxEpsilon, minimumStrokesInCluster, strokes, distanceEquation);
            optics.BuildReachability();
            var reachabilityDistances = optics.ReachabilityDistances().ToList();

            #region K-Means Clustering

            var normalizedReachabilityPlot = reachabilityDistances.Select(i => new Point(0, i.ReachabilityDistance)).Skip(1).ToList();
            var plotView = new OPTICSReachabilityPlotView()
                           {
                               Owner = Application.Current.MainWindow,
                               WindowStartupLocation = WindowStartupLocation.Manual,
                               Reachability = normalizedReachabilityPlot
                           };
            plotView.Show();

            //var rawData = new double[normalizedReachabilityPlot.Count][];
            //for (var i = 0; i < rawData.Length; i++)
            //{
            //    rawData[i] = new[] { 0.0, normalizedReachabilityPlot[i].Y };
            //}

            //var clustering = InkClustering.K_MEANS_Clustering(rawData, 2);

            //var zeroCount = 0;
            //var zeroTotal = 0.0;
            //var oneCount = 0;
            //var oneTotal = 0.0;
            //for (var i = 0; i < clustering.Length; i++)
            //{
            //    if (clustering[i] == 0)
            //    {
            //        zeroCount++;
            //        zeroTotal += normalizedReachabilityPlot[i].Y;
            //    }
            //    if (clustering[i] == 1)
            //    {
            //        oneCount++;
            //        oneTotal += normalizedReachabilityPlot[i].Y;
            //    }
            //}
            //var zeroMean = zeroTotal / zeroCount;
            //var oneMean = oneTotal / oneCount;
            //var clusterWithHighestMean = zeroMean > oneMean ? 0 : 1;

            #endregion // K-Means Clustering

            const double CLUSTERING_EPSILON = 51.0;

            var currentCluster = new StrokeCollection();
            var allClusteredStrokes = new List<Stroke>();
            var firstStrokeIndex = (int)reachabilityDistances[0].OriginalIndex;
            var firstStroke = strokes[firstStrokeIndex];
            currentCluster.Add(firstStroke);
            allClusteredStrokes.Add(firstStroke);

            var strokeClusters = new List<StrokeCollection>();
            for (var i = 1; i < reachabilityDistances.Count(); i++)
            {
                var strokeIndex = (int)reachabilityDistances[i].OriginalIndex;
                var stroke = strokes[strokeIndex];

                // K-Means cluster decision.
                //if (clustering[i - 1] != clusterWithHighestMean)
                //{
                //    currentCluster.Add(stroke);
                //    allClusteredStrokes.Add(stroke);
                //    continue;
                //}

                // Epsilon cluster decision.
                var currentReachabilityDistance = reachabilityDistances[i].ReachabilityDistance;
                if (currentReachabilityDistance < CLUSTERING_EPSILON)
                {
                    currentCluster.Add(stroke);
                    allClusteredStrokes.Add(stroke);
                    continue;
                }

                var fullCluster = currentCluster.ToList();
                currentCluster.Clear();
                currentCluster.Add(stroke);
                allClusteredStrokes.Add(stroke);
                strokeClusters.Add(new StrokeCollection(fullCluster));
            }
            if (currentCluster.Any())
            {
                var finalCluster = currentCluster.ToList();
                strokeClusters.Add(new StrokeCollection(finalCluster));
            }

            var unclusteredStrokes = new StrokeCollection();
            foreach (var stroke in strokes)
            {
                if (allClusteredStrokes.Contains(stroke))
                {
                    continue;
                }
                unclusteredStrokes.Add(stroke); // TODO: do something with these.
            }

            return strokeClusters;
        }

        public Command ClearTempBoundariesCommand { get; private set; }

        private void OnClearTempBoundariesCommandExecute()
        {
            var tempBoundaries = CurrentPage.PageObjects.OfType<TemporaryBoundary>().ToList();
            foreach (var temporaryBoundary in tempBoundaries)
            {
                CurrentPage.PageObjects.Remove(temporaryBoundary);
            }

            var tempGrids = CurrentPage.PageObjects.OfType<TemporaryGrid>().ToList();
            foreach (var tempGrid in tempGrids)
            {
                CurrentPage.PageObjects.Remove(tempGrid);
            }

            foreach (var stroke in CurrentPage.InkStrokes)
            {
                var drawingAttributes = stroke.DrawingAttributes;
                // var color = drawingAttributes.Color;
                // color.A = 255;
                drawingAttributes.Color = Colors.Black;
                // stroke.DrawingAttributes = drawingAttributes;
            }
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used</summary>
        public Command AnalyzeSkipCountingCommand { get; private set; }

        private void OnAnalyzeSkipCountingCommandExecute()
        {
            AnalyzeSkipCountingStatic(CurrentPage, IsDebuggingFlag);
        }

        public static void AnalyzeSkipCountingStatic(CLPPage page, bool isDebuggingFlag = false)
        {
            var existingTags = page.Tags.OfType<TempArraySkipCountingTag>().ToList();
            foreach (var tempArraySkipCountingTag in existingTags)
            {
                page.RemoveTag(tempArraySkipCountingTag);
            }

            var arraysOnPage = page.PageObjects.OfType<CLPArray>().ToList();

            //Iterates over arrays on page
            foreach (var array in arraysOnPage)
            {
                var formattedSkips = ArraySemanticEvents.StaticSkipCountAnalysis(page, array, isDebuggingFlag);
                if (string.IsNullOrEmpty(formattedSkips))
                {
                    continue;
                }

                var unformattedSkips = formattedSkips.TrimAll().Split("\"\"", StringSplitOptions.None).Select(s => s.Replace("\"", string.Empty)).ToList();
                var heuristicsResults = ArraySemanticEvents.Heuristics(unformattedSkips, array.Rows, array.Columns);

                var tag = new TempArraySkipCountingTag(page, Origin.StudentPageGenerated)
                          {
                              CodedID = array.CodedID,
                              RowInterpretations = formattedSkips,
                              HeuristicsResults = heuristicsResults
                          };

                CLogger.AppendToLog(tag.FormattedValue);

                page.AddTag(tag);
            }
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used, with forced Debug Mode</summary>
        public Command AnalyzeSkipCountingWithDebugCommand { get; private set; }

        private void OnAnalyzeSkipCountingWithDebugCommandExecute()
        {
            IsDebuggingFlag = true;
            OnAnalyzeSkipCountingCommandExecute();
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used</summary>
        public Command AnalyzeBottomSkipCountingCommand { get; private set; }

        private void OnAnalyzeBottomSkipCountingCommandExecute()
        {
            var existingTags = CurrentPage.Tags.OfType<TempArraySkipCountingTag>().ToList();
            foreach (var tempArraySkipCountingTag in existingTags)
            {
                CurrentPage.RemoveTag(tempArraySkipCountingTag);
            }

            var arraysOnPage = CurrentPage.PageObjects.OfType<CLPArray>().ToList();

            //Iterates over arrays on page
            foreach (var array in arraysOnPage)
            {
                var formattedSkips = ArraySemanticEvents.StaticBottomSkipCountAnalysis(CurrentPage, array, IsDebuggingFlag);
                if (string.IsNullOrEmpty(formattedSkips))
                {
                    continue;
                }

                var expectedValue = string.Empty;
                for (var i = 1; i <= array.Columns; i++)
                {
                    expectedValue += i * array.Rows;
                }

                var expectedValueForWrongDimension = string.Empty;
                for (var i = 1; i <= array.Columns; i++)
                {
                    expectedValueForWrongDimension += i * array.Columns;
                }

                var editDistance = EditDistance.Compute(expectedValue, formattedSkips);
                var wrongDimensionEditDistance = EditDistance.Compute(expectedValueForWrongDimension, formattedSkips);
                //formattedSkips += "   ED: " + editDistance + "   WED: " + wrongDimensionEditDistance;

                if (editDistance > 4)
                {
                    continue;
                }

                var tag = new TempArraySkipCountingTag(CurrentPage, Origin.StudentPageGenerated)
                          {
                              CodedID = array.CodedID,
                              RowInterpretations = formattedSkips
                          };

                CurrentPage.AddTag(tag);
            }
        }

        public Command ShowInitialBoundariesCommand { get; private set; }

        private void OnShowInitialBoundariesCommandExecute()
        {
            //var bounds = CurrentPage.InkStrokes.GetBounds();
            //var tempyBoundary = new TemporaryBoundary(CurrentPage, bounds.X, bounds.Y, bounds.Height, bounds.Width)
            //{
            //    RegionText = "Accepted Boundary"
            //};

            //CurrentPage.PageObjects.Add(tempyBoundary);

            //CLogger.AppendToLog("Top: {0}", bounds.Y);
            //CLogger.AppendToLog("Bottom: {0}", bounds.Y + bounds.Height);
            //CLogger.AppendToLog("Left: {0}", bounds.X);
            //CLogger.AppendToLog("Right: {0}", bounds.X + bounds.Width);

            //foreach (var array in CurrentPage.PageObjects.OfType<CLPArray>().ToList())
            //{
            //    var arrayBottom = array.YPosition + array.Height - array.LabelLength;
            //    CLogger.AppendToLog("Top Delta: {0}", arrayBottom - bounds.Y);
            //    CLogger.AppendToLog("Bottom Delta: {0}", bounds.Y + bounds.Height - arrayBottom);
            //}

            //return;

            const double RIGHT_OF_VISUAL_RIGHT_THRESHOLD = 80.0;
            const double LEFT_OF_VISUAL_RIGHT_THRESHOLD = 41.5;

            var arraysOnPage = CurrentPage.PageObjects.OfType<CLPArray>().ToList();
            CurrentPage.ClearBoundaries();

            //Iterates over arrays on page
            foreach (var array in arraysOnPage)
            {
                var arrayVisualRight = array.XPosition + array.Width - array.LabelLength;
                var arrayVisualTop = array.YPosition + array.LabelLength;
                var halfGridSquareSize = array.GridSquareSize * 0.5;
                var xpos = arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD;
                var ypos = arrayVisualTop - halfGridSquareSize;
                var width = LEFT_OF_VISUAL_RIGHT_THRESHOLD + RIGHT_OF_VISUAL_RIGHT_THRESHOLD;
                if (xpos + width > CurrentPage.Width)
                {
                    width = CurrentPage.Width - xpos;
                }

                var height = array.GridSquareSize * (array.Rows + 1);
                if (ypos + height > CurrentPage.Height)
                {
                    height = CurrentPage.Height - ypos;
                }

                var acceptedBoundary = new Rect(xpos, ypos, width, height);

                var tempBoundary = new TemporaryBoundary(CurrentPage, acceptedBoundary.X, acceptedBoundary.Y, acceptedBoundary.Height, acceptedBoundary.Width)
                                   {
                                       RegionText = "Accepted Boundary"
                                   };

                CurrentPage.PageObjects.Add(tempBoundary);
            }
        }

        #region Obsolete Commands

        public Command FixCommand { get; private set; }

        private void OnFixCommandExecute()
        {
            foreach (var dt in CurrentPage.PageObjects.OfType<DivisionTemplate>())
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
            // var output = strokes.Select(s => string.Format("Weight: {0}, Num Points: {1}", s.StrokeWeight(), s.StylusPoints.Count)).ToList();
            // foreach (var line in output)
            // {
            // CLogger.AppendToLog(line);
            // }
        }

        #endregion // Obsolete Commands 

        #endregion // Analysis Commands

        /// <summary>Deletes the currently selected Tag.</summary>
        public Command<ITag> DeleteTagCommand { get; private set; }

        private void OnDeleteTagCommandExecute(ITag tag)
        {
            CurrentPage.RemoveTag(tag);
        }

        #endregion //Commands
    }
}