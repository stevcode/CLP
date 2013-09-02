using System.Windows.Controls;
using System.Windows.Input;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class DisplayListPanelViewModel : ViewModelBase, IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayListPanelViewModel"/> class.
        /// </summary>
        public DisplayListPanelViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;
            SetCurrentGridDisplayCommand = new Command<MouseButtonEventArgs>(OnSetCurrentGridDisplayCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "DisplayListPanelVM"; } }

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model]
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

        #endregion //Model

        #region IPanel Members

        public string PanelName
        {
            get
            {
                return "DisplayListPanel";
            }
        }

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

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), false);

        /// <summary>
        /// Initial Width of the Panel, before any resizing.
        /// </summary>
        public double InitialWidth
        {
            get { return 250; }
        }

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Right);

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

        #region Commands

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentGridDisplayCommand { get; private set; }

        private void OnSetCurrentGridDisplayCommandExecute(MouseButtonEventArgs e)
        {
            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = ((e.Source as ItemsControl).DataContext as GridDisplayViewModel);
        }

        #endregion //Commands
    }
}
