using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;
using System.Windows.Media;
using System;
using Classroom_Learning_Partner.Model;
using System.Windows;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    [InterestedIn(typeof(SideBarViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(CLPNotebook notebook)
            : base()
        {
            Notebook = notebook;
            Console.WriteLine(Title + " created");
            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            LinkedDisplay = new LinkedDisplayViewModel(Notebook.Pages[0]);
            SelectedDisplay = LinkedDisplay;
            SelectedDisplay.IsActive = true;

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
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
        public LinkedDisplayViewModel LinkedDisplay
        {
            get { return GetValue<LinkedDisplayViewModel>(LinkedDisplayProperty); }
            set { SetValue(LinkedDisplayProperty, value); }
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
                    if (App.CurrentUserMode == App.UserMode.Instructor)
                    {
                        SelectedDisplay.IsOnProjector = true;
                    }
                }
            }

            if (propertyName == "CurrentPage")
            {
                SelectedDisplay.AddPageToDisplay((viewModel as SideBarViewModel).CurrentPage);
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
            
        }
    }
}
