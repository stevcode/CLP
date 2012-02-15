using System.Collections.ObjectModel;
using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using Catel.Data;
using System.Collections.Generic;
using System;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(MainWindowViewModel))]
    public class SideBarViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookSideBarViewModel class.
        /// </summary>
        public SideBarViewModel(CLPNotebook notebook)
            : base()
        {
            Notebook = notebook;
            Console.WriteLine(Title + " created");
        }

        public override string Title { get { return "SideBarVM"; } }

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
        [ViewModelToModel("Notebook")]
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        /// <summary>
        /// Register the Pages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //public ObservableCollection<CLPPageViewModel> Pages
        //{
        //    get { return GetValue<ObservableCollection<CLPPageViewModel>>(PagesProperty); }
        //    private set { SetValue(PagesProperty, value); }
        //}

        ///// <summary>
        ///// Register the Pages property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPageViewModel>));

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //public Dictionary<string, ObservableCollection<CLPPageViewModel>> Submissions
        //{
        //    get { return GetValue<Dictionary<string, ObservableCollection<CLPPageViewModel>>>(SubmissionsProperty); }
        //    set { SetValue(SubmissionsProperty, value); }
        //}

        ///// <summary>
        ///// Register the Submissions property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(Dictionary<string, ObservableCollection<CLPPageViewModel>>));

        #endregion //Model

        #region Bindings

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage SelectedNotebookPage
        {
            get { return GetValue<CLPPage>(SelectedNotebookPageProperty); }
            set
            {
                SetValue(SelectedNotebookPageProperty, value);
                CurrentPage = SelectedNotebookPage;
            }
        }

        /// <summary>
        /// Register the SelectedNotebookPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedNotebookPageProperty = RegisterProperty("SelectedNotebookPage", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage SelectedSubmissionPage
        {
            get { return GetValue<CLPPage>(SelectedSubmissionPageProperty); }
            set
            {
                SetValue(SelectedSubmissionPageProperty, value);
                CurrentPage = SelectedSubmissionPage;
            }
        }

        /// <summary>
        /// Register the SelectedSubmissionPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedSubmissionPageProperty = RegisterProperty("SelectedSubmissionPage", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        /// <summary>
        /// Register the CurrentPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Bindings

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "CurrentNotebookIndex")
            {
                int index = (viewModel as MainWindowViewModel).CurrentNotebookIndex;
                Notebook = App.MainWindowViewModel.OpenNotebooks[index];
                //NotebookPages.Clear();
                //foreach (var page in Notebook.Pages)
                //{
                //    NotebookPages.Add(new CLPPageViewModel(page));
                //}
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);

        }
    }
}