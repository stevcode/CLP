﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
            OnSetMirrorDisplayCommandExecute();

            AddGridDisplayCommand = new Command(OnAddGridDisplayCommandExecute);
            SetMirrorDisplayCommand = new Command(OnSetMirrorDisplayCommandExecute);
            RemoveDisplayCommand = new Command<ICLPDisplay>(OnRemoveDisplayCommandExecute);
            IsVisible = false;
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

        #region Bindings

        /// <summary>
        /// Color of the highlighted border around the MirrorDisplay.
        /// </summary>
        public string MirrorDisplaySelectedColor
        {
            get { return GetValue<string>(MirrorDisplaySelectedColorProperty); }
            set { SetValue(MirrorDisplaySelectedColorProperty, value); }
        }

        public static readonly PropertyData MirrorDisplaySelectedColorProperty = RegisterProperty("MirrorDisplaySelectedColor", typeof(string));

        /// <summary>
        /// Whether or not the MirrorDisplay has been sent to the projector.
        /// </summary>
        public bool MirrorDisplayIsOnProjector
        {
            get { return GetValue<bool>(MirrorDisplayIsOnProjectorProperty); }
            set { SetValue(MirrorDisplayIsOnProjectorProperty, value); }
        }

        public static readonly PropertyData MirrorDisplayIsOnProjectorProperty = RegisterProperty("MirrorDisplayIsOnProjector", typeof(bool), false, OnIsOnProjectorChanged);

        private static void OnIsOnProjectorChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var displayListPanel = sender as DisplayListPanelViewModel;
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(displayListPanel == null || notebookWorkspaceViewModel == null || App.CurrentUserMode != App.UserMode.Instructor)
            {
                return;
            }

            displayListPanel.MirrorDisplayIsOnProjector = advancedPropertyChangedEventArgs.NewValue is bool && (bool)advancedPropertyChangedEventArgs.NewValue;
            if(!displayListPanel.MirrorDisplayIsOnProjector)
            {
                return;
            }

            if(App.Network.ProjectorProxy == null)
            {
                displayListPanel.MirrorDisplayIsOnProjector = false;
                displayListPanel.ProjectedDisplayString = string.Empty;

                return;
            }
            
            try
            {
                displayListPanel.ProjectedDisplayString = displayListPanel.MirrorDisplay.UniqueID;
                App.Network.ProjectorProxy.SwitchProjectorDisplay("MirrorDisplay", new List<string> { displayListPanel.Notebook.MirrorDisplay.CurrentPage.UniqueID });
            }
            catch(Exception)
            {

            }
        }

        /// <summary>
        /// The selected display in the list of the Notebook's Displays. Does not include the MirrorDisplay.
        /// </summary>
        public ICLPDisplay CurrentDisplay
        {
            get { return GetValue<ICLPDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(ICLPDisplay), null, OnCurrentDisplayChanged);

        private static void OnCurrentDisplayChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var displayListPanelViewModel = sender as DisplayListPanelViewModel;
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(displayListPanelViewModel == null || notebookWorkspaceViewModel == null || App.CurrentUserMode != App.UserMode.Instructor || args.NewValue == null)
            {
                return;
            }

            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["GrayBorderColor"].ToString();
            displayListPanelViewModel.MirrorDisplaySelectedColor = color;

            notebookWorkspaceViewModel.SelectedDisplay = args.NewValue as ICLPDisplay;
        }

        /// <summary>
        /// UniqueID of the projected display.
        /// </summary>
        public string ProjectedDisplayString
        {
            get { return GetValue<string>(ProjectedDisplayStringProperty); }
            set { SetValue(ProjectedDisplayStringProperty, value); }
        }

        public static readonly PropertyData ProjectedDisplayStringProperty = RegisterProperty("ProjectedDisplayString", typeof(string), string.Empty);

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Adds a GridDisplay to the notebook.
        /// </summary>
        public Command AddGridDisplayCommand { get; private set; }

        private void OnAddGridDisplayCommandExecute()
        {
            Notebook.AddDisplay(new CLPGridDisplay());
            CurrentDisplay = Displays.LastOrDefault();
        }     

        /// <summary>
        /// Sets the current display to the Mirror Display.
        /// </summary>
        public Command SetMirrorDisplayCommand { get; private set; }

        private void OnSetMirrorDisplayCommandExecute()
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["MainColor"].ToString();
            MirrorDisplaySelectedColor = color;
            CurrentDisplay = null;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.SelectedDisplay = MirrorDisplay;
            }
        }

        /// <summary>
        /// Hides the Display from the list of Displays. Allows permanently deletion if in Authoring Mode.
        /// </summary>
        public Command<ICLPDisplay> RemoveDisplayCommand { get; private set; }

        private void OnRemoveDisplayCommandExecute(ICLPDisplay display)
        {
            var result = MessageBox.Show("Are you sure you want to delete Grid Display " + (display as CLPGridDisplay).DisplayIndex +"?", "Delete Display?", MessageBoxButton.YesNo);

            if(result == MessageBoxResult.No)
            {
                return;
            }

            display.IsTrashed = true;
            OnSetMirrorDisplayCommandExecute();
        }

        #endregion //Commands

        #region Static Methods

        public static DisplayListPanelViewModel GetDisplayListPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.DisplayListPanel;
        }

        #endregion //Static Methods
    }
}