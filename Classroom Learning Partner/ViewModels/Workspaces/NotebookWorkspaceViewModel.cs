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

            Console.WriteLine(Title + " created");
            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            Notebook = notebook;
            SubmissionPages = new ObservableCollection<CLPPageViewModel>();

            NotebookPages = new ObservableCollection<CLPPageViewModel>();
            foreach (var page in Notebook.Pages)
            {
                NotebookPages.Add(new CLPPageViewModel(page));
            }

            

            LinkedDisplay = new LinkedDisplayViewModel(NotebookPages[0]);

            SelectedDisplay = LinkedDisplay;
            SelectedDisplay.IsActive = true;
            //SelectedNotebookPage = NotebookPages[0];

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                SelectedDisplay.IsOnProjector = true;
            }
            else
            {
                SelectedDisplay.IsOnProjector = false;
            }
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

        #endregion //Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPageViewModel> NotebookPages
        {
            get { return GetValue<ObservableCollection<CLPPageViewModel>>(NotebookPagesProperty); }
            set { SetValue(NotebookPagesProperty, value); }
        }

        /// <summary>
        /// Register the NotebookPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookPagesProperty = RegisterProperty("NotebookPages", typeof(ObservableCollection<CLPPageViewModel>));

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
            set { SetValue(SelectedDisplayProperty, value); }
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
        public CLPPageViewModel SelectedNotebookPage
        {
            get { return GetValue<CLPPageViewModel>(SelectedNotebookPageProperty); }
            set
            {
                SetValue(SelectedNotebookPageProperty, value);
                CurrentPage = SelectedNotebookPage;
            }
        }

        /// <summary>
        /// Register the SelectedNotebookPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedNotebookPageProperty = RegisterProperty("SelectedNotebookPage", typeof(CLPPageViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPageViewModel> SubmissionPages
        {
            get { return GetValue<ObservableCollection<CLPPageViewModel>>(SubmissionPagesProperty); }
            set { SetValue(SubmissionPagesProperty, value); }
        }

        /// <summary>
        /// Register the SubmissionPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPageViewModel>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPageViewModel CurrentPage
        {
            get { return GetValue<CLPPageViewModel>(CurrentPageProperty); }
            set
            {
                SetValue(CurrentPageProperty, value);
                SelectedDisplay.AddPageToDisplay(value);
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
            CurrentPage = (e.Source as CLPPagePreviewView).DataContext as CLPPageViewModel;
        }

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsAuthoring")
            {
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
