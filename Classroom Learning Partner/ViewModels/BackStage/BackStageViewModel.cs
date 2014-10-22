using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum NavigationPanes
    {
        Info,
        New,
        Open,
        Save,
        Export,
        Options
    }

    public class BackStageViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        public static CLPPage CurrentPage
        {
            get { return NotebookPagesPanelViewModel.GetCurrentPage(); }
        }

        public BackStageViewModel() { InitializeCommands(); }

        private void InitializeCommands() { HideBackStageCommand = new Command(OnHideBackStageCommandExecute); }

        #region Bindings

        /// <summary>
        /// Currently Displayed Navigation Pane.
        /// </summary>
        public NavigationPanes CurrentNavigationPane
        {
            get { return GetValue<NavigationPanes>(CurrentNavigationPaneProperty); }
            set { SetValue(CurrentNavigationPaneProperty, value); }
        }

        public static readonly PropertyData CurrentNavigationPaneProperty = RegisterProperty("CurrentNavigationPane", typeof (NavigationPanes), NavigationPanes.Info); 

        #endregion //Bindings

        #region Commands

        /// <summary>Hides the BackStage.</summary>
        public Command HideBackStageCommand { get; private set; }

        private void OnHideBackStageCommandExecute() { MainWindow.IsBackStageVisible = false; }

        #endregion //Commands
    }
}