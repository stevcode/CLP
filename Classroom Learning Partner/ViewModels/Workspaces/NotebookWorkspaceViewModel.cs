using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;
using System.Windows.Media;
using System;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using Classroom_Learning_Partner.Views;
using System.Windows.Data;
using System.ComponentModel;
using System.IO;
using Classroom_Learning_Partner.Views.Displays;
using System.Windows.Controls;
using System;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
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

            SortTypes = new ObservableCollection<string>();
            SortTypes.Add("Student Name - Ascending");
            SortTypes.Add("Student Name - Descending");
            SortTypes.Add("Time In - Ascending");
            SortTypes.Add("Time In - Descending");

            SelectedCollectionViewSource = new CollectionViewSource();
            SelectedCollectionViewSource.Source = SubmissionPages;
            SwitchSortingMethod("Student Name - Ascending");

            //InitializeLinkedDisplay();
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
            set { SetValue(SubmissionPagesProperty, value); }
        }

        /// <summary>
        /// Register the SubmissionPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>));


        //List myList;
        //CollectionView myCollectionView = new CollectionView(myList);
        //myCollectionView.SortDescriptions.Add(new SortDescription("hey", ListSortDirection=Ascending));
        //AlphaSortedCollectionViewA.SortDescriptions.Clear();

        //CollectionViewSource AlphaSortedCollectionViewA = new CollectionViewSource();
        //AlphaSortedCollectionViewA.SortDescriptions.Clear();
        //SortDescription sdAA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
        //AlphaSortedCollectionViewA.SortDescriptions.Add(sdAA);

        //People.SortDescriptions.Clear()
        //SortDescription sd = new SortDescription("Name", ListSortOrder.Ascending)
        //People.SortDescriptions.Add(sd)

        //ListCollectionView lcv = CollectionViewSource.GetDefaultView(myCollection) as ListCollectionView;
        //lcv.SortDescriptions.Add(new SortDescription(…));

        //private string SelectedSort = "{Binding ElementName=ComboBox, Path=SlectedValue.Name()}";
        //private string SelectedSort;
        //Console.WriteLine("_selectedSort: {0}", "hi");

        //private CollectionViewSource SelectedCollectionViewSource;

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource SelectedCollectionViewSource
        {
            get { return GetValue<CollectionViewSource>(SelectedCollectionViewSourceProperty); }
            set { SetValue(SelectedCollectionViewSourceProperty, value); }
        }

        /// <summary>
        /// Register the SelectedCollectionViewSource property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedCollectionViewSourceProperty = RegisterProperty("SelectedCollectionViewSource", typeof(CollectionViewSource), null);

        public void SwitchSortingMethod(string Sort)
        {
            SelectedCollectionViewSource = new CollectionViewSource();
            SelectedCollectionViewSource.Source = "{Binding SubmissionPages}";
            bool FoundSort = false;

            if(Sort == "Student Name - Ascending")
            {
                SelectedCollectionViewSource.SortDescriptions.Clear();
                SortDescription sdAA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
                SelectedCollectionViewSource.SortDescriptions.Add(sdAA);
                FoundSort = true;
            }

            else if(Sort == "Student Name - Descending")
            {
                SelectedCollectionViewSource.SortDescriptions.Clear();
                SortDescription sdAD = new SortDescription("SubmitterName", ListSortDirection.Descending);
                SelectedCollectionViewSource.SortDescriptions.Add(sdAD);
                FoundSort = true;
            }

            else if(Sort == "Time In - Ascending")
            {
                SelectedCollectionViewSource.SortDescriptions.Clear();
                SortDescription sdTA = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
                SelectedCollectionViewSource.SortDescriptions.Add(sdTA);
                FoundSort = true;
            }

            else if(Sort == "Time In - Descending")
            {
                SelectedCollectionViewSource.SortDescriptions.Clear();
                SortDescription sdTD = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
                SelectedCollectionViewSource.SortDescriptions.Add(sdTD);
                FoundSort = true;
            }

        }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> SortTypes
        {
            get { return GetValue<ObservableCollection<string>>(SortTypeProperty); }
            set { SetValue(SortTypeProperty, value); }
        }

        /// <summary>
        /// Register the SortType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SortTypeProperty = RegisterProperty("SortType", typeof(ObservableCollection<string>), null);


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string SelectedSortType
        {
            get { return GetValue<string>(SelectedSortTypeProperty); }
            //set { SetValue(SelectedSortTypeProperty, SwitchSortingMethod(SelectedSortType)); }
            set
            {
                SetValue(SelectedSortTypeProperty, value);
                SwitchSortingMethod(SelectedSortType);
            }
        }

        /// <summary>
        /// Register the SelectedSortType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedSortTypeProperty = RegisterProperty("SelectedSortType", typeof(string), null);







        //private string SelectedSort = "{Binding ElementName=ComboBox, Path=SlectedItem.Name()}";
        ////Console.WriteLine("SelectedSort: {0}", SelectedSort);

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //public CollectionViewSource CurrentCollectionViewSource
        //{
        //    get { return GetValue<CollectionViewSource>(CurrentCollectionViewSourceProperty); }
        //    set { SetValue(CurrentCollectionViewSourceProperty, SwitchSortingMethod(SelectedSort)); }
        //}

        ///// <summary>
        ///// Register the CurrentCollectionViewSource property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData CurrentCollectionViewSourceProperty = RegisterProperty("CurrentCollectionViewSource", typeof(CollectionViewSource), null);



        //#Stuff that I added to ViewModel
        //CollectionViewSource AlphaSortedCollectionViewA = new CollectionViewSource();
        //SortDescription sdAA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
        //AlphaSortedCollectionViewA.SortDescriptions.Add(sdAA);


        //CollectionViewSource AlphaSortedCollectionViewD = new CollectionViewSource();
        //SortDescription sdAD = new SortDescription("SubmitterName", ListSortDirection.Descending);
        //AlphaSortedCollectionViewD.SortDescriptions.Add(sdAD);

        //CollectionViewSource TimeSortedCollectionViewA = new CollectionViewSource();
        //SortDescription sdTA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
        //AlphaSortedCollectionViewA.SortDescriptions.Add(sdTA);

        //CollectionViewSource TimeSortedCollectionViewD = new CollectionViewSource();
        //SortDescription sdTD = new SortDescription("SubmitterName", ListSortDirection.Ascending);
        //AlphaSortedCollectionViewD.SortDescriptions.Add(sdTD);


        //private string _selectedSort;
        //private CollectionViewSource _selectedCollectionViewSource;
        //public CollectionViewSource SelectedCollectionViewSource
        //{
        //    get { return _selectedCollectionViewSource}
        //    set 
        //    {
        //        if(_selectedSort == "SubmitterNameA")
        //            _selectedCollectionViewSource = AlphaSortedCollectionViewA;
        //        if(_selectedSort == "SubmitterNameD")
        //            _selectedCollectionViewSource = AlphaSortedCollectionViewD;
        //        if(_selectedSort == "SubmissionTimeA")
        //            _selectedCollectionViewSource = TimeSortedCollectionViewA;
        //        if(_selectedSort == "SubmissionTimeD")
        //            _selectedCollectionViewSource = TimeSortedCollectionViewD;   
        //    }
        //}








       
        //private ObservableCollection<Card> _combodata;
        //public ObservableCollection<Card> comboData
        //{
        //   get
        //   {
        //      if (_combodata == null)
        //         _combodata = new ObservableCollection<Card>();
        //      return _combodata;
        //   }
        //   set
        //   {
        //       if (value != _combodata)
        //           _combodata = value;
        //   }
        //}


        

//        private CollectionViewSource AlphaSortedCollectionViewA;
//public ICollectionView AlphaSortedCollectionViews
//{
//        get
//        {
//                if (AlphaSortedCollectionViewA == null)
//                {
//                        AlphaSortedCollectionViewA = new CollectionViewSource();
//                        //where listOfDataObjects is collection of your data objects
//                        //that you want to display
//                        AlphaSortedCollectionViews.Source = new ObservableCollection<DataObject>(listOfDataObjects);

//                }

//                return AlphaSortedCollectionViews.View;
//        }
//}




        //AlphaSortedCollectionViewA.Add(new SortDescription("SubmitterName", ListSortDirection.Ascending));
        //private static readonly SortDescription AlphaSortedCollectionViewB = new SortDescription("SubmitterName", ListSortDirection.Ascending);
        //private static readonly SortDescription AlphaSortedCollectionViewD = new SortDescription("SubmitterName", ListSortDirection.Descending);
        //private static readonly SortDescription TimeSortedCollectionViewA = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
        //private static readonly SortDescription TimeSortedCollectionViewD = new SortDescription("SubmissionTime", ListSortDirection.Descending);



        //AlphaSortedCollectionViewA.SortDescriptions.Add(new SortDescription("SubmissionTime", ListSortDirection.Ascending));

        //CollectionViewSource cvs = (CollectionView)CollectionViewSource.GetDefaultView();

        
        //CollectionViewSource AlphaSortedCollectionViewD = new CollectionViewSource();
        //SortDescription AlphaSortedCollectionViewD = new SortDescription();
        //public ObservableCollection<string> AvailableSorts { get; set; }

        //private string _selectedSort;
        //private CollectionViewSource _selectedCollectionViewSource;

        //public CollectionViewSource SelectedSortDescription
        //{
            

        //    get { return _selectedCollectionViewSource; }  
        //    set
        //    {
        //        _selectedCollectionViewSource = new CollectionViewSource();
        //        if(_selectedSort == "SubmitterNameA")
        //        {
        //            _selectedCollectionViewSource.Add(new SortDescription(AlphaSortedCollectionViewA, ListSortDirection.Ascending));
        //        }

        //        if(_selectedSort == "SubmitterNameD")
        //        {
        //            _selectedSortDescription = AlphaSortedCollectionViewD;
        //        }

        //        if(_selectedSort == "SubmissionTimeA")
        //        {
        //            _selectedSortDescription = TimeSortedCollectionViewA;
        //        }

        //        if(_selectedSort == "SubmissionTimeD")
        //        {
        //            _selectedSortDescription = TimeSortedCollectionViewD;
        //        }
        //    }
        //}

        //set
        //{
        //    if(_selectedSort == "SubmitterNameA")
        //    {
        //        ItemsSource = AlphaSortedCollectionViewA;
        //    }

        //    if(_selectedSort == "SubmitterNameD")
        //    {
        //        ItemsSource = AlphaSortedCollectionViewD;
        //    }

        //    if(_selectedSort == "SubmissionTimeA")
        //    {
        //        ItemsSource = TimeSortedCollectionViewA;
        //    }

        //    if(_selectedSort == "SubmissionTimeD")
        //    {
        //        ItemsSource = TimeSortedCollectionViewD;
        //    }
        //}

        //public ObservableCollection Customers
        //{
        //    get { return _customers; }
        //    set
        //    {
        //        if(_customers != value)
        //        {
        //            _customers = value;
        //            OnPropertyChanged("Customers");
        //        }
        //    }
        //}

        //public void SelectedSort
        //{
        //    //get { return _selectedSort; }
        //    set
        //    {
        //        if(_selectedSort == "SubmitterNameA")
        //        {
        //            ItemsSource = AlphaSortedCollectionViewA;
        //        }
 
        //        if(_selectedSort == "SubmitterNameD")
        //        {
        //            ItemsSource = AlphaSortedCollectionViewD;
        //        }

        //        if(_selectedSort == "SubmissionTimeA")
        //        {
        //            ItemsSource = TimeSortedCollectionViewA;
        //        }
 
        //        if(_selectedSort == "SubmissionTimeD")
        //        {
        //            ItemsSource = TimeSortedCollectionViewD;
        //        }

        //        if(_selectedSort != value)
        //        {
        //            _selectedSort = value;
        //            LastName = value.LastName;
        //            OnPropertyChanged("SelectedCustomer");
        //        }
        //    }
        //}

        //public Customer LastName
        //{
        //    get { return _lastName; }
        //    set
        //    {
        //        if(_lastName != value)
        //        {
        //            _lastName = value;
        //            OnPropertyChanged("LastName");
        //        }
        //    }
        //}





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