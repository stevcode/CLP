namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using System.Windows.Ink;
    using Catel.Data;
    using Classroom_Learning_Partner.Resources;
    using System.Collections.ObjectModel;
    using Classroom_Learning_Partner.Model;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPInkShapeRegionViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPHandwritingRegionViewModel"/> class.
        /// </summary>
        public CLPInkShapeRegionViewModel(CLPInkShapeRegion inkRegion)
            : base()
        {
            PageObject = inkRegion;
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "InkShapeRegionVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string InkShapesString
        {
            get { return GetValue<string>(InkShapesStringProperty); }
            set { SetValue(InkShapesStringProperty, value); }
        }

        /// <summary>
        /// Register the InkShapesString property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapesStringProperty = RegisterProperty("InkShapesString", typeof(string));

        /// <summary>
        /// Stored strokecollections that constitute shapes
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPInkShape> InkShapes
        {
            get { return GetValue<ObservableCollection<CLPInkShape>>(InkShapesProperty); }
            set { SetValue(InkShapesProperty, value); }
        }

        /// <summary>
        /// Register the ShapeStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapesProperty = RegisterProperty("InkShapes", typeof(ObservableCollection<CLPInkShape>));


        #endregion //Model

    }
}
