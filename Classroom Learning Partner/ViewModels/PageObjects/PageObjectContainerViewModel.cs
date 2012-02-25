using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Catel.Data;
using Classroom_Learning_Partner.Model;
using System;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class PageObjectContainerViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the PageObjectContainerViewModel class.
        /// </summary>
        public PageObjectContainerViewModel(ICLPPageObject pageObject) : base()
        {
            PageObject = pageObject;
            Console.WriteLine("POContainerViewModel created");
        }

        public override string Title { get { return "PageObjectContainerVM"; } }


        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject=false)]
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            private set { SetValue(PageObjectProperty, value); }
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

        #endregion //Model

    }
}