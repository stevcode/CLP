using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class ACLPPageObjectBaseViewModel : ViewModelBase
    {
        protected ACLPPageObjectBaseViewModel() : base()
        {
        }

        public override string Title { get { return "APageObjectBaseVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject=false)]
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            protected set { SetValue(PageObjectProperty, value); }
        }

        /// <summary>
        /// Register the PageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(ICLPPageObject));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// Register the Height property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// Register the Width property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public Point Position
        {
            get { return GetValue<Point>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Register the Position property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(Point));
        
        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get { _pageObjectStrokes = CLPPage.StringsToStrokes(PageObject.PageObjectStrokes);
            return _pageObjectStrokes;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        /// <summary>
        /// Register the IsBackground property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool));

        #endregion //Model

    }
}