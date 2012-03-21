namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using Catel.Data;
    using Classroom_Learning_Partner.Model;
    using System;
    using System.Windows.Controls.Primitives;
    using System.Windows;
    using System.Threading;
    using System.Windows.Threading;

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
        public CLPStrokePathContainer StrokePathContainer
        {
            get { return GetValue<CLPStrokePathContainer>(StrokePathContainerProperty); }
            set { SetValue(StrokePathContainerProperty, value); }
        }

        /// <summary>
        /// Register the StrokePathContainer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StrokePathContainerProperty = RegisterProperty("StrokePathContainer", typeof(CLPStrokePathContainer));

        /// <summary>
        /// Gets the CopyStampCommand command.
        /// </summary>
        public Command CopyStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CopyStampCommand command is executed.
        /// </summary>
        private void OnCopyStampCommandExecute()
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                (DispatcherOperationCallback)delegate(object arg)
                {


                    CopyStamp();

                    return null;
                }, null);
            
            StrokePathContainer.PageObjectStrokes = PageObject.PageObjectStrokes;
            StrokePathContainer.IsStrokePathsVisible = true;
        }

        private void CopyStamp()
        {
            try
            {
                CLPStamp leftBehindStamp = PageObject.Duplicate() as CLPStamp;
                leftBehindStamp.UniqueID = PageObject.UniqueID;

                CLPPage page = CLPServiceAgent.Instance.GetPageFromID(PageObject.PageID);

                if (page != null)
                {
                    leftBehindStamp.PageID = page.UniqueID;

                    //foreach (var pageObject in page.PageObjects)
                    //{
                    //    if (pageObject.UniqueID == leftBehindStamp.UniqueID)
                    //    {

                    //    }
                    //}

                    int index = page.PageObjects.IndexOf(PageObject);
                    Console.WriteLine(index.ToString());

                    page.PageObjects.Add(leftBehindStamp);

                    if (!page.PageHistory.IgnoreHistory)
                    {
                        CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.AddPageObject, leftBehindStamp.UniqueID, null, null);
                        page.PageHistory.HistoryItems.Add(item);
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
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
            StrokePathContainer.Position = new Point(PageObject.Position.X, PageObject.Position.Y + 50);
            StrokePathContainer.ParentID = PageObject.UniqueID;
            CLPServiceAgent.Instance.AddPageObjectToPage(PageObject.PageID, StrokePathContainer);
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
