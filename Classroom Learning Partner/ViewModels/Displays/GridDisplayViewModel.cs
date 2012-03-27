using Catel.MVVM;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using Catel.Data;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System;

namespace Classroom_Learning_Partner.ViewModels.Displays
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
            DisplayedPages.Add(new CLPPageViewModel(new CLPPage()));
            DisplayedPages.Add(new CLPPageViewModel(new CLPPage()));
            DisplayID = Guid.NewGuid().ToString();
            IsOnProjector = false;

            RemovePageFromGridDisplayCommand = new Command<CLPPageViewModel>(OnRemovePageFromGridDisplayCommandExecute);
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
        private void OnRemovePageFromGridDisplayCommandExecute(CLPPageViewModel page)
        {
            DisplayedPages.Remove(page);

            //if (App.CurrentUserMode == App.UserMode.Instructor)
            //{
            //    if (App.Peer.Channel != null)
            //    {
            //        if (this.IsOnProjector)
            //        {
            //            string pageString = ObjectSerializer.ToString(pageViewModel.Page);
            //            App.Peer.Channel.RemovePageFromGridDisplay(pageString);
            //        }
            //    }
            //}
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


        public void AddPageToDisplay(CLPPageViewModel page)
        {
            DisplayedPages.Add(page);
        }
    }
}