using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
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
            ReplayHistoryCommand = new Command<CLPPage>(OnReplayHistoryCommandExecute);
        }

        #region Overrides of ViewModelBase

        protected override void OnClosing()
        {
            Pages.CollectionChanged -= Pages_CollectionChanged;
            base.OnClosing();
        }

        #endregion

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

            if(App.Network.ProjectorProxy == null ||
               App.CurrentUserMode != App.UserMode.Instructor ||
               IsDisplayPreview)
            {
                return;
            }

            if(e.OldItems != null)
            {
                foreach(var page in e.OldItems.OfType<CLPPage>()) 
                {
                    try
                    {
                        App.Network.ProjectorProxy.RemovePageFromDisplay(page.ID, page.OwnerID, page.DifferentiationLevel, page.VersionIndex, GridDisplay.ID);
                    }
                    catch { }
                }
            }

            if(e.NewItems != null)
            {
                foreach(var page in e.NewItems.OfType<CLPPage>()) 
                {
                    try
                    {
                        App.Network.ProjectorProxy.AddPageToDisplay(page.ID, page.OwnerID, page.DifferentiationLevel, page.VersionIndex, GridDisplay.ID);
                    }
                    catch { }
                }
            }  
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand command.
        /// </summary>
        public Command<CLPPage> RemovePageFromGridDisplayCommand { get; private set; }

        public void OnRemovePageFromGridDisplayCommandExecute(CLPPage page) { GridDisplay.RemovePageFromDisplay(page); }

        /// <summary>
        /// Replays the interaction history of the page on the Grid Display.
        /// </summary>
        public Command<CLPPage> ReplayHistoryCommand { get; private set; }

        private void OnReplayHistoryCommandExecute(CLPPage page)
        {
            var currentPage = page;
            if(currentPage == null) { return; }

            currentPage.IsTagAddPrevented = true;
            var oldPageInteractionMode = (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.None) ? PageInteractionMode.Pen : App.MainWindowViewModel.Ribbon.PageInteractionMode;
            App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.None;

            while(currentPage.History.UndoItems.Any()) { currentPage.History.Undo(); }

            var t = new Thread(() =>
                               {
                                   while(currentPage.History.RedoItems.Any())
                                   {
                                       var historyItemAnimationDelay = Convert.ToInt32(Math.Round(currentPage.History.CurrentAnimationDelay / 1.0));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              currentPage.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyItemAnimationDelay);
                                   }
                                   currentPage.IsTagAddPrevented = false;
                                   App.MainWindowViewModel.Ribbon.PageInteractionMode = oldPageInteractionMode;
                               });

            t.Start();
        }

        #endregion //Commands
    }
}