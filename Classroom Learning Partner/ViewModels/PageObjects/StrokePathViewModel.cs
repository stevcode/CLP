using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StrokePathViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public StrokePathViewModel(Geometry pathData, SolidColorBrush pathColor, double width, bool isHighlighter)
        {
            _pathData = pathData;
            _pathColor = pathColor;
            _pathWidth = width;
            _isHighlighter = isHighlighter;
        }

        public override string Title { get { return "StrokePathVM"; } }

        #endregion //Constructor

        #region Bindings

        private readonly Geometry _pathData;
        public Geometry PathData
        {
            get { return _pathData; }
        }

        private readonly SolidColorBrush _pathColor;
        public SolidColorBrush PathColor
        {
            get { return _pathColor; }
        }

        private readonly double _pathWidth;
        public double PathWidth
        {
            get { return _pathWidth; }
        }

        private readonly bool _isHighlighter;
        public bool IsHighlighter
        {
            get { return _isHighlighter; }
        }

        #endregion //Bindings
    }
}
