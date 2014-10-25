using System;
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
        /// Title Text for the currnet navigation pane.
        /// </summary>
        public string DetailTitleText
        {
            get { return GetValue<string>(DetailTitleTextProperty); }
            set { SetValue(DetailTitleTextProperty, value); }
        }

        public static readonly PropertyData DetailTitleTextProperty = RegisterProperty("DetailTitleText", typeof (string), string.Empty);

        /// <summary>
        /// Currently Displayed Navigation Pane.
        /// </summary>
        public NavigationPanes CurrentNavigationPane
        {
            get { return GetValue<NavigationPanes>(CurrentNavigationPaneProperty); }
            set
            {
                SetValue(CurrentNavigationPaneProperty, value);
                SetBackStagePane();
            }
        }

        public static readonly PropertyData CurrentNavigationPaneProperty = RegisterProperty("CurrentNavigationPane", typeof (NavigationPanes), NavigationPanes.Info); 

        #endregion //Bindings

        #region Commands

        /// <summary>Hides the BackStage.</summary>
        public Command HideBackStageCommand { get; private set; }

        private void OnHideBackStageCommandExecute() { MainWindow.IsBackStageVisible = false; }

        #endregion //Commands

        #region Methods

        public void SetBackStagePane()
        {
            switch (CurrentNavigationPane)
            {
                case NavigationPanes.Info:
                    DetailTitleText = "Notebook Information";
                    break;
                case NavigationPanes.New:
                    DetailTitleText = "New Notebook";
                    break;
                case NavigationPanes.Open:
                    DetailTitleText = "Open Notebook";
                    break;
                case NavigationPanes.Save:
                    DetailTitleText = "Save Notebook";
                    break;
                case NavigationPanes.Export:
                    DetailTitleText = "Export";
                    break;
                case NavigationPanes.Options:
                    DetailTitleText = "Options";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion //Methods
    }
}