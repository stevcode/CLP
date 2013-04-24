using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StrokePathViewModel : ACLPPageObjectBaseViewModel
    {
        #region Constructor

        public StrokePathViewModel(Geometry pathData, SolidColorBrush pathColor, double width)
        {
            _pathData = pathData;
            _pathColor = pathColor;
            _pathWidth = width;
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

        #endregion //Bindings
    }
}
