using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using System;
using System.IO;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ProgressPanelViewModel : ViewModelBase, IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPanelViewModel"/> class.
        /// </summary>
        public ProgressPanelViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;

            CurrentPages = new ObservableCollection<ICLPPage>();
            CurrentPages.Add(Notebook.Pages[2]);
            CurrentPages.Add(Notebook.Pages[3]);
            CurrentPages.Add(Notebook.Pages[4]);
            CurrentPages.Add(Notebook.Pages[5]);

            PanelWidth = InitialWidth;
            StudentList = GetStudentNames();
            PanelResizeDragCommand = new Command<DragDeltaEventArgs>(OnPanelResizeDragCommandExecute);
            SetCurrentPageCommand = new Command<ICLPPage>(OnSetCurrentPageCommandExecute);
        }

         /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "ProgressPanelVM"; } }

        #region Model

        /// <summary>
        /// Notebook associated with the panel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        /// <summary>
        /// Pages of the Notebook.
        /// </summary>
        public ObservableCollection<ICLPPage> CurrentPages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(CurrentPagesProperty); }
            set { SetValue(CurrentPagesProperty, value); }
        }

        public static readonly PropertyData CurrentPagesProperty = RegisterProperty("CurrentPages", typeof(ObservableCollection<ICLPPage>));

        #endregion //Model
        
        #region IPanel Members

        public string PanelName { get { return "ProgressPanel"; } }

        /// <summary>
        /// Whether the Panel is pinned to the same Z-Index as the Workspace.
        /// </summary>
        public bool IsPinned
        {
            get { return GetValue<bool>(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        public static readonly PropertyData IsPinnedProperty = RegisterProperty("IsPinned", typeof(bool), true);

        /// <summary>
        /// Visibility of Panel, True for Visible, False for Collapsed.
        /// </summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), true);

        /// <summary>
        /// Can the Panel be resized.
        /// </summary>
        public bool IsResizable
        {
            get { return GetValue<bool>(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), true);

        /// <summary>
        /// Initial Width of the Panel, before any resizing.
        /// </summary>
        public double InitialWidth
        {
            get { return 250; }
        }

        public double PanelWidth
        {
            get { return GetValue<double>(PanelWidthProperty); }
            set { SetValue(PanelWidthProperty, value); }
        }

        public static readonly PropertyData PanelWidthProperty = RegisterProperty("PanelWidth", typeof(double), 250);

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Left);

        /// <summary>
        /// A Linked IPanel if more than one IPanel is to be used in the same Location.
        /// </summary>
        public IPanel LinkedPanel
        {
            get { return GetValue<IPanel>(LinkedPanelProperty); }
            set { SetValue(LinkedPanelProperty, value); }
        }

        public static readonly PropertyData LinkedPanelProperty = RegisterProperty("LinkedPanel", typeof(IPanel));

        #endregion

        #region Bindings

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage));

        public ObservableCollection<string> StudentList
        {
            get { return GetValue<ObservableCollection<string>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<string>));

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Resizes the panel.
        /// </summary>
        public Command<DragDeltaEventArgs> PanelResizeDragCommand
        {
            get;
            private set;
        }

        private void OnPanelResizeDragCommandExecute(DragDeltaEventArgs e)
        {
            var newWidth = PanelWidth + e.HorizontalChange;
            if(newWidth < 50)
            {
                newWidth = 50;
            }
            if(newWidth > Application.Current.MainWindow.ActualWidth - 100)
            {
                newWidth = Application.Current.MainWindow.ActualWidth - 100;
            }
            PanelWidth = newWidth;
        }

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<ICLPPage> SetCurrentPageCommand
        {
            get;
            private set;
        }

        private void OnSetCurrentPageCommandExecute(ICLPPage page)
        {
            CurrentPage = page;
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.SelectedDisplay.AddPageToDisplay(page);
                var historyPanel = notebookWorkspaceViewModel.SubmissionHistoryPanel;
                if(historyPanel != null)
                {
                    historyPanel.CurrentPage = null;
                    historyPanel.IsSubmissionHistoryVisible = false;
                }
            }
        }

        #endregion
        //This is copied over from SubmissionsPanelViewModel, it wants to be 
        //database-agnostic when stuff's finalized
        public ObservableCollection<string> GetStudentNames()
        {
            var userNames = new ObservableCollection<string>();
            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";
            userNames.Add("Original");
            if(File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string name;
                while((name = reader.ReadLine()) != null)
                {
                    var user = name.Split(new[] { ',' })[0];
                    userNames.Add(user);
                }
                reader.Dispose();
            }
            return userNames;
        }
    }
}
