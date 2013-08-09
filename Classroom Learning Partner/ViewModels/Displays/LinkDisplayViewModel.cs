﻿using System;
using System.Windows.Controls;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(CLPPageViewModel))]
    public class LinkedDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel(ICLPPage page)
        {
            DisplayedPage = page;
            PageScrollCommand = new Command<ScrollChangedEventArgs>(OnPageScrollCommandExecute);
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
        public ICLPPage DisplayedPage
        {
            get { return GetValue<ICLPPage>(DisplayedPageProperty); }
            set 
            { 
                SetValue(DisplayedPageProperty, value);
                ResizePage();
                SetPageBorderColor();
            }
        }

        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(ICLPPage));

        /// <summary>
        /// Specifies Border Color of main Page
        /// </summary>
        public String PageBorderColor
        {
            get { return GetValue<String>(PageBorderColorProperty); }
            set { SetValue(PageBorderColorProperty, value); }
        }

        public static readonly PropertyData PageBorderColorProperty = RegisterProperty("PageBorderColor", typeof(String));

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

        #region Commands

        /// <summary>
        /// Gets the PageScrollCommand command.
        /// </summary>
        public Command<ScrollChangedEventArgs> PageScrollCommand { get; private set; }

        private void OnPageScrollCommandExecute(ScrollChangedEventArgs e)
        {
            if(App.CurrentUserMode == App.UserMode.Instructor)
            {
                string submissionID = "";
                if(DisplayedPage.IsSubmission)
                {
                    submissionID = DisplayedPage.SubmissionID;
                }

                if(App.Network.ProjectorProxy != null)
                {
                    try
                    {
                        //TODO: Steve - Make the offset a percentage and convert back on receive. If
                        //Instructor and Projector are on different screen sizes, they don't have the
                        //same vertical offsets.
                        App.Network.ProjectorProxy.ScrollPage(DisplayedPage.UniqueID, submissionID, e.VerticalOffset);
                    }
                    catch(System.Exception)
                    {

                    }
                }
                else
                {
                    Console.WriteLine("Projector NOT Available");
                }
            }
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "PageHeight")
            {
                var pageViewModel = (viewModel as CLPPageViewModel);
                if (pageViewModel.Page.UniqueID == DisplayedPage.UniqueID)
                {
                    if(pageViewModel.Page.IsSubmission)
                    {
                        if(pageViewModel.Page.SubmissionID == DisplayedPage.SubmissionID)
                        {
                            ResizePage();
                        }
                    }
                    ResizePage();
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);

        }

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
        public void AddPageToDisplay(ICLPPage page)
        {
            if(DisplayedPage is CLPAnimationPage && page.UniqueID != DisplayedPage.UniqueID)
            {
                var clpProofHistory = DisplayedPage.PageHistory as CLPProofHistory;
                if(clpProofHistory != null)
                {
                    clpProofHistory.IsPaused = true;
                    lock(CLPProofPageViewModel.obj)
                    {
                        clpProofHistory.Freeze();
                    }
                }

                SetPageBorderColor();
            }

            DisplayedPage = page;
            if(page is CLPAnimationPage)
            {
                (page as CLPAnimationPage).updateProgress();
            }
            if(IsOnProjector)
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

                if(App.Network.ProjectorProxy != null)
                {
                    try
                    {
                    	App.Network.ProjectorProxy.AddPageToDisplay(pageID);
                    }
                    catch (System.Exception)
                    {
                    	
                    }
                }
                else
                {
                    //TODO: Steve - add pages to a queue and send when a projector is found
                    Console.WriteLine("Projector NOT Available");
                }
            }
        }

        public void AddPageObjectToCurrentPage(ICLPPageObject pageObject)
        {
            CLPServiceAgent.Instance.AddPageObjectToPage(DisplayedPage, pageObject);
        }

        public void SetPageBorderColor()
        {
            if(!DisplayedPage.IsSubmission)
            {
                PageBorderColor = "#2F64B9";
            }
            else
            {
                PageBorderColor = "DarkOrange";
            }
        }

        #endregion //Methods

    }
}