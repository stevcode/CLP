using Catel.MVVM;
using Catel.Data;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{

    public class CLPBlankStampViewModel : CLPStampBaseViewModel
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPBlankStampViewModel class.
        /// </summary>
        public CLPBlankStampViewModel(CLPBlankStamp stamp)
            : base(stamp)
        {
            PageObjectStrokes = CLPPage.StringsToStrokes(stamp.PageObjectStrokes);

            if (!IsAnchored)
            {
                ScribblesToStrokePaths();
            }
        }

        public override string Title { get { return "BlankStampVM"; } }

        #endregion //Constructors

        #region Bindings

        private ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        {
            get { return _strokePathViewModels; }
        }

        #endregion //Bindings

        #region Methods

        public void ScribblesToStrokePaths()
        {
            if (!IsAnchored)
            {
                foreach (Stroke stroke in PageObjectStrokes)
                {
                    StylusPoint firstPoint = stroke.StylusPoints[0];

                    StreamGeometry geometry = new StreamGeometry();
                    using (StreamGeometryContext geometryContext = geometry.Open())
                    {
                        geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
                        foreach (StylusPoint point in stroke.StylusPoints)
                        {
                            geometryContext.LineTo(new Point(point.X, point.Y), true, true);
                        }
                    }
                    geometry.Freeze();

                    StrokePathViewModel strokePathViewModel = new StrokePathViewModel(geometry, (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString()), stroke.DrawingAttributes.Width);
                    StrokePathViewModels.Add(strokePathViewModel);
                }
            }
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            if (IsAnchored)
            {
                this.ProcessStrokes(addedStrokes, removedStrokes);
            }
        }

        #endregion //Methods
    }
}