using Classroom_Learning_Partner.Model;
using Catel.MVVM;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    [InterestedIn(typeof(SideBarViewModel))]
    public class LinkedDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
        public CLPPage DisplayedPage
        {
            get { return GetValue<CLPPage>(DisplayedPageProperty); }
            set { SetValue(DisplayedPageProperty, value); }
        }

        /// <summary>
        /// Register the DisplayedPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(CLPPage));

        public string DisplayName
        {
            get { return "LinkedDisplay"; }
        }

        public bool IsActive { get; set; }
        public bool IsOnProjector { get; set; }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "CurrentPage")
            {
                AddPageToDisplay((viewModel as SideBarViewModel).CurrentPage);
                //Steve - send to projector
                //App.Peer.Channel.AddPageToDisplay? if IsOnProjector
                // if (this.IsActive)
                //{
                //    if (App.CurrentUserMode == App.UserMode.Instructor)
                //    {
                //        if (App.Peer.Channel != null)
                //        {
                //            if (this.IsOnProjector)
                //            {
                //            //run this in background thread?
                //                string pageString = ObjectSerializer.ToString(pageViewModel.Page);
                //                App.Peer.Channel.AddPageToDisplay(pageString);
                //            }
                //        }
                //    }
                //}
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }


        public void AddPageToDisplay(CLPPage page)
        {
            DisplayedPage = page;
        }
    }
}