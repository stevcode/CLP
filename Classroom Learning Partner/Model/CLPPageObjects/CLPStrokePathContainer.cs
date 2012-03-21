using System;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPStrokePathContainer : CLPPageObjectBase
    {
        #region Constructors

        public CLPStrokePathContainer(ICLPPageObject internalPageObject)
            : base()
        {

        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPStrokePathContainer(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors


        #region Methods


        //put in VM
        //private ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        //public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        //{
        //    get { return _strokePathViewModels; }
        //}

        //public void ScribblesToStrokePaths()
        //{
        //    foreach (Stroke stroke in PageObjectStrokes)
        //    {
        //        StylusPoint firstPoint = stroke.StylusPoints[0];

        //        StreamGeometry geometry = new StreamGeometry();
        //        using (StreamGeometryContext geometryContext = geometry.Open())
        //        {
        //            geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
        //            foreach (StylusPoint point in stroke.StylusPoints)
        //            {
        //                geometryContext.LineTo(new Point(point.X, point.Y), true, true);
        //            }
        //        }
        //        geometry.Freeze();

        //        StrokePathViewModel strokePathViewModel = new StrokePathViewModel(geometry, (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString()), stroke.DrawingAttributes.Width);
        //        StrokePathViewModels.Add(strokePathViewModel);
        //    }
        //}

        #endregion //Methods

        public override string PageObjectType
        {
            get { throw new NotImplementedException(); }
        }

        public override ICLPPageObject Duplicate()
        {
            throw new NotImplementedException();
        }
    }
}
