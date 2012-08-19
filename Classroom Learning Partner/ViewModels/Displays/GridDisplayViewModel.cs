using System;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GridDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel()
            : base()
        {
            DisplayedPages = new ObservableCollection<CLPPageViewModel>();
            DisplayedPages.CollectionChanged += DisplayedPages_CollectionChanged;
            DisplayID = Guid.NewGuid().ToString();
            IsOnProjector = false;
            UGridRows = 1;

            RemovePageFromGridDisplayCommand = new Command<CLPPageViewModel>(OnRemovePageFromGridDisplayCommandExecute);
        }

        void DisplayedPages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DisplayedPages.Count < 3)
            {
                UGridRows = 1;
            }
            else
            {
                UGridRows = 0;
            }
        }

        public override string Title { get { return "GridDisplayVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string DisplayID
        {
            get { return GetValue<string>(DisplayIDProperty); }
            set { SetValue(DisplayIDProperty, value); }
        }

        /// <summary>
        /// Register the DisplayID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayIDProperty = RegisterProperty("DisplayID", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPageViewModel> DisplayedPages
        {
            get { return GetValue<ObservableCollection<CLPPageViewModel>>(DisplayedPagesProperty); }
            set { SetValue(DisplayedPagesProperty, value); }
        }

        /// <summary>
        /// Register the DisplayedPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPagesProperty = RegisterProperty("DisplayedPages", typeof(ObservableCollection<CLPPageViewModel>));

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand command.
        /// </summary>
        public Command<CLPPageViewModel> RemovePageFromGridDisplayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RemovePageFromGridDisplayCommand command is executed.
        /// </summary>
        public void OnRemovePageFromGridDisplayCommandExecute(CLPPageViewModel page)
        {
            DisplayedPages.Remove(page);
        }

        public string DisplayName
        {
            get { return "GridDisplay"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsOnProjector
        {
            get { return GetValue<bool>(IsOnProjectorProperty); }
            set { SetValue(IsOnProjectorProperty, value); }
        }

        /// <summary>
        /// Register the IsOnProjector property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsOnProjectorProperty = RegisterProperty("IsOnProjector", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int UGridRows
        {
            get { return GetValue<int>(UGridRowsProperty); }
            set { SetValue(UGridRowsProperty, value); }
        }

        /// <summary>
        /// Register the UGridRows property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UGridRowsProperty = RegisterProperty("UGridRows", typeof(int), null);

        public void AddPageToDisplay(CLPPageViewModel page)
        {
            DisplayedPages.Add(page);
        }
    }
}