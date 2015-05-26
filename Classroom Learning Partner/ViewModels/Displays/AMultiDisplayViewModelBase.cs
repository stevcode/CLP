using System.Collections.ObjectModel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class AMultiDisplayViewModelBase : ViewModelBase
    {
        protected INotebookService _notebookService;

        #region Constructor

        protected AMultiDisplayViewModelBase(IDisplay display)
        {
            _notebookService = DependencyResolver.Resolve<INotebookService>();

            MultiDisplay = display;

            SetPageAsCurrentPageCommand = new Command<CLPPage>(OnSetPageAsCurrentPageCommandExecute);
            RemovePageFromMultiDisplayCommand = new Command<CLPPage>(OnRemovePageFromMultiDisplayCommandExecute);
        }

        #endregion //Constructor

        #region Model

        /// <summary>The Model for this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public IDisplay MultiDisplay
        {
            get { return GetValue<IDisplay>(MultiDisplayProperty); }
            private set { SetValue(MultiDisplayProperty, value); }
        }

        public static readonly PropertyData MultiDisplayProperty = RegisterProperty("MultiDisplay", typeof (IDisplay));

        /// <summary>Index of the Display in the notebook.</summary>
        [ViewModelToModel("MultiDisplay")]
        public int DisplayNumber
        {
            get { return GetValue<int>(DisplayNumberProperty); }
            set { SetValue(DisplayNumberProperty, value); }
        }

        public static readonly PropertyData DisplayNumberProperty = RegisterProperty("DisplayNumber", typeof (int));

        /// <summary>Pages displayed by the MultiDisplay.</summary>
        [ViewModelToModel("MultiDisplay")]
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof (ObservableCollection<CLPPage>));

        #endregion //Model

        #region Properties

        /// <summary>Toggle to ignore viewModels of Display Previews</summary>
        public bool IsDisplayPreview
        {
            get { return GetValue<bool>(IsDisplayPreviewProperty); }
            set { SetValue(IsDisplayPreviewProperty, value); }
        }

        public static readonly PropertyData IsDisplayPreviewProperty = RegisterProperty("IsDisplayPreview", typeof (bool), false);

        #endregion //Properties

        #region Commands

        /// <summary>Sets the specific page as the notebook's CurrentPage</summary>
        public Command<CLPPage> SetPageAsCurrentPageCommand { get; private set; }

        private void OnSetPageAsCurrentPageCommandExecute(CLPPage page)
        {
            if (_notebookService == null ||
                page == null)
            {
                return;
            }

            _notebookService.CurrentNotebook.CurrentPage = page;
        }

        /// <summary>Removes a specific page from the MultiDisplay.</summary>
        public Command<CLPPage> RemovePageFromMultiDisplayCommand { get; private set; }

        public void OnRemovePageFromMultiDisplayCommandExecute(CLPPage page) { MultiDisplay.RemovePageFromDisplay(page); }

        #endregion //Commands
    }
}