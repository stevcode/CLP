using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class PlotPoint : ModelBase
    {
        public const double GRAPH_LENGTH = 700.0;
        public static double XMin = 0.0;
        public static double XMax = 0.0;
        public static double YMin = 0.0;
        public static double YMax = 0.0;

        public PlotPoint(QueryResult queryResult)
        {
            QueryResults.Add(queryResult);

            StudentActionDistance = queryResult.Page.StudentActionDistance;
            AnalysisDistance = queryResult.Page.AnalysisDistance;

            IsPartOfCurrentCluster = false;

            var xDiff = XMax - XMin;
            var xScale = GRAPH_LENGTH / xDiff;
            X = (XMax - StudentActionDistance) * xScale;

            var yDiff = YMax - YMin;
            var yScale = GRAPH_LENGTH / yDiff;
            Y = GRAPH_LENGTH - ((YMax - AnalysisDistance) * yScale);
        }

        public List<QueryResult> QueryResults { get; set; } = new List<QueryResult>();
        public int NumberOfPages => QueryResults.Count;

        public double StudentActionDistance { get; set; }

        //public double X => (StudentActionDistance + BUFFER) * 5;
        public double X { get; set; }

        public double AnalysisDistance { get; set; }

        //public double Y => 300 - ((AnalysisDistance + BUFFER) * 5);
        public double Y { get; set; }

    public bool IsPartOfCurrentCluster
        {
            get => GetValue<bool>(IsPartOfCurrentClusterProperty);
            set => SetValue(IsPartOfCurrentClusterProperty, value);
        }

        public static readonly PropertyData IsPartOfCurrentClusterProperty = RegisterProperty(nameof(IsPartOfCurrentCluster), typeof(bool), false);
    }

    public class GraphViewModel : ViewModelBase
    {
        #region Constructor

        public GraphViewModel(List<QueryResult> queryResults, double xMin, double xMax, double yMin, double yMax)
        {
            PlotPoint.XMin = xMin;
            PlotPoint.XMax = xMax;
            PlotPoint.YMin = yMin;
            PlotPoint.YMax = yMax;

            var clusterNames = new List<string>();
            foreach (var queryResult in queryResults)
            {
                clusterNames.Add(queryResult.ClusterName);
                var queryablePage = queryResult.Page;
                var existingPoint = PlotPoints.FirstOrDefault(p => p.StudentActionDistance == queryablePage.StudentActionDistance && 
                                                                   p.AnalysisDistance == queryablePage.AnalysisDistance);

                if (existingPoint == null)
                {
                    PlotPoints.Add(new PlotPoint(queryResult));
                }
                else
                {
                    existingPoint.QueryResults.Add(queryResult);
                }
            }

            ClusterNames = clusterNames.Distinct().ToObservableCollection();

            InitializeCommands();
        }

        #endregion // Constructor

        #region Bindings

        public ObservableCollection<PlotPoint> PlotPoints
        {
            get => GetValue<ObservableCollection<PlotPoint>>(PlotPointsProperty);
            set => SetValue(PlotPointsProperty, value);
        }

        public static readonly PropertyData PlotPointsProperty =
            RegisterProperty(nameof(PlotPoints), typeof(ObservableCollection<PlotPoint>), () => new ObservableCollection<PlotPoint>());

        public ObservableCollection<string> ClusterNames
        {
            get => GetValue<ObservableCollection<string>>(ClusterNamesProperty);
            set => SetValue(ClusterNamesProperty, value);
        }

        public static readonly PropertyData ClusterNamesProperty =
            RegisterProperty(nameof(ClusterNames), typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        public string FormattedValue
        {
            get => GetValue<string>(FormattedValueProperty);
            set => SetValue(FormattedValueProperty, value);
        }

        public static readonly PropertyData FormattedValueProperty = RegisterProperty(nameof(FormattedValue), typeof(string), string.Empty);

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            ToggleClusterCommand = new Command<string>(OnToggleClusterExecute);
            SetFormattedValueCommand = new Command<PlotPoint>(OnSetFormattedValueExecute);
        }

        public Command<string> ToggleClusterCommand { get; private set; }

        private string _currentClusterName = string.Empty;
        private void OnToggleClusterExecute(string clusterName)
        {
            foreach (var plotPoint in PlotPoints)
            {
                plotPoint.IsPartOfCurrentCluster = false;
            }

            if (_currentClusterName == clusterName)
            {
                _currentClusterName = string.Empty;
            }
            else
            {
                foreach (var plotPoint in PlotPoints)
                {
                    if (plotPoint.QueryResults.Any(qr => qr.ClusterName == clusterName))
                    {
                        plotPoint.IsPartOfCurrentCluster = true;
                    }
                }

                _currentClusterName = clusterName;
            }
        }

        public Command<PlotPoint> SetFormattedValueCommand { get; private set; }

        private void OnSetFormattedValueExecute(PlotPoint plotPoint)
        {
            FormattedValue = string.Join("\n\n", plotPoint.QueryResults.Select(qr => $"{qr.Page.StudentName}, Page {qr.PageNumber}\n\n{qr.Page.FormattedDistance}"));
        }

        #endregion // Commands
    }
}