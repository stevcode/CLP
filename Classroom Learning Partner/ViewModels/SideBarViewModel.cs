using System.Collections.ObjectModel;
using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using Catel.Data;
using System.Collections.Generic;
using System;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SideBarViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookSideBarViewModel class.
        /// </summary>
        public SideBarViewModel(ObservableCollection<CLPPageViewModel> notebookPages)
            : base()
        {
            Pages = notebookPages;
            Console.WriteLine(Title + " created");
        }

        public override string Title { get { return "SideBarVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPageViewModel> Pages
        {
            get { return GetValue<ObservableCollection<CLPPageViewModel>>(PagesProperty); }
            private set { SetValue(PagesProperty, value); }
        }

        /// <summary>
        /// Register the Pages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPageViewModel>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Dictionary<string, ObservableCollection<CLPPageViewModel>> Submissions
        {
            get { return GetValue<Dictionary<string, ObservableCollection<CLPPageViewModel>>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the Submissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(Dictionary<string, ObservableCollection<CLPPageViewModel>>));

        #endregion //Model

        #region Bindings

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
        public CLPPageViewModel SelectedNotebookPage
        {
            get { return GetValue<CLPPageViewModel>(SelectedNotebookPageProperty); }
            set
            {
                if (SelectedNotebookPage != null)
                {
                	SelectedNotebookPage.SaveViewModel();
                }
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
        public CLPPageViewModel SelectedSubmissionPage
        {
            get { return GetValue<CLPPageViewModel>(SelectedSubmissionPageProperty); }
            set
            {
                SetValue(SelectedSubmissionPageProperty, value);
                CurrentPage = SelectedSubmissionPage;
            }
        }

        /// <summary>
        /// Register the SelectedSubmissionPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedSubmissionPageProperty = RegisterProperty("SelectedSubmissionPage", typeof(CLPPageViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPageViewModel CurrentPage
        {
            get { return GetValue<CLPPageViewModel>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        /// <summary>
        /// Register the CurrentPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPageViewModel));

        #endregion //Bindings
    }
}