using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            DisplayedPages = new ObservableCollection<ICLPPage>();
            DisplayedPages.CollectionChanged += DisplayedPages_CollectionChanged;

            RemovePageFromGridDisplayCommand = new Command<ICLPPage>(OnRemovePageFromGridDisplayCommandExecute);
            SendDisplayToProjectorCommand = new Command(OnSendDisplayToProjectorCommandExecute);
        }

        public override string Title { get { return "GridDisplayVM"; } }

        #region Interface

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

        void DisplayedPages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UGridRows = DisplayedPages.Count < 3 ? 1 : 0;
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

        /// <summary>
        /// Sends the current Display to the projector.
        /// </summary>
        public Command SendDisplayToProjectorCommand { get; private set; }

        private void OnSendDisplayToProjectorCommandExecute()
        {
            //if(App.Network.ProjectorProxy != null)
            //{
            //    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;
            //    foreach(var gridDisplay in (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays)
            //    {
            //        gridDisplay.IsOnProjector = false;
            //    }

            //    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.IsOnProjector = true;
            //    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);

            //    List<string> pageIDs = new List<string>();
            //    if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
            //    {
            //        var page = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            //        string pageID;
            //        if(page.SubmissionType != SubmissionType.None)
            //        {
            //            pageID = page.SubmissionID;
            //        }
            //        else
            //        {
            //            pageID = page.UniqueID;
            //        }
            //        pageIDs.Add(pageID);
            //        try
            //        {
            //            // App.Network.ProjectorProxy.SwitchProjectorDisplay((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.DisplayName, pageIDs);
            //        }
            //        catch(System.Exception)
            //        {

            //        }
            //    }
            //    else
            //    {
            //        foreach(var page in ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as GridDisplayViewModel).DisplayedPages)
            //        {
            //            if(page.SubmissionType != SubmissionType.None)
            //            {
            //                pageIDs.Add(page.SubmissionID);
            //            }
            //            else
            //            {
            //                pageIDs.Add(page.UniqueID);
            //            }
            //        }
            //        try
            //        {
            //            //  App.Network.ProjectorProxy.SwitchProjectorDisplay((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.DisplayID, pageIDs);
            //        }
            //        catch(System.Exception)
            //        {

            //        }
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("Projector NOT Available");
            //}
        }

        #endregion //Commands

    }
}