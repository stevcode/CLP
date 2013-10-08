using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(DisplayListPanelViewModel))]
    public class GridDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel(CLPGridDisplay gridDisplay)
        {
            GridDisplay = gridDisplay;
            Pages.CollectionChanged += Pages_CollectionChanged;
            UGridRows = Pages.Count < 3 ? 1 : 0;
            RemovePageFromGridDisplayCommand = new Command<ICLPPage>(OnRemovePageFromGridDisplayCommandExecute);

            var displayListViewModel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            if(displayListViewModel == null)
            {
                return;
            }
            IsOnProjector = displayListViewModel.ProjectedDisplayString == GridDisplay.UniqueID;
        }

        public override string Title { get { return "GridDisplayVM"; } }

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPGridDisplay GridDisplay
        {
            get { return GetValue<CLPGridDisplay>(GridDisplayProperty); }
            set { SetValue(GridDisplayProperty, value); }
        }

        public static readonly PropertyData GridDisplayProperty = RegisterProperty("GridDisplay", typeof(CLPGridDisplay));

        /// <summary>
        /// Index of the Display in the notebook.
        /// This property is automatically mapped to the corresponding property in GridDisplay.
        /// </summary>
        [ViewModelToModel("GridDisplay")]
        public int DisplayIndex
        {
            get { return GetValue<int>(DisplayIndexProperty); }
            set { SetValue(DisplayIndexProperty, value); }
        }

        public static readonly PropertyData DisplayIndexProperty = RegisterProperty("DisplayIndex", typeof(int));

        /// <summary>
        /// A property mapped to a property on the Model GridDisplay.
        /// </summary>
        [ViewModelToModel("GridDisplay")]
        public ObservableCollection<ICLPPage> Pages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<ICLPPage>));

        #endregion //Model

        #region Interface

        /// <summary>
        /// If Display is currently being projected.
        /// </summary>
        public bool IsOnProjector
        {
            get { return GetValue<bool>(IsOnProjectorProperty); }
            set { SetValue(IsOnProjectorProperty, value); }
        }

        public static readonly PropertyData IsOnProjectorProperty = RegisterProperty("IsOnProjector", typeof(bool), false, IsOnProjectorChanged);

        private static void IsOnProjectorChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var gridDisplayViewModel = sender as GridDisplayViewModel;
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            var displayListPanel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            if(gridDisplayViewModel == null || notebookWorkspaceViewModel == null || displayListPanel == null || App.CurrentUserMode != App.UserMode.Instructor)
            {
                return;
            }

            gridDisplayViewModel.IsOnProjector = args.NewValue is bool && (bool)args.NewValue;
            if(!gridDisplayViewModel.IsOnProjector || !gridDisplayViewModel.IsDisplayPreview)
            {
                return;
            }

            if(App.Network.ProjectorProxy == null)
            {
                displayListPanel.MirrorDisplayIsOnProjector = false;
                displayListPanel.ProjectedDisplayString = string.Empty;

                return;
            }

            var pageIDs = new List<string>();
            foreach(var page in gridDisplayViewModel.Pages)
            {
                var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
                pageIDs.Add(pageID);
            }

            try
            {
                displayListPanel.MirrorDisplayIsOnProjector = false;
                displayListPanel.ProjectedDisplayString = gridDisplayViewModel.GridDisplay.UniqueID;
                App.Network.ProjectorProxy.SwitchProjectorDisplay(gridDisplayViewModel.GridDisplay.UniqueID, pageIDs);
            }
            catch(Exception)
            {

            }
        }

        #endregion //Interface

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

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "ProjectedDisplayString" && viewModel is DisplayListPanelViewModel)
            {
                IsOnProjector = (viewModel as DisplayListPanelViewModel).ProjectedDisplayString == GridDisplay.UniqueID;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UGridRows = Pages.Count < 3 ? 1 : 0;

            if(!IsOnProjector || App.Network.ProjectorProxy == null || IsDisplayPreview)
            {
                return;
            }

            if(e.NewItems != null)
            {
                foreach(var pageAdded in e.NewItems)
                {
                    var page = pageAdded as ICLPPage;
                    if(page == null)
                    {
                        continue;
                    }
                    var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
                    try
                    {
                        App.Network.ProjectorProxy.AddPageToDisplay(pageID);
                    }
                    catch(Exception)
                    {

                    }
                }
            }

            if(e.OldItems != null)
            {
                foreach(var pageRemoved in e.OldItems)
                {
                    var page = pageRemoved as ICLPPage;
                    if(page == null)
                    {
                        continue;
                    }
                    var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
                    try
                    {
                        App.Network.ProjectorProxy.RemovePageFromDisplay(pageID);
                    }
                    catch(Exception)
                    {

                    }
                }
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
            GridDisplay.RemovePageFromDisplay(page);
        }

        #endregion //Commands

    }
}