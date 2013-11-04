using System;
using System.Windows.Controls;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(CLPPageViewModel))]
    [InterestedIn(typeof(CLPAnimationPageViewModel))]
    public class MirrorDisplayViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MirrorDisplayViewModel class.
        /// </summary>
        public MirrorDisplayViewModel(CLPMirrorDisplay mirrorDisplay)
        {
            MirrorDisplay = mirrorDisplay;
            PageScrollCommand = new Command<ScrollChangedEventArgs>(OnPageScrollCommandExecute);
        }

        public override string Title { get { return "MirrorDisplayVM"; } }

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPMirrorDisplay MirrorDisplay
        {
            get { return GetValue<CLPMirrorDisplay>(MirrorDisplayProperty); }
            set { SetValue(MirrorDisplayProperty, value); }
        }

        public static readonly PropertyData MirrorDisplayProperty = RegisterProperty("MirrorDisplay", typeof(CLPMirrorDisplay));

        /// <summary>
        /// A property mapped to a property on the Model MirrorDisplay.
        /// </summary>
        [ViewModelToModel("MirrorDisplay")]
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage), null, OnCurrentPageChanged);

        private static void OnCurrentPageChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var displayPanel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            var mirrorDisplayViewModel = sender as MirrorDisplayViewModel;
            if(mirrorDisplayViewModel == null || displayPanel == null)
            {
                return;
            }

            mirrorDisplayViewModel.OnPageResize();

            if(!App.MainWindowViewModel.Ribbon.IsProjectorOn || App.Network.ProjectorProxy == null)
            {
                return;
            }

            var pageID = mirrorDisplayViewModel.CurrentPage.SubmissionType != SubmissionType.None ? mirrorDisplayViewModel.CurrentPage.SubmissionID : mirrorDisplayViewModel.CurrentPage.UniqueID;

            try
            {
                App.Network.ProjectorProxy.AddPageToDisplay(pageID);
            }
            catch(Exception)
            {

            }
        }

        #endregion //Model

        #region Page Resizing Bindings

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
                    OnPageResize();
                }
            }
        }

        public static readonly PropertyData DisplayWidthHeightProperty = RegisterProperty("DisplayWidthHeight", typeof(Tuple<double, double>), new Tuple<double, double>(0.0, 0.0));

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
        /// Physical Width of Page. Differs from the PageWidth because PageWidth is inside a ViewBox.
        /// </summary>
        public double DimensionWidth
        {
            get { return GetValue<double>(DimensionWidthProperty); }
            set { SetValue(DimensionWidthProperty, value); }
        }

        public static readonly PropertyData DimensionWidthProperty = RegisterProperty("DimensionWidth", typeof(double));

        /// <summary>
        /// Physical Height of Page. Differs from the PageHeight because PageHeight is inside a ViewBox.
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
            if(App.CurrentUserMode != App.UserMode.Instructor || App.Network.ProjectorProxy == null || !App.MainWindowViewModel.Ribbon.IsProjectorOn)
            {
                return;
            }

            var submissionID = CurrentPage.SubmissionType != SubmissionType.None ? CurrentPage.SubmissionID : "";
            try
            {
                //TODO: Steve - Make the offset a percentage and convert back on receive. If
                //Instructor and Projector are on different screen sizes, they don't have the
                //same vertical offsets.
                App.Network.ProjectorProxy.ScrollPage(CurrentPage.UniqueID, submissionID, e.VerticalOffset);
            }
            catch(Exception)
            {

            }
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "PageHeight")
            {
                var pageViewModel = viewModel as ACLPPageBaseViewModel;
                if(pageViewModel != null && pageViewModel.Page.UniqueID == CurrentPage.UniqueID)
                {
                    OnPageResize();
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void OnPageResize()
        {
            var pageAspectRatio = CurrentPage.InitialPageAspectRatio;
            var pageHeight = CurrentPage.PageHeight;
            var pageWidth = CurrentPage.PageWidth;
            var scrolledAspectRatio = pageWidth / pageHeight;
            const double PAGE_MARGIN = 10.0;

            var borderWidth = DisplayWidthHeight.Item1 - PAGE_MARGIN - 20;
            var borderHeight = borderWidth / pageAspectRatio;

            if(borderHeight > DisplayWidthHeight.Item2 - PAGE_MARGIN)
            {
                borderHeight = DisplayWidthHeight.Item2 - PAGE_MARGIN;
                borderWidth = borderHeight * pageAspectRatio;
            }

            BorderHeight = borderHeight;
            BorderWidth = borderWidth;

            DimensionWidth = BorderWidth - 2 * BorderThickness;
            DimensionHeight = DimensionWidth / scrolledAspectRatio;
        }

        #endregion //Methods
    }
}
