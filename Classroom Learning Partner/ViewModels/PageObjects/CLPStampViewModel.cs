namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using Catel.Data;
    using Classroom_Learning_Partner.Model;
    using System;
    using System.Windows.Controls.Primitives;
    using System.Windows;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStampViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampViewModel"/> class.
        /// </summary>
        public CLPStampViewModel(CLPStamp stamp)
            : base()
        {
            PageObject = stamp;

            if (stamp.InternalPageObject == null)
            {
                InternalType = "Blank";
            }
            else
            {
                InternalType = stamp.InternalPageObject.PageObjectType;
            }

            CopyStampCommand = new Command(OnCopyStampCommandExecute);
            PlaceStampCommand = new Command(OnPlaceStampCommandExecute);
            DragStampCommand = new Command<DragDeltaEventArgs>(OnDragStampCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StampVM"; } }

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

        /// <summary>
        /// Gets the CopyStampCommand command.
        /// </summary>
        public Command CopyStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CopyStampCommand command is executed.
        /// </summary>
        private void OnCopyStampCommandExecute()
        {
            CLPStamp leftBehindStamp = PageObject.Duplicate() as CLPStamp;
            leftBehindStamp.UniqueID = PageObject.UniqueID;
            CLPServiceAgent.Instance.AddPageObjectToPage(PageObject.PageID, leftBehindStamp);
        }

                /// <summary>
        /// Gets the PlaceStampCommand command.
        /// </summary>
        public Command PlaceStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the PlaceStampCommand command is executed.
        /// </summary>
        private void OnPlaceStampCommandExecute()
        {
            CLPStrokePathContainer tempContainer = new CLPStrokePathContainer(InternalPageObject);
            tempContainer.Height = Height - 50;
            tempContainer.Width = Width;
            tempContainer.Position = new Point(Position.X, Position.Y + 50);
            CLPServiceAgent.Instance.AddPageObjectToPage(PageObject.PageID, tempContainer);
            CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Gets the DragStampCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DragStampCommand command is executed.
        /// </summary>
        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            double x = PageObject.Position.X + e.HorizontalChange;
            double y = PageObject.Position.Y + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < -49)
            {
                y = -49;
            }
            if (x > 1056 - PageObject.Width)
            {
                x = 1056 - PageObject.Width;
            }
            if (y > 816 - PageObject.Height)
            {
                y = 816 - PageObject.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(PageObject, pt);
        }
    }
}
