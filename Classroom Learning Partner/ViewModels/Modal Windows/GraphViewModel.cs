using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public class PlotPoint
    {
        public PlotPoint(QueryablePage page)
        {
            StudentActionDistance = (page.StudentActionDistance + 10) * 5;
            AnalysisDistance = 300 - ((page.AnalysisDistance + 10) * 5);
            ProblemStructureDistance = page.ProblemStructureDistance;

            FormattedValue = page.FormattedValue;
        }

        public double StudentActionDistance { get; set; }
        public double AnalysisDistance { get; set; }
        public double ProblemStructureDistance { get; set; }
        public string FormattedValue { get; set; }
    }

    public class GraphViewModel : ViewModelBase
    {
        #region Constructor

        public GraphViewModel(List<QueryablePage> queryablePages)
        {
            foreach (var queryablePage in queryablePages)
            {
                PlotPoints.Add(new PlotPoint(queryablePage));
            }
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

        #endregion // Bindings
    }
}