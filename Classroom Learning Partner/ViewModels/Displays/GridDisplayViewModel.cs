using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GridDisplayViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel(GridDisplay gridDisplay)
        {
            GridDisplay = gridDisplay;
            Pages.CollectionChanged += Pages_CollectionChanged;
            UGridRows = Pages.Count < 3 ? 1 : 0;
            RemovePageFromGridDisplayCommand = new Command<CLPPage>(OnRemovePageFromGridDisplayCommandExecute);
        }

        public override string Title
        {
            get { return "GridDisplayVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public GridDisplay GridDisplay
        {
            get { return GetValue<GridDisplay>(GridDisplayProperty); }
            private set { SetValue(GridDisplayProperty, value); }
        }

        public static readonly PropertyData GridDisplayProperty = RegisterProperty("GridDisplay", typeof(GridDisplay));

        /// <summary>
        /// Index of the Display in the notebook.
        /// This property is automatically mapped to the corresponding property in GridDisplay.
        /// </summary>
        [ViewModelToModel("GridDisplay")]
        public int DisplayNumber
        {
            get { return GetValue<int>(DisplayNumberProperty); }
            set { SetValue(DisplayNumberProperty, value); }
        }

        public static readonly PropertyData DisplayNumberProperty = RegisterProperty("DisplayNumber", typeof(int));

        /// <summary>
        /// A property mapped to a property on the Model GridDisplay.
        /// </summary>
        [ViewModelToModel("GridDisplay")]
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Bindings

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
        /// Toggle to ignore viewModels of Display Previews
        /// </summary>
        public bool IsDisplayPreview
        {
            get { return GetValue<bool>(IsDisplayPreviewProperty); }
            set { SetValue(IsDisplayPreviewProperty, value); }
        }

        public static readonly PropertyData IsDisplayPreviewProperty = RegisterProperty("IsDisplayPreview", typeof(bool), false);

        #endregion //Bindings

        #region Methods

        private void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UGridRows = Pages.Count < 3 ? 1 : 0;
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand command.
        /// </summary>
        public Command<CLPPage> RemovePageFromGridDisplayCommand { get; private set; }

        public void OnRemovePageFromGridDisplayCommandExecute(CLPPage page) { GridDisplay.RemovePageFromDisplay(page); }

        #endregion //Commands
    }
}