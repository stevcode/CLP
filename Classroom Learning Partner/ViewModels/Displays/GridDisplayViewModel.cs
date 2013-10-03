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
            var displayListPanel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            if(gridDisplayViewModel == null || displayListPanel == null)
            {
                return;
            }

            foreach(var displayViewModel in (from display in displayListPanel.Displays
                                             where gridDisplayViewModel.GridDisplay.UniqueID != display.UniqueID
                                             select CLPServiceAgent.Instance.GetViewModelsFromModel(display as ModelBase).FirstOrDefault()).OfType<IDisplayViewModel>())
            {
                displayViewModel.IsOnProjector = false;
            }

            displayListPanel.MirrorDisplayIsOnProjector = false;

            if(App.Network.ProjectorProxy == null || !gridDisplayViewModel.IsOnProjector)
            {
                gridDisplayViewModel.IsOnProjector = false;
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

        #endregion //Bindings

        #region Methods

        //From Interface IDisplayViewModel
        public void AddPageToDisplay(ICLPPage page)
        {
            GridDisplay.AddPageToDisplay(page);
            if(!IsOnProjector)
            {
                return;
            }

            var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;

            if(App.Network.ProjectorProxy != null)
            {
                try
                {
                    App.Network.ProjectorProxy.AddPageToDisplay(pageID);
                }
                catch(Exception)
                {

                }
            }
        }

        void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UGridRows = Pages.Count < 3 ? 1 : 0;
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
            if(!IsOnProjector)
            {
                return;
            }

            var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;

            if(App.Network.ProjectorProxy != null)
            {
                try
                {
                    App.Network.ProjectorProxy.RemovePageFromDisplay(pageID);
                }
                catch(Exception)
                {

                }
            }
        }

        #endregion //Commands

    }
}