using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Brush = System.Windows.Media.Brush;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    [InterestedIn(typeof(RibbonViewModel))]
    
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;
            SelectedDisplay = MirrorDisplay;

            ProgressPanel = new ProgressPanelViewModel(notebook);
            NotebookPagesPanel = new NotebookPagesPanelViewModel(notebook);
            LeftPanel = ProgressPanel; //NotebookPagesPanel;
            ProgressPanel.IsVisible = true;
            DisplayListPanel = new DisplayListPanelViewModel(notebook);
            RightPanel = DisplayListPanel;

            if(App.CurrentUserMode == App.UserMode.Student)
            {
                SubmissionHistoryPanel = new SubmissionHistoryPanelViewModel(notebook);
                BottomPanel = SubmissionHistoryPanel;
            }

            if(App.CurrentUserMode == App.UserMode.Projector)
            {
                NotebookPagesPanel.IsVisible = false;
            }

            WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
        }

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        #region Model

        /// <summary>
        /// Model
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPMirrorDisplay MirrorDisplay
        {
            get { return GetValue<CLPMirrorDisplay>(MirrorDisplayProperty); }
            set { SetValue(MirrorDisplayProperty, value); }
        }

        public static readonly PropertyData MirrorDisplayProperty = RegisterProperty("MirrorDisplay", typeof(CLPMirrorDisplay));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<ICLPDisplay> Displays
        {
            get { return GetValue<ObservableCollection<ICLPDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<ICLPDisplay>));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Color of Workspace Background.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        #region Displays

        /// <summary>
        /// The Currently Selected Display.
        /// </summary>
        public ICLPDisplay SelectedDisplay
        {
            get { return GetValue<ICLPDisplay>(SelectedDisplayProperty); }
            set { SetValue(SelectedDisplayProperty, value); }
        }

        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(ICLPDisplay));

        #endregion //Displays

        #region Panels

        /// <summary>
        /// Right side Panel.
        /// </summary>
        public IPanel RightPanel
        {
            get { return GetValue<IPanel>(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        public static readonly PropertyData RightPanelProperty = RegisterProperty("RightPanel", typeof(IPanel));

        /// <summary>
        /// Left side Panel.
        /// </summary>
        public IPanel LeftPanel
        {
            get { return GetValue<IPanel>(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        public static readonly PropertyData LeftPanelProperty = RegisterProperty("LeftPanel", typeof(IPanel));

        /// <summary>
        /// Bottom Panel.
        /// </summary>
        public IPanel BottomPanel
        {
            get { return GetValue<IPanel>(BottomPanelProperty); }
            set { SetValue(BottomPanelProperty, value); }
        }

        public static readonly PropertyData BottomPanelProperty = RegisterProperty("BottomPanel", typeof(IPanel));

        /// <summary>
        /// NotebookPagesPanel.
        /// </summary>
        public NotebookPagesPanelViewModel NotebookPagesPanel
        {
            get { return GetValue<NotebookPagesPanelViewModel>(NotebookPagesPanelProperty); }
            set { SetValue(NotebookPagesPanelProperty, value); }
        }

        public static readonly PropertyData NotebookPagesPanelProperty = RegisterProperty("NotebookPagesPanel", typeof(NotebookPagesPanelViewModel));

        /// <summary>
        /// ProgressPanel.
        /// </summary>
        public ProgressPanelViewModel ProgressPanel
        {
            get { return GetValue<ProgressPanelViewModel>(ProgressPanelProperty); }
            set { SetValue(ProgressPanelProperty, value); }
        }

        public static readonly PropertyData ProgressPanelProperty = RegisterProperty("ProgressPanel", typeof(ProgressPanelViewModel));

        /// <summary>
        /// DisplayPanel.
        /// </summary>
        public DisplayListPanelViewModel DisplayListPanel
        {
            get { return GetValue<DisplayListPanelViewModel>(DisplayListPanelProperty); }
            set { SetValue(DisplayListPanelProperty, value); }
        }

        public static readonly PropertyData DisplayListPanelProperty = RegisterProperty("DisplayListPanel", typeof(DisplayListPanelViewModel));

        /// <summary>
        /// SubmissionHistoryPanel.
        /// </summary>
        public SubmissionHistoryPanelViewModel SubmissionHistoryPanel
        {
            get { return GetValue<SubmissionHistoryPanelViewModel>(SubmissionHistoryPanelProperty); }
            set { SetValue(SubmissionHistoryPanelProperty, value); }
        }

        public static readonly PropertyData SubmissionHistoryPanelProperty = RegisterProperty("SubmissionHistoryPanel", typeof(SubmissionHistoryPanelViewModel));
         
        #endregion //Panels

        #endregion //Bindings

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsAuthoring")
            {                
                SelectedDisplay = MirrorDisplay;
                if((viewModel as MainWindowViewModel).IsAuthoring)
                {
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;
                }
                else
                {
                    WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Collapsed;
                }
            }

            if (propertyName == "SideBarVisibility")
            {
                LeftPanel = ProgressPanel; //NotebookPagesPanel;
                LeftPanel.IsVisible = (viewModel as RibbonViewModel).SideBarVisibility;
            }

            if(propertyName == "DisplayPanelVisibility")
            {
                RightPanel = DisplayListPanel;
                RightPanel.IsVisible = (viewModel as RibbonViewModel).DisplayPanelVisibility;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName); 
        }

        #endregion //Methods
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           