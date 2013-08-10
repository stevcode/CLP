using System;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GridDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel()
            : base()
        {
            DisplayedPages = new ObservableCollection<ICLPPage>();
            DisplayedPages.CollectionChanged += DisplayedPages_CollectionChanged;

            RemovePageFromGridDisplayCommand = new Command<ICLPPage>(OnRemovePageFromGridDisplayCommandExecute);
        }

        public override string Title { get { return "GridDisplayVM"; } }

        #region Bindings

        #region Interface

        public string DisplayName
        {
            get { return "GridDisplay"; }
        }

        /// <summary>
        /// Unique ID of Display.
        /// </summary>
        public string DisplayID
        {
            get { return GetValue<string>(DisplayIDProperty); }
            set { SetValue(DisplayIDProperty, value); }
        }

        public static readonly PropertyData DisplayIDProperty = RegisterProperty("DisplayID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// If Display is currently being projected.
        /// </summary>
        public bool IsOnProjector
        {
            get { return GetValue<bool>(IsOnProjectorProperty); }
            set { SetValue(IsOnProjectorProperty, value); }
        }

        public static readonly PropertyData IsOnProjectorProperty = RegisterProperty("IsOnProjector", typeof(bool), false);

        #endregion //Interface

        /// <summary>
        /// Number of Rows in the UniformGrid
        /// </summary>
        public int UGridRows
        {
            get { return GetValue<int>(UGridRowsProperty); }
            set { SetValue(UGridRowsProperty, value); }
        }

        public static readonly PropertyData UGridRowsProperty = RegisterProperty("UGridRows", typeof(int), 1);

        /// <summary>
        /// Currently Displayed pages in the GridDisplay.
        /// </summary>
        public ObservableCollection<ICLPPage> DisplayedPages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(DisplayedPagesProperty); }
            set { SetValue(DisplayedPagesProperty, value); }
        }

        public static readonly PropertyData DisplayedPagesProperty = RegisterProperty("DisplayedPages", typeof(ObservableCollection<ICLPPage>));

        #endregion //Bindings

        #region Methods

        //From Interface IDisplayViewModel
        public void AddPageToDisplay(ICLPPage page)
        {
            DisplayedPages.Add(page);
        }

        void DisplayedPages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DisplayedPages.Count < 3)
            {
                UGridRows = 1;
            }
            else
            {
                UGridRows = 0;
            }
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand command.
        /// </summary>
        public Command<ICLPPage> RemovePageFromGridDisplayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RemovePageFromGridDisplayCommand command is executed.
        /// </summary>
        public void OnRemovePageFromGridDisplayCommandExecute(ICLPPage page)
        {
            DisplayedPages.Remove(page);
        }

        #endregion //Commands

    }
}