using System;
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
        public LinkedDisplayViewModel(CLPPageViewModel page)
            : base()
        {
            DisplayedPage = page;
            DisplayID = Guid.NewGuid().ToString();
            IsOnProjector = false;
        }

        public override string Title { get { return "LinkDisplayVM"; } }

        #region Bindings

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
            set 
            { 
                SetValue(DisplayedPageProperty, value);
                ResizePage();
            }
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

        #region Page Resizing

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Tuple<double, double> DisplayWidthHeight
        {
            get { return GetValue<Tuple<double, double>>(DisplayWidthHeightProperty); }
            set 
            { 
                SetValue(DisplayWidthHeightProperty, value);
                ResizePage();
            }
        }

        /// <summary>
        /// Register the DisplayWidthHeight property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayWidthHeightProperty = RegisterProperty("DisplayWidthHeight", typeof(Tuple<double, double>), new Tuple<double, double>(0.0, 0.0));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double BorderWidth
        {
            get { return GetValue<double>(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        /// <summary>
        /// Register the BorderWidth property so it is known in the class.
        /// </summary>
        public static readonly PropertyData BorderWidthProperty = RegisterProperty("BorderWidth", typeof(double), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double BorderHeight
        {
            get { return GetValue<double>(BorderHeightProperty); }
            set { SetValue(BorderHeightProperty, value); }
        }

        /// <summary>
        /// Register the BorderHeight property so it is known in the class.
        /// </summary>
        public static readonly PropertyData BorderHeightProperty = RegisterProperty("BorderHeight", typeof(double), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double DimensionWidth
        {
            get { return GetValue<double>(DimensionWidthProperty); }
            set { SetValue(DimensionWidthProperty, value); }
        }

        /// <summary>
        /// Register the DimensionWidth property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DimensionWidthProperty = RegisterProperty("DimensionWidth", typeof(double), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double DimensionHeight
        {
            get { return GetValue<double>(DimensionHeightProperty); }
            set { SetValue(DimensionHeightProperty, value); }
        }

        /// <summary>
        /// Register the DimensionHeight property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DimensionHeightProperty = RegisterProperty("DimensionHeight", typeof(double), null);

        #endregion //Page Resizing

        #endregion //Bindings

        public void ResizePage()
        {
            double pageAspectRatio = DisplayedPage.PageAspectRatio;
            double pageHeight = DisplayedPage.PageHeight;
            double pageWidth = DisplayedPage.PageWidth;
            double scrolledAspectRatio = pageWidth / pageHeight;

            double borderWidth = DisplayWidthHeight.Item1 - 20;
            double borderHeight = borderWidth / pageAspectRatio;

            if(borderHeight > DisplayWidthHeight.Item2 - 20)
            {
                borderHeight = DisplayWidthHeight.Item2 - 20;
                borderWidth = borderHeight * pageAspectRatio;
            }

            BorderHeight = borderHeight;
            BorderWidth = borderWidth;

            DimensionWidth = BorderWidth - 2;
            DimensionHeight = DimensionWidth / scrolledAspectRatio;
        }


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
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(DisplayedPage.Page, pageObject);
        }

    }
}