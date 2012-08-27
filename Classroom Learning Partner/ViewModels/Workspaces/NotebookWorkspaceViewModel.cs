using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(CLPNotebook notebook)
            : base()
        {
            SetCurrentPageCommand = new Command<MouseButtonEventArgs>(OnSetCurrentPageCommandExecute);
            SetCurrentGridDisplayCommand = new Command<MouseButtonEventArgs>(OnSetCurrentGridDisplayCommandExecute);

            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            Notebook = notebook;
            SubmissionPages = new ObservableCollection<CLPPage>();
            GridDisplays = new ObservableCollection<GridDisplayViewModel>();

            Notebook.GeneratePageIndexes();

            FilteredSubmissions = new CollectionViewSource();
            FilterTypes = new ObservableCollection<string>();
            FilterTypes.Add("Student Name - Ascending");
            FilterTypes.Add("Student Name - Descending");
            FilterTypes.Add("Time In - Ascending");
            FilterTypes.Add("Time In - Descending");
        }

        public void InitializeLinkedDisplay()
        {
            Console.WriteLine("LinkedDisplay Initialization Started");

            LinkedDisplay = new LinkedDisplayViewModel(CurrentPage);

            SelectedDisplay = LinkedDisplay;

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                SelectedDisplay.IsOnProjector = true;
                WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
            }
            else
            {
                SelectedDisplay.IsOnProjector = false;
            }

            Console.WriteLine("LinkedDisplay Initialization Ended");
        }

        protected override void Close()
        {
            Console.WriteLine(Title + " closed");
            base.Close();
        }

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        /// <summary>
        /// Register the Notebook property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Notebook","Pages")]
        public ObservableCollection<CLPPage> NotebookPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(NotebookPagesProperty); }
            set { SetValue(NotebookPagesProperty, value); }
        }

        /// <summary>
        /// Register the NotebookPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookPagesProperty = RegisterProperty("NotebookPages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<GridDisplayViewModel> GridDisplays
        {
            get { return GetValue<ObservableCollection<GridDisplayViewModel>>(GridDisplaysProperty); }
            set { SetValue(GridDisplaysProperty, value); }
        }

        /// <summary>
        /// Register the GridDisplays property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GridDisplaysProperty = RegisterProperty("GridDisplays", typeof(ObservableCollection<GridDisplayViewModel>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public LinkedDisplayViewModel LinkedDisplay
        {
            get { return GetValue<LinkedDisplayViewModel>(LinkedDisplayProperty); }
            private set { SetValue(LinkedDisplayProperty, value); }
        }

        /// <summary>
        /// Register the LinkedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LinkedDisplayProperty = RegisterProperty("LinkedDisplay", typeof(LinkedDisplayViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public IDisplayViewModel SelectedDisplay
        {
            get { return GetValue<IDisplayViewModel>(SelectedDisplayProperty); }
            set
            {
                SetValue(SelectedDisplayProperty, value);
                if (SelectedDisplay != null)
                {
                    if (SelectedDisplay.IsOnProjector)
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
                    }
                    else
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
                    }
                }
            }
        }

        /// <summary>
        /// Register the SelectedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(IDisplayViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Register the WorkspaceBackgroundColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionPagesProperty); }
            set 
            { 
                SetValue(SubmissionPagesProperty, value);
                SelectedFilterType = "Student Name - Ascending"; 
                FilterSubmissions("Student Name - Ascending");
            } 
        }

        /// <summary>
        /// Register the SubmissionPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource FilteredSubmissions
        {
            get { return GetValue<CollectionViewSource>(FilteredSubmissionsProperty); }
            set { SetValue(FilteredSubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the FilteredSubmissionsProperty property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FilteredSubmissionsProperty = RegisterProperty("FilteredSubmissions", typeof(CollectionViewSource), null);

        public void FilterSubmissions(string Sort)
        {
            FilteredSubmissions = new CollectionViewSource();
            FilteredSubmissions.Source = SubmissionPages;
            FilteredSubmissions.SortDescriptions.Clear();

            PropertyGroupDescription gd = new PropertyGroupDescription();
            gd.PropertyName = "SubmitterName";

            if(Sort == "Student Name - Ascending")
            {
                FilteredSubmissions.GroupDescriptions.Add(gd);
                SortDescription sdAA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
                FilteredSubmissions.SortDescriptions.Add(sdAA);
            }
            else if(Sort == "Student Name - Descending")
            {
                FilteredSubmissions.GroupDescriptions.Add(gd);
                SortDescription sdAD = new SortDescription("SubmitterName", ListSortDirection.Descending);
                FilteredSubmissions.SortDescriptions.Add(sdAD);
            }
            else if(Sort == "Time In - Ascending")
            {
                SortDescription sdTA = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
                FilteredSubmissions.SortDescriptions.Add(sdTA);
            }
            else if(Sort == "Time In - Descending")
            {
                SortDescription sdTD = new SortDescription("SubmissionTime", ListSortDirection.Descending);
                FilteredSubmissions.SortDescriptions.Add(sdTD);
            }
        }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> FilterTypes
        {
            get { return GetValue<ObservableCollection<string>>(FilterTypesProperty); }
            set { SetValue(FilterTypesProperty, value); }
        }

        /// <summary>
        /// Register the FilterTypes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FilterTypesProperty = RegisterProperty("FilterTypes", typeof(ObservableCollection<string>), null);


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string SelectedFilterType
        {
            get { return GetValue<string>(SelectedFilterTypeProperty); }
            set
            {
                SetValue(SelectedFilterTypeProperty, value);
                FilterSubmissions(SelectedFilterType);
            }
        }

        /// <summary>
        /// Register the SelectedFilterType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedFilterTypeProperty = RegisterProperty("SelectedFilterType", typeof(string), null);


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPageViewModel CurrentPage
        {
            get { return GetValue<CLPPageViewModel>(CurrentPageProperty); }

            set
            {
                if (CurrentPage != null)
                {
                    try
                    {
                        CurrentPage.stopAudioPlayback();
                        CurrentPage.stopAudio();
                        CurrentPage.StopPlayback();
                        App.MainWindowViewModel.isRecordingAudio = false;
                        
                       }
                    catch (Exception e)
                    { }
                }
                SetValue(CurrentPageProperty, value);
                if (LinkedDisplay == null)
                {
                    InitializeLinkedDisplay();
                }
                
                SelectedDisplay.AddPageToDisplay(value);

                String pageID = CurrentPage.Page.UniqueID;
                String notebookID = CurrentPage.Page.ParentNotebookID.ToString();
                App.MainWindowViewModel.PageHasAudioFile = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\" + notebookID + @" - " + pageID + ".wav");
                App.MainWindowViewModel.AudioPlayImage = new Uri("..\\Images\\play2.png", UriKind.Relative);
                App.MainWindowViewModel.AudioRecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);

                try
                {
                    App.MainWindowViewModel.record_timer.Stop();
                    App.MainWindowViewModel.record_timer.Dispose();
                }
                catch (Exception)
                { }

                Console.WriteLine("CurrentPage Set");
            }
        }

        /// <summary>
        /// Register the CurrentPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPageViewModel));

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetCurrentPageCommand command is executed.
        /// </summary>
        private void OnSetCurrentPageCommandExecute(MouseButtonEventArgs e)
        {
            CurrentPage = ((e.Source as CLPPagePreviewView).ViewModel as CLPPageViewModel);
        }

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentGridDisplayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetCurrentPageCommand command is executed.
        /// </summary>
        private void OnSetCurrentGridDisplayCommandExecute(MouseButtonEventArgs e)
        {
            //try
            //{
                //SelectedDisplay = null;
                SelectedDisplay = ((e.Source as ItemsControl).DataContext as GridDisplayViewModel);
            //}
            //catch (Exception ex)
            //{
            //}  
        }

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsAuthoring")
            {
                if (LinkedDisplay == null)
                {
                    InitializeLinkedDisplay();
                }
                SelectedDisplay = LinkedDisplay;
                if ((viewModel as MainWindowViewModel).IsAuthoring)
                {
                    SelectedDisplay.IsOnProjector = false;
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    App.MainWindowViewModel.AuthoringTabVisibility = Visibility.Visible;
                }
                else
                {
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
                    App.MainWindowViewModel.AuthoringTabVisibility = Visibility.Collapsed;
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
            
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           