using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class ACLPPageObjectBaseViewModel : ViewModelBase
    {
        protected ACLPPageObjectBaseViewModel() : base()
        {
            RemovePageObjectCommand = new Command(OnRemovePageObjectCommandExecute);
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
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        /// <summary>
        /// Register the XPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        /// <summary>
        /// Register the YPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double));
        
        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get { _pageObjectStrokes = CLPPage.BytesToStrokes(PageObject.PageObjectByteStrokes);
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

        #region Commands

        #region Default Adorners

        /// <summary>
        /// Gets the RemovePageObjectCommand command.
        /// </summary>
        public Command RemovePageObjectCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RemovePageObjectCommand command is executed.
        /// </summary>
        private void OnRemovePageObjectCommandExecute()
        {
            CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
        }

        #endregion //Default Adorners

        #endregion //Commands

    }
}