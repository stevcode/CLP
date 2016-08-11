using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        HistoryActions,
        HistoryItems
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

            InitializedAsync += PageInformationPanelViewModel_InitializedAsync;
            IsVisible = false;

            PageOrientations.Add("Default - Landscape");
            PageOrientations.Add("Default - Portrait");
            PageOrientations.Add("Animation - Landscape");
            PageOrientations.Add("Animation - Portrait");
            SelectedPageOrientation = PageOrientations.First();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            #region Analysis Commands

            GenerateHistoryActionsCommand = new Command(OnGenerateHistoryActionsCommandExecute);
            ShowAnalysisClustersCommand = new Command(OnShowAnalysisClustersCommandExecute);
            ClusterTestCommand = new Command<string>(OnClusterTestCommandExecute);
            ClearTempBoundariesCommand = new Command(OnClearTempBoundariesCommandExecute);
            StrokeTestingCommand = new Command(OnStrokeTestingCommandExecute);
            v1Command = new Command(Onv1CommandExecute);
            v2Command = new Command(Onv2CommandExecute);
            AnalyzeSkipCountingCommand = new Command(OnAnalyzeSkipCountingCommandExecute);
            AnalyzeSkipCountingWithDebugCommand = new Command(OnAnalyzeSkipCountingWithDebugCommandExecute);
            AnalyzeBottomSkipCountingCommand = new Command(OnAnalyzeBottomSkipCountingCommandExecute);
            TSVCommand = new Command(OnTSVCommandExecute);
            ShowInitialBoundariesCommand = new Command(OnShowInitialBoundariesCommandExecute);

            #region Obsolete Commands

            AnalyzePageCommand = new Command(OnAnalyzePageCommandExecute);
            AnalyzePageHistoryCommand = new Command(OnAnalyzePageHistoryCommandExecute);
            PrintAllHistoryItemsCommand = new Command(OnPrintAllHistoryItemsCommandExecute);
            FixCommand = new Command(OnFixCommandExecute);

            #endregion // Obsolete Commands

            #endregion // Analysis Commands
        }

        private async Task PageInformationPanelViewModel_InitializedAsync(object sender, EventArgs e)
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
                var dataService = DependencyResolver.Resolve<IDataService>();
                if (dataService == null)
                {
                    return string.Empty;
                }

                var submissions = dataService.GetLoadedSubmissionsForTeacherPage(Notebook.ID, CurrentPage.ID, CurrentPage.DifferentiationLevel);

                var minSubmissionHistoryLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.HistoryLength).Concat(new[] { int.MaxValue }).Min();
                var maxSubmissionHistoryLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.HistoryLength).Concat(new[] { 0 }).Max();
                var averageSubmissionHistoryLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.HistoryLength).Average());

                return string.Format("{0} / {1} / {2}", minSubmissionHistoryLength, maxSubmissionHistoryLength, averageSubmissionHistoryLength);
            }
        }

        public double StandardDeviationZScore
        {
            get
            {
                var dataService = DependencyResolver.Resolve<IDataService>();
                if (dataService == null)
                {
                    return double.NaN;
                }

                var submissions = dataService.GetLoadedSubmissionsForTeacherPage(Notebook.ID, CurrentPage.ID, CurrentPage.DifferentiationLevel);
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
                var dataService = DependencyResolver.Resolve<IDataService>();
                if (dataService == null)
                {
                    return string.Empty;
                }

                var submissions = dataService.GetLoadedSubmissionsForTeacherPage(Notebook.ID, CurrentPage.ID, CurrentPage.DifferentiationLevel);

                var minSubmissionAnimationLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.AnimationLength).Concat(new[] { int.MaxValue }).Min();
                var maxSubmissionAnimationLength = !submissions.Any() ? -1 : submissions.Select(submission => submission.History.AnimationLength).Concat(new[] { 0 }).Max();
                var averageSubmissionAnimationLength = !submissions.Any() ? double.NaN : Math.Round(submissions.Select(submission => submission.History.AnimationLength).Average());

                return string.Format("{0} / {1} / {2}", minSubmissionAnimationLength, maxSubmissionAnimationLength, averageSubmissionAnimationLength);
            }
        }

        public double AnimationStandardDeviationZScore
        {
            get
            {
                var dataService = DependencyResolver.Resolve<IDataService>();
                if (dataService == null)
                {
                    return double.NaN;
                }

                var submissions = dataService.GetLoadedSubmissionsForTeacherPage(Notebook.ID, CurrentPage.ID, CurrentPage.DifferentiationLevel);
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

        #region Analysis Commands

        public Command GenerateHistoryActionsCommand { get; private set; }

        private void OnGenerateHistoryActionsCommandExecute()
        {
            var existingTags = CurrentPage.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag)).ToList();
            foreach (var tempArraySkipCountingTag in existingTags)
            {
                CurrentPage.RemoveTag(tempArraySkipCountingTag);
            }

            HistoryAnalysis.GenerateHistoryActions(CurrentPage);
        }

        public Command ShowAnalysisClustersCommand { get; private set; }

        private void OnShowAnalysisClustersCommandExecute()
        {
            foreach (var cluster in InkCodedActions.InkClusters.Where(c => c.ClusterType != InkCluster.ClusterTypes.Ignore))
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

        public Command StrokeTestingCommand { get; private set; }

        private void OnStrokeTestingCommandExecute()
        {
            Console.WriteLine("NEW STROKE TEST");
            var strokes = CurrentPage.InkStrokes.ToList();
            var strokeIndexesInEnclosure = new List<int>();
            for (var i = 0; i < strokes.Count; i++)
            {
                var stroke = strokes[i];
                // var strokeStartPoint = stroke.StylusPoints.First();
                bool isEnclosed;
                if (strokeIndexesInEnclosure.Contains(i))
                {
                    isEnclosed = true;
                }
                else
                {
                    if (IsDebuggingFlag)
                    {
                        isEnclosed = stroke.IsEnclosedShape(CurrentPage);
                    }
                    else
                    {
                        isEnclosed = stroke.IsEnclosedShape();
                    }

                    if (!isEnclosed)
                    {
                        for (var j = i + 1; j < strokes.Count; j++)
                        {
                            var otherStroke = strokes[j];
                            if (closeMatch(stroke, otherStroke))
                            {
                                Console.WriteLine("close match");
                                var strokeCollection = new StrokeCollection();
                                strokeCollection.Add(stroke);
                                strokeCollection.Add(otherStroke);
                                if (IsDebuggingFlag)
                                {
                                    isEnclosed = strokeCollection.IsEnclosedShape(CurrentPage);
                                }
                                else
                                {
                                    isEnclosed = strokeCollection.IsEnclosedShape();
                                }

                                if (isEnclosed)
                                {
                                    strokeIndexesInEnclosure.Add(j);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Console.WriteLine("Strokes start at ({0}, {1}), IsEnclosedShape: {2}", strokeStartPoint.X, strokeStartPoint.Y, isEnclosed);
                /*
                Console.WriteLine("Horizontal Line Test");
                if (stroke.IsHorizontalLine())
                {
                    stroke.DrawingAttributes.Color = Colors.Purple;
                }


                Console.WriteLine("Vertical Line Test");
                if (stroke.IsVerticalLine())
                {
                     stroke.DrawingAttributes.Color = Colors.Orange;
                }

                Console.WriteLine("Dot Test");
                if (stroke.IsDot())
                {
                    stroke.DrawingAttributes.Color = Colors.Blue;
                }
                */

                #region Debugging

                if (IsDebuggingFlag)
                {
                    var oldWidth = stroke.DrawingAttributes.Width;
                    var oldHeight = stroke.DrawingAttributes.Height;
                    var oldColor = stroke.DrawingAttributes.Color;
                    // stroke.DrawingAttributes.Width = 8;
                    // stroke.DrawingAttributes.Height = 8;
                    // PageHistory.UISleep(1000);
                    if (isEnclosed)
                    {
                        stroke.DrawingAttributes.Color = Colors.Green;
                    }
                    else
                    {
                        stroke.DrawingAttributes.Color = Colors.Crimson;
                    }
                    // PageHistory.UISleep(3000);
                    // stroke.DrawingAttributes.Width = oldWidth;
                    // DrawingAttributes.Height = oldHeight;
                    // stroke.DrawingAttributes.Color = oldColor;
                }

                #endregion // Debugging
            }
        }

        private Boolean closeMatch(Stroke stroke1, Stroke stroke2)
        {
            const double minDistanceSquared = 5000;
            return ((DistanceSquaredBetweenPoints(stroke1.StylusPoints.First().ToPoint(), stroke2.StylusPoints.First().ToPoint()) < minDistanceSquared) &&
                    (DistanceSquaredBetweenPoints(stroke1.StylusPoints.Last().ToPoint(), stroke2.StylusPoints.Last().ToPoint()) < minDistanceSquared)) ||
                   ((DistanceSquaredBetweenPoints(stroke1.StylusPoints.First().ToPoint(), stroke2.StylusPoints.Last().ToPoint()) < minDistanceSquared) &&
                    (DistanceSquaredBetweenPoints(stroke1.StylusPoints.Last().ToPoint(), stroke2.StylusPoints.First().ToPoint()) < minDistanceSquared));
        }

        private static double DistanceSquaredBetweenPoints(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var distanceSquared = (dx * dx) + (dy * dy); // Again, for performance purposes, multiplication is used here instead of Math.Pow(). 20x performance boost.

            return distanceSquared;
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used</summary>
        public Command v1Command { get; private set; }

        private void Onv1CommandExecute()
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
                const double RIGHT_OF_VISUAL_RIGHT_THRESHOLD = 80.0;
                const double LEFT_OF_VISUAL_RIGHT_THRESHOLD = 41.5;

                var arrayVisualRight = array.XPosition + array.Width - array.LabelLength;
                var arrayVisualTop = array.YPosition + array.LabelLength;
                var halfGridSquareSize = array.GridSquareSize * 0.5;
                var acceptedBoundary = new Rect(arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD,
                                                arrayVisualTop - halfGridSquareSize,
                                                LEFT_OF_VISUAL_RIGHT_THRESHOLD + RIGHT_OF_VISUAL_RIGHT_THRESHOLD,
                                                array.GridSquareSize * (array.Rows + 1));

                var strokes = CurrentPage.InkStrokes;
                var strokesInsideBoundary = new List<Stroke>();

                foreach (var stroke in strokes)
                {
                    var strokeBounds = stroke.GetBounds();

                    // Rule 3: Rejected for being outside the accepted skip counting bounds
                    var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                    if (intersect.IsEmpty)
                    {
                        continue;
                    }

                    var intersectPercentage = intersect.Area() / strokeBounds.Area();
                    if (intersectPercentage <= 0.50)
                    {
                        continue;
                    }

                    if (intersectPercentage <= 0.90)
                    {
                        var weightedCenterX = stroke.WeightedCenter().X;
                        if (weightedCenterX < arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD ||
                            weightedCenterX > arrayVisualRight + RIGHT_OF_VISUAL_RIGHT_THRESHOLD)
                        {
                            continue;
                        }
                    }

                    strokesInsideBoundary.Add(stroke);
                }

                if (!strokesInsideBoundary.Any())
                {
                    continue;
                }

                var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokesInsideBoundary));
                var guess = InkInterpreter.InterpretationClosestToANumber(interpretations); //or use [0]
                //var guess = interpretations[0];

                var tag = new TempArraySkipCountingTag(CurrentPage, Origin.StudentPageGenerated)
                          {
                              CodedID = array.CodedID,
                              RowInterpretations = guess
                          };

                CurrentPage.AddTag(tag);

                if (IsDebuggingFlag)
                {
                    CurrentPage.ClearBoundaries();
                    var tempBoundary = new TemporaryBoundary(CurrentPage, acceptedBoundary.X, acceptedBoundary.Y, acceptedBoundary.Height, acceptedBoundary.Width)
                                       {
                                           RegionText = "Boundary Interpretation: " + guess
                                       };
                    CurrentPage.PageObjects.Add(tempBoundary);
                    PageHistory.UISleep(5000);
                    var heightWidths = new Dictionary<Stroke, Point>();
                    foreach (var stroke in strokesInsideBoundary)
                    {
                        var width = stroke.DrawingAttributes.Width;
                        var height = stroke.DrawingAttributes.Height;
                        heightWidths.Add(stroke, new Point(width, height));

                        stroke.DrawingAttributes.Width = 8;
                        stroke.DrawingAttributes.Height = 8;
                    }
                    PageHistory.UISleep(5000);
                    foreach (var stroke in strokesInsideBoundary)
                    {
                        var width = heightWidths[stroke].X;
                        var height = heightWidths[stroke].Y;
                        stroke.DrawingAttributes.Width = width;
                        stroke.DrawingAttributes.Height = height;
                    }
                    heightWidths.Clear();
                    PageHistory.UISleep(2000);
                }
            }
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used</summary>
        public Command v2Command { get; private set; }

        private void Onv2CommandExecute()
        {
            var existingTags = CurrentPage.Tags.OfType<TempArraySkipCountingTag>().ToList();
            foreach (var tempArraySkipCountingTag in existingTags)
            {
                CurrentPage.RemoveTag(tempArraySkipCountingTag);
            }

            var arraysOnPage = CurrentPage.PageObjects.OfType<CLPArray>().ToList();
            var strokes = CurrentPage.InkStrokes.ToList();
            var historyIndex = 0;
            var lastHistoryItem = CurrentPage.History.CompleteOrderedHistoryItems.LastOrDefault();
            if (lastHistoryItem != null)
            {
                historyIndex = lastHistoryItem.HistoryIndex;
            }

            //Iterates over arrays on page
            foreach (var array in arraysOnPage)
            {
                var strokeGroupPerRow = ArrayCodedActions.GroupPossibleSkipCountStrokes(CurrentPage, array, strokes, historyIndex);
                var skipStrokes = strokeGroupPerRow.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                if (!skipStrokes.Any())
                {
                    continue;
                }

                var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(skipStrokes));
                var guess = InkInterpreter.InterpretationClosestToANumber(interpretations); //or use [0]
                //var guess = interpretations[0];

                var tag = new TempArraySkipCountingTag(CurrentPage, Origin.StudentPageGenerated)
                          {
                              CodedID = array.CodedID,
                              RowInterpretations = guess
                          };

                Console.WriteLine(tag.FormattedValue);

                CurrentPage.AddTag(tag);

                if (IsDebuggingFlag)
                {
                    var skipStrokeBounds = new StrokeCollection(skipStrokes).GetBounds();

                    CurrentPage.ClearBoundaries();
                    var tempBoundary = new TemporaryBoundary(CurrentPage, skipStrokeBounds.X, skipStrokeBounds.Y, skipStrokeBounds.Height, skipStrokeBounds.Width)
                                       {
                                           RegionText = "Potential SC Interpretation: " + guess
                                       };
                    CurrentPage.PageObjects.Add(tempBoundary);
                    PageHistory.UISleep(5000);
                    var heightWidths = new Dictionary<Stroke, Point>();
                    foreach (var stroke in skipStrokes)
                    {
                        var width = stroke.DrawingAttributes.Width;
                        var height = stroke.DrawingAttributes.Height;
                        heightWidths.Add(stroke, new Point(width, height));

                        stroke.DrawingAttributes.Width = 8;
                        stroke.DrawingAttributes.Height = 8;
                    }
                    PageHistory.UISleep(5000);
                    foreach (var stroke in skipStrokes)
                    {
                        var width = heightWidths[stroke].X;
                        var height = heightWidths[stroke].Y;
                        stroke.DrawingAttributes.Width = width;
                        stroke.DrawingAttributes.Height = height;
                    }
                    heightWidths.Clear();
                    PageHistory.UISleep(2000);
                }
            }
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used</summary>
        public Command AnalyzeSkipCountingCommand { get; private set; }

        private void OnAnalyzeSkipCountingCommandExecute()
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
                var formattedSkips = ArrayCodedActions.StaticSkipCountAnalysis(CurrentPage, array, IsDebuggingFlag);
                if (string.IsNullOrEmpty(formattedSkips))
                {
                    continue;
                }

                var unformattedSkips = formattedSkips.TrimAll().Split(new[] { "\"\"" }, StringSplitOptions.None).Select(s => s.Replace("\"", string.Empty)).ToList();
                var heuristicsResults = ArrayCodedActions.Heuristics(unformattedSkips, array.Rows, array.Columns);

                var tag = new TempArraySkipCountingTag(CurrentPage, Origin.StudentPageGenerated)
                          {
                              CodedID = array.CodedID,
                              RowInterpretations = formattedSkips,
                              HeuristicsResults = heuristicsResults
                          };

                Console.WriteLine(tag.FormattedValue);

                CurrentPage.AddTag(tag);
            }
        }

        /// <summary>Analyzes ink strokes near array objects to determine if skip counting was used, with forced Debug Mode</summary>
        public Command AnalyzeSkipCountingWithDebugCommand { get; private set; }

        private void OnAnalyzeSkipCountingWithDebugCommandExecute()
        {
            IsDebuggingFlag = true;
            OnAnalyzeSkipCountingCommandExecute();
        }

        public Command TSVCommand { get; private set; }

        private void OnTSVCommandExecute()
        {
            var pageNumber = CurrentPage.PageNumber;
            var studentName = CurrentPage.Owner.FullName;
            var historyActions = CurrentPage.History.HistoryActions;
            var firstAction = historyActions.FirstOrDefault();
            var compActions = new ObservableCollection<IHistoryAction>();
            var index = 0;
            var strCompActions = "";
            //Store actions logged from Pass 3
            for (var i = 0; i < historyActions.Count; i++)
            {
                if (historyActions[i].CodedValue == "PASS [3]")
                {
                    index = i + 1;
                    break;
                }
            }
            for (var j = index; j < historyActions.Count; j++)
            {
                compActions.Add(historyActions[j]);
            }

            foreach (var action in compActions)
            {
                if (action.CodedObject == Codings.OBJECT_INK &&
                    action.CodedObjectAction == Codings.ACTION_INK_IGNORE)
                {
                    continue;
                }

                if (action.CodedObjectAction == Codings.ACTION_OBJECT_MOVE)
                {
                    continue;
                }

                if (!action.IsObjectActionVisible)
                {
                    strCompActions += action.CodedObject + "; ";
                }
                else
                {
                    strCompActions += action.CodedObject + " " + action.CodedObjectAction + "; ";
                }
            }

            //Edit Name and Page Number for comparison from Machine Codes to Human Codes
            string stringPageNumber;
            string compareName;

            if (pageNumber < 10)
            {
                stringPageNumber = "A0" + pageNumber;
            }
            else
            {
                stringPageNumber = "A" + pageNumber;
            }
            if (studentName.Contains("Allison"))
            {
                compareName = studentName.Substring(0, 9) + ".";
            }
            else if (studentName.Contains("Ella"))
            {
                compareName = studentName.Substring(0, 6) + ".";
            }
            else
            {
                compareName = studentName.Split(' ')[0];
            }

            //Load TSV file from desktop
            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var tsvFile = Path.Combine(desktopDirectory, "Test.tsv");
            var sr = new StreamReader(tsvFile);
            var full_doc = new List<List<string>>();
            string readLine;
            while ((readLine = sr.ReadLine()) != null)
            {
                var row = readLine.Split('\t').ToList();
                full_doc.Add(row);
            }

            // Grab matching TSV data and format for comparison
            var changed = "";
            var analysisCodes = "";
            foreach (var row in full_doc)
            {
                if (row[2] == compareName &&
                    row[4] == stringPageNumber)
                {
                    var cellContents = row[12];
                    analysisCodes = row[13];
                    var contents = cellContents.Split(';');
                    foreach (var con in contents)
                    {
                        var reg_con = Regex.Replace(con, "\\[.*\\]", string.Empty).Trim();
                        var reg_con2 = Regex.Replace(reg_con, "[^a-zA-Z0-9_\\s]+", "").Trim();

                        if (reg_con2.Length >= 3)
                        {
                            if (reg_con2.Substring(0, 3) == "EQN")
                            {
                                changed += Codings.OBJECT_ARITH + reg_con2.Substring(3) + "; ";
                            }
                        }
                        if (reg_con2.Length - 5 > 0)
                        {
                            if (reg_con2.Substring((reg_con2.Length) - 4) == "leqn")
                            {
                                changed += reg_con2.Substring(0, reg_con2.Length - 4) + "eqn; ";
                            }
                            else
                            {
                                changed += reg_con2 + "; ";
                            }
                        }

                        else
                        {
                            changed += reg_con2 + "; ";
                        }
                    }
                }
            }
            //Format changes to allow for comparison
            strCompActions = strCompActions.Trim(' ');
            changed = changed.Trim(' ');
            strCompActions = strCompActions.Trim(';');
            changed = changed.Trim(';');
            strCompActions = " " + strCompActions;
            changed = " " + changed;
            // Print for Comparison
            var output = string.Format("Student Testing: {0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                                       compareName + ' ',
                                       stringPageNumber,
                                       Environment.NewLine,
                                       "Machine Codes: ",
                                       strCompActions,
                                       Environment.NewLine,
                                       "People Codes: ",
                                       changed,
                                       "Human Analysis Codes: ",
                                       analysisCodes);
            Console.WriteLine(output);

            var consecutive_count = 0;
            var total_matches = 0;
            var machineElems = strCompActions.Split(';');
            var humanElems = changed.Split(';');
            var elems = Math.Min(machineElems.Length, humanElems.Length);
            var total_codes = Math.Max(machineElems.Length, humanElems.Length);
            var copyMachineElems = machineElems.ToList();
            for (int i = 0; i < elems; i++)
            {
                //Console.WriteLine(humanElems[i]);
                if (machineElems[i] == humanElems[i])
                {
                    consecutive_count++;
                }
                else
                {
                    break;
                }
            }

            for (int j = 0; j < humanElems.Length; j++)
            {
                //Console.WriteLine(humanElems[j]);
                //Console.WriteLine(copyMachineElems.Contains(humanElems[j]));
                if (copyMachineElems.Contains(humanElems[j]))
                {
                    copyMachineElems.RemoveAt(copyMachineElems.FindIndex(humanElems[j]));
                    total_matches++;
                }
            }

            Console.WriteLine("Total Matches: " + total_matches + " out of " + total_codes);
            Console.WriteLine("Consecutive Matches: " + consecutive_count + " out of " + total_codes);
            Console.WriteLine();
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
                var formattedSkips = ArrayCodedActions.StaticBottomSkipCountAnalysis(CurrentPage, array, IsDebuggingFlag);
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

            //Console.WriteLine("Top: {0}", bounds.Y);
            //Console.WriteLine("Bottom: {0}", bounds.Y + bounds.Height);
            //Console.WriteLine("Left: {0}", bounds.X);
            //Console.WriteLine("Right: {0}", bounds.X + bounds.Width);

            //foreach (var array in CurrentPage.PageObjects.OfType<CLPArray>().ToList())
            //{
            //    var arrayBottom = array.YPosition + array.Height - array.LabelLength;
            //    Console.WriteLine("Top Delta: {0}", arrayBottom - bounds.Y);
            //    Console.WriteLine("Bottom Delta: {0}", bounds.Y + bounds.Height - arrayBottom);
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
            //var correctnessTag = page.Tags.FirstOrDefault(x => x is CorrectnessTag) as CorrectnessTag;
            //if (correctnessTag != null &&
            //    correctnessTag.IsCorrectnessManuallySet)
            //{
            //    return;
            //}

            //var correctnessTags =
            //    page.Tags.OfType<DivisionTemplateRepresentationCorrectnessTag>()
            //        .Select(divisionTemplateCorrectnessTag => new CorrectnessTag(page, Origin.StudentPageGenerated, divisionTemplateCorrectnessTag.Correctness, true))
            //        .ToList();
            //correctnessTags.AddRange(
            //                         page.Tags.OfType<ArrayCorrectnessSummaryTag>()
            //                             .Select(arrayCorrectnessTag => new CorrectnessTag(page, Origin.StudentPageGenerated, arrayCorrectnessTag.Correctness, true)));

            //if (!correctnessTags.Any())
            //{
            //    return;
            //}

            //var correctnessSum = Correctness.Unknown;
            //foreach (var tag in correctnessTags)
            //{
            //    if (correctnessSum == tag.Correctness)
            //    {
            //        continue;
            //    }

            //    if (correctnessSum == Correctness.Unknown)
            //    {
            //        correctnessSum = tag.Correctness;
            //        continue;
            //    }

            //    if (correctnessSum == Correctness.Correct &&
            //        (tag.Correctness == Correctness.Incorrect || tag.Correctness == Correctness.PartiallyCorrect))
            //    {
            //        correctnessSum = Correctness.PartiallyCorrect;
            //        break;
            //    }

            //    if (tag.Correctness == Correctness.Correct &&
            //        (correctnessSum == Correctness.Incorrect || correctnessSum == Correctness.PartiallyCorrect))
            //    {
            //        correctnessSum = Correctness.PartiallyCorrect;
            //        break;
            //    }
            //}

            //page.AddTag(new CorrectnessTag(page, Origin.StudentPageGenerated, correctnessSum, true));
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

        public Command PrintAllHistoryItemsCommand { get; private set; }

        private void OnPrintAllHistoryItemsCommandExecute()
        {
            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "HistoryLogs");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, PageNameComposite.ParsePage(CurrentPage).ToFileName() + ".txt");
            if (File.Exists(filePath))
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
            // Console.WriteLine(line);
            // }
        }

        #endregion // Obsolete Commands 

        #endregion // Analysis Commands

        #endregion //Commands
    }
}