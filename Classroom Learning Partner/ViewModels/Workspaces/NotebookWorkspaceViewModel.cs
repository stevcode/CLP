﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using Brush = System.Windows.Media.Brush;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(RibbonViewModel))]
    [InterestedIn(typeof(MainWindowViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(Notebook notebook)
        {
            Notebook = notebook;
            CurrentDisplay = SingleDisplay;

            NotebookPagesPanel = new NotebookPagesPanelViewModel(notebook);
            StudentWorkPanel = new StudentWorkPanelViewModel(notebook);
            ProgressPanel = new ProgressPanelViewModel(notebook);
            LeftPanel = NotebookPagesPanel;

            DisplaysPanel = new DisplaysPanelViewModel(notebook);
            RightPanel = DisplaysPanel;

            // TODO: Use StagingPanel instead?
            //if(App.CurrentUserMode == App.UserMode.Student)
            //{
            //    SubmissionHistoryPanel = new SubmissionHistoryPanelViewModel(notebook);
            //    BottomPanel = SubmissionHistoryPanel;
            //}

            if(App.CurrentUserMode == App.UserMode.Projector)
            {
                NotebookPagesPanel.IsVisible = false;
            }

            // TODO: Convert this to string, see DisplaysPanelViewModel to pull from CLPBrushes.xaml
            WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
        }

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Model
        /// </summary>
        [Model]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public SingleDisplay SingleDisplay
        {
            get { return GetValue<SingleDisplay>(SingleDisplayProperty); }
            set { SetValue(SingleDisplayProperty, value); }
        }

        public static readonly PropertyData SingleDisplayProperty = RegisterProperty("SingleDisplay", typeof(SingleDisplay));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>));

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
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay));

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
        /// StudentWorkPanel.
        /// </summary>
        public StudentWorkPanelViewModel StudentWorkPanel
        {
            get { return GetValue<StudentWorkPanelViewModel>(StudentWorkPanelProperty); }
            set { SetValue(StudentWorkPanelProperty, value); }
        }

        public static readonly PropertyData StudentWorkPanelProperty = RegisterProperty("StudentWorkPanel", typeof(StudentWorkPanelViewModel));

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
        /// DisplaysPanel.
        /// </summary>
        public DisplaysPanelViewModel DisplaysPanel
        {
            get { return GetValue<DisplaysPanelViewModel>(DisplaysPanelProperty); }
            set { SetValue(DisplaysPanelProperty, value); }
        }

        public static readonly PropertyData DisplaysPanelProperty = RegisterProperty("DisplaysPanel", typeof(DisplaysPanelViewModel));

        /// <summary>
        /// SubmissionHistoryPanel.
        /// </summary>
        // TODO: Replace with StagingPanel?
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
            if(viewModel == null)
            {
                return;
            }

            if(viewModel is RibbonViewModel)
            {
                var ribbon = viewModel as RibbonViewModel;
                if(propertyName == "DisplayPanelVisibility")
                {
                    RightPanel = DisplaysPanel;
                    RightPanel.IsVisible = ribbon.DisplayPanelVisibility;
                }

                if(propertyName == "CurrentLeftPanel")
                {
                    switch(ribbon.CurrentLeftPanel)
                    {
                        case Panels.NotebookPages:
                            LeftPanel = NotebookPagesPanel;
                            LeftPanel.IsVisible = true;
                            break;
                        case Panels.StudentWork:
                            LeftPanel = StudentWorkPanel;
                            LeftPanel.IsVisible = true;
                            break;
                        case Panels.Progress:
                            LeftPanel = ProgressPanel;
                            LeftPanel.IsVisible = true;
                            break;
                        default:
                            LeftPanel.IsVisible = false;
                            break;
                    }
                }
            }

            if(viewModel is MainWindowViewModel)
            {
                var mainWindow = viewModel as MainWindowViewModel;
                if(propertyName == "IsAuthoring")
                {
                    CurrentDisplay = SingleDisplay;
                    if(mainWindow.IsAuthoring)
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                        mainWindow.Ribbon.AuthoringTabVisibility = Visibility.Visible;
                    }
                    else
                    {
                        WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
                        mainWindow.Ribbon.AuthoringTabVisibility = Visibility.Collapsed;
                    }
                }
            }           

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods

        #region Static Methods

        public static void CreateNewNotebook()
        {
            var nameChooserLoop = true;

            while(nameChooserLoop)
            {
                var nameChooser = new NotebookNamerWindowView {Owner = Application.Current.MainWindow};
                nameChooser.ShowDialog();
                if(nameChooser.DialogResult == true)
                {
                    string notebookName = nameChooser.NotebookName.Text;
                    string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";

                    if(!File.Exists(filePath))
                    {
                        var newNotebook = new Notebook {Name = notebookName};
                        var newPage = new CLPPage();
                        newNotebook.AddCLPPageToNotebook(newPage);
                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;

                        nameChooserLoop = false;
                        //Send empty notebook to db
                        //ObjectSerializer.ToString(newNotebookViewModel)
                    }
                    else
                    {
                        MessageBox.Show("A Notebook with that name already exists. Please choose a different name.");
                    }
                }
                else
                {
                    nameChooserLoop = false;
                }
            }
        }

        #endregion //Static Methods
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 