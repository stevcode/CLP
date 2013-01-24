using System;
using System.Linq;
using System.ServiceModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class LinkedDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel(CLPPage page)
            : base()
        {
            DisplayedPage = page;
        }

        public override string Title { get { return "LinkDisplayVM"; } }

        #region Bindings

        #region Interface

        public string DisplayName
        {
            get { return "LinkedDisplay"; }
        }

        /// <summary>
        /// Unique ID of Display. Not really applicable for MirrorDisplay as a notebook should only need 1 MirrorDisplay.
        /// </summary>
        public string DisplayID
        {
            get { return GetValue<string>(DisplayIDProperty); }
            set { SetValue(DisplayIDProperty, value); }
        }

        public static readonly PropertyData DisplayIDProperty = RegisterProperty("DisplayID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// If Display is currently being projected.
        /// </summary>
        public bool IsOnProjector
        {
            get { return GetValue<bool>(IsOnProjectorProperty); }
            set { SetValue(IsOnProjectorProperty, value); }
        }

        public static readonly PropertyData IsOnProjectorProperty = RegisterProperty("IsOnProjector", typeof(bool), false);

        #endregion //Interface

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage DisplayedPage
        {
            get { return GetValue<CLPPage>(DisplayedPageProperty); }
            set 
            { 
                SetValue(DisplayedPageProperty, value);
                ResizePage();
            }
        }

        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(CLPPage));

        #region Page Resizing

        /// <summary>
        /// Tuple that stores the ActualWidth and ActualHeight, repsectively, of the entire MirrorDisplay.
        /// DataBinding done from Dependency Property in the View.
        /// </summary>
        public Tuple<double, double> DisplayWidthHeight
        {
            get { return GetValue<Tuple<double, double>>(DisplayWidthHeightProperty); }
            set 
            {
                SetValue(DisplayWidthHeightProperty, value);

                if(value != null)
                {
                    ResizePage();
                }
            }
        }

        public static readonly PropertyData DisplayWidthHeightProperty = RegisterProperty("DisplayWidthHeight", typeof(Tuple<double, double>), new Tuple<double, double>(0.0, 0.0));

        /// <summary>
        /// Width of the visible border around a page.
        /// Scales based on zoom leve.
        /// </summary>
        public double BorderWidth
        {
            get { return GetValue<double>(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        public static readonly PropertyData BorderWidthProperty = RegisterProperty("BorderWidth", typeof(double), null);

        /// <summary>
        /// Height of the visible border around a page.
        /// Scales based on zoom leve.
        /// </summary>
        public double BorderHeight
        {
            get { return GetValue<double>(BorderHeightProperty); }
            set { SetValue(BorderHeightProperty, value); }
        }

        public static readonly PropertyData BorderHeightProperty = RegisterProperty("BorderHeight", typeof(double), null);

        /// <summary>
        /// Physical Width of Page
        /// </summary>
        public double DimensionWidth
        {
            get { return GetValue<double>(DimensionWidthProperty); }
            set { SetValue(DimensionWidthProperty, value); }
        }

        public static readonly PropertyData DimensionWidthProperty = RegisterProperty("DimensionWidth", typeof(double), null);

        /// <summary>
        /// Physical Height of Page
        /// </summary>
        public double DimensionHeight
        {
            get { return GetValue<double>(DimensionHeightProperty); }
            set { SetValue(DimensionHeightProperty, value); }
        }

        public static readonly PropertyData DimensionHeightProperty = RegisterProperty("DimensionHeight", typeof(double), null);

        public bool ZoomToWholePage = true;

        #endregion //Page Resizing

        #endregion //Bindings

        #region Methods

        public void ResizePage()
        {
            double pageAspectRatio = DisplayedPage.PageAspectRatio;
            double pageHeight = DisplayedPage.PageHeight;
            double pageWidth = DisplayedPage.PageWidth;
            double scrolledAspectRatio = pageWidth / pageHeight;

            double borderWidth = DisplayWidthHeight.Item1 - 20;
            double borderHeight = borderWidth / pageAspectRatio;

            if(ZoomToWholePage)
            {
                if(borderHeight > DisplayWidthHeight.Item2 - 20)
                {
                    borderHeight = DisplayWidthHeight.Item2 - 20;
                    borderWidth = borderHeight * pageAspectRatio;
                }
            }

            BorderHeight = borderHeight;
            BorderWidth = borderWidth;

            DimensionWidth = BorderWidth - 2;
            DimensionHeight = DimensionWidth / scrolledAspectRatio;
        }

        //From Interface IDisplayViewModel
        public void AddPageToDisplay(CLPPage page)
        {
            DisplayedPage = page;
            if (IsOnProjector)
            {
                string pageID;
                if(DisplayedPage.IsSubmission)
                {
                    pageID = DisplayedPage.SubmissionID;
                }
                else
                {
                    pageID = DisplayedPage.UniqueID;
                }

                if(App.Network.DiscoveredProjectors.Addresses.Count() > 0)
                {
                    IProjectorContract ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(new NetTcpBinding(), App.Network.DiscoveredProjectors.Addresses[0]);
                    ProjectorProxy.AddPageToDisplay(pageID);
                    //TODO: Steve - add try/catch around closing in case projector closes in between the 20 seconds DiscoveredProjectors refreshes
                    (ProjectorProxy as ICommunicationObject).Close();
                }
                else
                {
                    Console.WriteLine("Address NOT Available");
                }
            }
        }

        public void AddPageObjectToCurrentPage(ICLPPageObject pageObject)
        {
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(DisplayedPage, pageObject);
        }

        #endregion //Methods

    }
}