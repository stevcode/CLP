using System;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

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

        public BackStageViewModel() { InitializeCommands(); }

        private void InitializeCommands() { HideBackStageCommand = new Command(OnHideBackStageCommandExecute); }

        #region Bindings

        /// <summary>Title Text for the currnet navigation pane.</summary>
        public string PaneTitleText
        {
            get { return GetValue<string>(PaneTitleTextProperty); }
            set { SetValue(PaneTitleTextProperty, value); }
        }

        public static readonly PropertyData PaneTitleTextProperty = RegisterProperty("PaneTitleText", typeof (string), string.Empty);

        /// <summary>Currently Displayed Navigation Pane.</summary>
        public NavigationPanes CurrentNavigationPane
        {
            get { return GetValue<NavigationPanes>(CurrentNavigationPaneProperty); }
            set
            {
                SetValue(CurrentNavigationPaneProperty, value);
                SetBackStagePane();
            }
        }

        public static readonly PropertyData CurrentNavigationPaneProperty = RegisterProperty("CurrentNavigationPane",
                                                                                             typeof (NavigationPanes),
                                                                                             NavigationPanes.Info);

        /// <summary>Pane currently displayed.</summary>
        public APaneBaseViewModel DisplayedPane
        {
            get { return GetValue<APaneBaseViewModel>(DisplayedPaneProperty); }
            set { SetValue(DisplayedPaneProperty, value); }
        }

        public static readonly PropertyData DisplayedPaneProperty = RegisterProperty("DisplayedPane", typeof (APaneBaseViewModel));

        #endregion //Bindings

        #region Commands

        /// <summary>Hides the BackStage.</summary>
        public Command HideBackStageCommand { get; private set; }

        private void OnHideBackStageCommandExecute() { MainWindow.IsBackStageVisible = false; }

        #endregion //Commands

        #region Methods

        private void SetBackStagePane()
        {
            switch (CurrentNavigationPane)
            {
                case NavigationPanes.Info:
                    DisplayedPane = new NotebookInfoPaneViewModel();
                    break;
                case NavigationPanes.New:
                    DisplayedPane = new NewNotebookPaneViewModel();
                    break;
                case NavigationPanes.Open:
                    DisplayedPane = new OpenNotebookPaneViewModel();
                    break;
                case NavigationPanes.Save:
                    //DisplayedPane = new SaveNotebookPaneViewModel();
                    SaveCurrentNotebook();
                    break;
                case NavigationPanes.Export:
                    DisplayedPane = new ExportPaneViewModel();
                    break;
                case NavigationPanes.Options:
                    DisplayedPane = new OptionsPaneViewModel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            PaneTitleText = DisplayedPane.PaneTitleText;
        }

        private void SaveCurrentNotebook()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService.CurrentNotebook == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(notebookService.SaveCurrentNotebook, null, "Saving Notebook");
        }

        #endregion //Methods
    }
}