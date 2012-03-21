using System.Collections.ObjectModel;
namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using Classroom_Learning_Partner.Model;
    using Catel.Data;
    using System.Windows.Media;
    using System.Windows.Ink;
    using System.Windows.Input;
    using System.Windows;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStrokePathContainerViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStrokePathContainerViewModel"/> class.
        /// </summary>
        public CLPStrokePathContainerViewModel(CLPStrokePathContainer container)
        {
            PageObject = container;

            if (container.InternalPageObject == null)
            {
                InternalType = "Blank";
            }
            else
            {
                InternalType = container.InternalPageObject.PageObjectType;
            }

            ScribblesToStrokePaths();
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StrokePathContainerVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ICLPPageObject InternalPageObject
        {
            get { return GetValue<ICLPPageObject>(InternalPageObjectProperty); }
            set { SetValue(InternalPageObjectProperty, value); }
        }

        /// <summary>
        /// Register the InternalPageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InternalPageObjectProperty = RegisterProperty("InternalPageObject", typeof(ICLPPageObject));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string InternalType
        {
            get { return GetValue<string>(InternalTypeProperty); }
            set { SetValue(InternalTypeProperty, value); }
        }

        /// <summary>
        /// Register the InternalType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InternalTypeProperty = RegisterProperty("InternalType", typeof(string));

        #region Methods


        //put in VM
        private ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        {
            get { return _strokePathViewModels; }
        }

        public void ScribblesToStrokePaths()
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

        #endregion //Methods

    }
}
