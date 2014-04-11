﻿using System;
using System.Windows.Controls;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(CLPPageViewModel))]
    [InterestedIn(typeof(CLPAnimationPageViewModel))]
    public class SingleDisplayViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SingleDisplayViewModel class.
        /// </summary>
        public SingleDisplayViewModel(Notebook notebook)
        {
            Notebook = notebook;
            PageScrollCommand = new Command<ScrollChangedEventArgs>(OnPageScrollCommandExecute);
        }

        public override string Title
        {
            get { return "SingleDisplayVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The Model of the ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// A property mapped to a property on the Model SingleDisplay.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage), null, OnCurrentPageChanged);

        private static void OnCurrentPageChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var displayPanel = DisplaysPanelViewModel.GetDisplayListPanelViewModel();
            var singleDisplayViewModel = sender as SingleDisplayViewModel;
            if(singleDisplayViewModel == null ||
               displayPanel == null ||
               singleDisplayViewModel.CurrentPage == null)
            {
                return;
            }

            singleDisplayViewModel.OnPageResize();

            // TODO: Entities
            //if(!App.MainWindowViewModel.Ribbon.IsProjectorOn || App.Network.ProjectorProxy == null)
            //{
            //    return;
            //}

            //var pageID = singleDisplayViewModel.CurrentPage.SubmissionType != SubmissionType.None ? singleDisplayViewModel.CurrentPage.SubmissionID : singleDisplayViewModel.CurrentPage.UniqueID;

            //try
            //{
            //    App.Network.ProjectorProxy.AddPageToDisplay(pageID);
            //}
            //catch(Exception)
            //{

            //}
        }

        #endregion //Model

        #region Page Resizing Bindings

        /// <summary>
        /// Tuple that stores the ActualWidth and ActualHeight, repsectively, of the entire SingleDisplay.
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
                    OnPageResize();
                }
            }
        }

        public static readonly PropertyData DisplayWidthHeightProperty = RegisterProperty("DisplayWidthHeight", typeof(Tuple<double, double>), new Tuple<double, double>(0.0, 0.0));

        /// <summary>
        /// Height of the toolbar for the current page.
        /// </summary>
        public double ToolBarHeight
        {
            get { return GetValue<double>(ToolBarHeightProperty); }
            set { SetValue(ToolBarHeightProperty, value); }
        }

        public static readonly PropertyData ToolBarHeightProperty = RegisterProperty("ToolBarHeight", typeof(double), 0.0);

        /// <summary>
        /// Thickness of the border around the page.
        /// </summary>
        public double BorderThickness
        {
            get { return GetValue<double>(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly PropertyData BorderThicknessProperty = RegisterProperty("BorderThickness", typeof(double), 1.0);

        /// <summary>
        /// Width of the visible border around a page.
        /// Scales based on zoom leve.
        /// </summary>
        public double BorderWidth
        {
            get { return GetValue<double>(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        public static readonly PropertyData BorderWidthProperty = RegisterProperty("BorderWidth", typeof(double));

        /// <summary>
        /// Height of the visible border around a page.
        /// Scales based on zoom leve.
        /// </summary>
        public double BorderHeight
        {
            get { return GetValue<double>(BorderHeightProperty); }
            set { SetValue(BorderHeightProperty, value); }
        }

        public static readonly PropertyData BorderHeightProperty = RegisterProperty("BorderHeight", typeof(double));

        /// <summary>
        /// Physical Width of Page. Differs from the Width because Width is inside a ViewBox.
        /// </summary>
        public double DimensionWidth
        {
            get { return GetValue<double>(DimensionWidthProperty); }
            set { SetValue(DimensionWidthProperty, value); }
        }

        public static readonly PropertyData DimensionWidthProperty = RegisterProperty("DimensionWidth", typeof(double));

        /// <summary>
        /// Physical Height of Page. Differs from the Height because Height is inside a ViewBox.
        /// </summary>
        public double DimensionHeight
        {
            get { return GetValue<double>(DimensionHeightProperty); }
            set { SetValue(DimensionHeightProperty, value); }
        }

        public static readonly PropertyData DimensionHeightProperty = RegisterProperty("DimensionHeight", typeof(double));

        #endregion //Page Resizing Bindings

        #region Commands

        /// <summary>
        /// Forwards PageScroll events to the Projector.
        /// </summary>
        public Command<ScrollChangedEventArgs> PageScrollCommand { get; private set; }

        private void OnPageScrollCommandExecute(ScrollChangedEventArgs e)
        {
            // TODO: Entities
            //if(App.CurrentUserMode != App.UserMode.Instructor || App.Network.ProjectorProxy == null || !App.MainWindowViewModel.Ribbon.IsProjectorOn)
            //{
            //    return;
            //}

            //var submissionID = CurrentPage.SubmissionType != SubmissionType.None ? CurrentPage.SubmissionID : "";
            //try
            //{
            //    //TODO: Steve - Make the offset a percentage and convert back on receive. If
            //    //Instructor and Projector are on different screen sizes, they don't have the
            //    //same vertical offsets.
            //    App.Network.ProjectorProxy.ScrollPage(CurrentPage.UniqueID, submissionID, e.VerticalOffset);
            //}
            //catch(Exception)
            //{

            //}
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "Height")
            {
                var pageViewModel = viewModel as ACLPPageBaseViewModel;
                if(pageViewModel != null &&
                   pageViewModel.Page.ID == CurrentPage.ID)
                {
                    OnPageResize();
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void OnPageResize()
        {
            var pageAspectRatio = CurrentPage.InitialAspectRatio;
            var pageHeight = CurrentPage.Height;
            var pageWidth = CurrentPage.Width;
            var scrolledAspectRatio = pageWidth / pageHeight;
            const double PAGE_MARGIN = 10.0;

            var borderWidth = DisplayWidthHeight.Item1 - PAGE_MARGIN - 20;
            var borderHeight = borderWidth / pageAspectRatio;

            if(borderHeight > DisplayWidthHeight.Item2 - PAGE_MARGIN - ToolBarHeight)
            {
                borderHeight = DisplayWidthHeight.Item2 - PAGE_MARGIN - ToolBarHeight;
                borderWidth = borderHeight * pageAspectRatio;
            }

            BorderHeight = Math.Max(0.0, borderHeight);
            BorderWidth = Math.Max(0.0, borderWidth);

            DimensionWidth = Math.Max(0.0, BorderWidth - 2 * BorderThickness);
            DimensionHeight = Math.Max(0.0, DimensionWidth / scrolledAspectRatio);
        }

        #endregion //Methods
    }
}