using Classroom_Learning_Partner.Model;
using Catel.MVVM;
using Catel.Data;
using System;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    public class LinkedDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel(CLPPageViewModel page)
            : base()
        {
            DisplayedPage = page;
            DisplayID = Guid.NewGuid().ToString();
            IsOnProjector = false;
        }

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
        public CLPPageViewModel DisplayedPage
        {
            get { return GetValue<CLPPageViewModel>(DisplayedPageProperty); }
            set { SetValue(DisplayedPageProperty, value); }
        }

        /// <summary>
        /// Register the DisplayedPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(CLPPageViewModel));

        public string DisplayName
        {
            get { return "LinkedDisplay"; }
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

        public override string Title { get { return "LinkDisplayVM"; } }


        public void AddPageToDisplay(CLPPageViewModel page)
        {
            DisplayedPage = page;
            if (IsOnProjector && App.Peer.Channel != null)
            {
                if (page.IsSubmission)
                {
                    App.Peer.Channel.AddPageToDisplay(page.Page.SubmissionID);
                }
                else
                {
                    App.Peer.Channel.AddPageToDisplay(page.Page.UniqueID);
                }
            }
        }

        public void AddPageObjectToCurrentPage(ICLPPageObject pageObject)
        {
            CLPServiceAgent.Instance.AddPageObjectToPage(DisplayedPage.Page, pageObject);
        }

    }
}