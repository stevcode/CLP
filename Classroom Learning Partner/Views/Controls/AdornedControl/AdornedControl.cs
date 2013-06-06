using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Catel.Windows;
using Classroom_Learning_Partner.Views;

namespace AdornedControl
{
    /// <summary>
    /// Specifies the placement of the adorner in related to the adorned control.
    /// </summary>
    public enum AdornerPlacement
    {
        Inside,
        Outside
    }

    /// <summary>
    /// A content control that allows an adorner for the content to
    /// be defined in XAML.
    /// </summary>
    public class AdornedControl : ContentControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Specifies the current show/hide state of the adorner.
        /// </summary>
        private enum AdornerShowState
        {
            Visible,
            Hidden,
            FadingIn,
            FadingOut,
        }

        #region Private Data Members

        /// <summary>
        /// Specifies the current show/hide state of the adorner.
        /// </summary>
        private AdornerShowState _adornerShowState = AdornerShowState.Hidden;

        /// <summary>
        /// Caches the adorner layer.
        /// </summary>
        private AdornerLayer _adornerLayer;

        /// <summary>
        /// The actual adorner create to contain our 'adorner UI content'.
        /// </summary>
        private FrameworkElementAdorner _adorner;

        /// <summary>
        /// This timer is used to fade in and open the adorner.
        /// </summary>
        private readonly DispatcherTimer _openAdornerTimer = new DispatcherTimer();

        /// <summary>
        /// This timer is used to fade out and close the adorner.
        /// </summary>
        private readonly DispatcherTimer _closeAdornerTimer = new DispatcherTimer();

        #endregion //Private Data Members

        #region Constructor

        public AdornedControl()
        {
            Focusable = false; // By default don't want 'AdornedControl' to be focusable.

            DataContextChanged += AdornedControl_DataContextChanged;

            _openAdornerTimer.Tick += openAdornerTimer_Tick;
            _openAdornerTimer.Interval = TimeSpan.FromSeconds(OpenAdornerTimeOut);

            _closeAdornerTimer.Tick += closeAdornerTimer_Tick;
            _closeAdornerTimer.Interval = TimeSpan.FromSeconds(CloseAdornerTimeOut);
        }

        /// <summary>
        /// Static constructor to register command bindings.
        /// </summary>
        static AdornedControl()
        {
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), ShowAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), FadeInAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), HideAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornedControl), FadeOutAdornerCommandBinding);  
        }

        #endregion //Constructor

        #region Dependency Properties

        /// <summary>
        /// Shows or hides the adorner.
        /// Set to 'true' to show the adorner or 'false' to hide the adorner.
        /// </summary>
        public bool IsAdornerVisible
        {
            get { return (bool)GetValue(IsAdornerVisibleProperty); }
            set { SetValue(IsAdornerVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsAdornerVisibleProperty =
            DependencyProperty.Register("IsAdornerVisible", typeof(bool), typeof(AdornedControl),
                new FrameworkPropertyMetadata(IsAdornerVisible_PropertyChanged));

        private static void IsAdornerVisible_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl)o;
            c.ShowOrHideAdornerInternal();
        }

        /// <summary>
        /// Used in XAML to define the UI content of the adorner.
        /// </summary>
        public FrameworkElement AdornerContent
        {
            get { return (FrameworkElement)GetValue(AdornerContentProperty); }
            set { SetValue(AdornerContentProperty, value); }
        }

        public static readonly DependencyProperty AdornerContentProperty =
            DependencyProperty.Register("AdornerContent", typeof(FrameworkElement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerContent_PropertyChanged));

        private static void AdornerContent_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl)o;
            c.ShowOrHideAdornerInternal();

            var oldAdornerContent = (FrameworkElement)e.OldValue;
            if(oldAdornerContent != null)
            {
                oldAdornerContent.MouseEnter -= c.adornerContent_MouseEnter;
                oldAdornerContent.MouseLeave -= c.adornerContent_MouseLeave;
            }

            var newAdornerContent = (FrameworkElement)e.NewValue;
            if(newAdornerContent != null)
            {
                newAdornerContent.MouseEnter += c.adornerContent_MouseEnter;
                newAdornerContent.MouseLeave += c.adornerContent_MouseLeave;
            }
        }

        /// <summary>
        /// Specifies the horizontal placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement HorizontalAdornerPlacement
        {
            get { return (AdornerPlacement)GetValue(HorizontalAdornerPlacementProperty); }
            set { SetValue(HorizontalAdornerPlacementProperty, value); }
        }
        
        public static readonly DependencyProperty HorizontalAdornerPlacementProperty =
            DependencyProperty.Register("HorizontalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        /// <summary>
        /// Specifies the vertical placement of the adorner relative to the adorned control.
        /// </summary>
        public AdornerPlacement VerticalAdornerPlacement
        {
            get { return (AdornerPlacement)GetValue(VerticalAdornerPlacementProperty); }
            set { SetValue(VerticalAdornerPlacementProperty, value); }
        }

        public static readonly DependencyProperty VerticalAdornerPlacementProperty =
            DependencyProperty.Register("VerticalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornedControl),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        /// <summary>
        /// X offset of the adorner.
        /// </summary>
        public double AdornerOffsetX
        {
            get { return (double)GetValue(AdornerOffsetXProperty); }
            set { SetValue(AdornerOffsetXProperty, value); }
        }

        public static readonly DependencyProperty AdornerOffsetXProperty =
            DependencyProperty.Register("AdornerOffsetX", typeof(double), typeof(AdornedControl));

        /// <summary>
        /// Y offset of the adorner.
        /// </summary>
        public double AdornerOffsetY
        {
            get { return (double)GetValue(AdornerOffsetYProperty); }
            set { SetValue(AdornerOffsetYProperty, value); }
        }

        public static readonly DependencyProperty AdornerOffsetYProperty =
            DependencyProperty.Register("AdornerOffsetY", typeof(double), typeof(AdornedControl));

        /// <summary>
        /// Set to 'true' to make the adorner automatically fade-in and become visible when the mouse is hovered
        /// over the adorned control.  Also the adorner automatically fades-out when the mouse cursor is moved
        /// away from the adorned control (and the adorner).
        /// </summary>
        public bool IsMouseOverShowEnabled
        {
            get { return (bool)GetValue(IsMouseOverShowEnabledProperty); }
            set { SetValue(IsMouseOverShowEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMouseOverShowEnabledProperty =
            DependencyProperty.Register("IsMouseOverShowEnabled", typeof(bool), typeof(AdornedControl),
                new FrameworkPropertyMetadata(true, IsMouseOverShowEnabled_PropertyChanged));

        private static void IsMouseOverShowEnabled_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl)o;
            c._closeAdornerTimer.Stop();
            c.HideAdorner();
        }

        /// <summary>
        /// Specifies the time (in seconds) it takes to fade in the adorner.
        /// </summary>
        public double FadeInTime
        {
            get { return (double)GetValue(FadeInTimeProperty); }
            set { SetValue(FadeInTimeProperty, value); }
        }
        
        public static readonly DependencyProperty FadeInTimeProperty =
            DependencyProperty.Register("FadeInTime", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(0.25));

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves over the 
        /// adorned control (or the adorner) when the adorner begins to fade in.
        /// </summary>
        public double OpenAdornerTimeOut
        {
            get { return (double)GetValue(OpenAdornerTimeOutProperty); }
            set { SetValue(OpenAdornerTimeOutProperty, value); }
        }

        public static readonly DependencyProperty OpenAdornerTimeOutProperty =
            DependencyProperty.Register("OpenAdornerTimeOut", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(0.0, OpenAdornerTimeOut_PropertyChanged));

        private static void OpenAdornerTimeOut_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl)o;
            c._openAdornerTimer.Stop();
            c._openAdornerTimer.Interval = TimeSpan.FromSeconds(c.OpenAdornerTimeOut);
        }

        /// <summary>
        /// Specifies the time (in seconds) it takes to fade out the adorner.
        /// </summary>
        public double FadeOutTime
        {
            get { return (double)GetValue(FadeOutTimeProperty); }
            set { SetValue(FadeOutTimeProperty, value); }
        }
        
        public static readonly DependencyProperty FadeOutTimeProperty =
            DependencyProperty.Register("FadeOutTime", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(0.5)); //default was originally 1.0, changed to .5, maybe change later?

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves away from the 
        /// adorned control (or the adorner) when the adorner begins to fade out.
        /// </summary>
        public double CloseAdornerTimeOut
        {
            get { return (double)GetValue(CloseAdornerTimeOutProperty); }
            set { SetValue(CloseAdornerTimeOutProperty, value); }
        }

        public static readonly DependencyProperty CloseAdornerTimeOutProperty =
            DependencyProperty.Register("CloseAdornerTimeOut", typeof(double), typeof(AdornedControl),
                new FrameworkPropertyMetadata(1.0, CloseAdornerTimeOut_PropertyChanged)); //default was originally 2.0, changed to 1.0, maybe change later?

        private static void CloseAdornerTimeOut_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (AdornedControl)o;
            c._closeAdornerTimer.Stop();
            c._closeAdornerTimer.Interval = TimeSpan.FromSeconds(c.CloseAdornerTimeOut);
        }

        /// <summary>
        /// By default this property is set to null.
        /// When set to non-null it specifies the part name of a UI element
        /// in the visual tree of the AdornedControl content that is to be adorned.
        /// When this property is null it is the AdornerControl content that is adorned,
        /// however when it is set the visual-tree is searched for a UI element that has the
        /// specified part name, if the part is found then that UI element is adorned, otherwise
        /// an exception "Failed to find part ..." is thrown.        /// 
        /// </summary>
        public string AdornedTemplatePartName
        {
            get { return (string)GetValue(AdornedTemplatePartNameProperty); }
            set { SetValue(AdornedTemplatePartNameProperty, value); }
        }
        
        public static readonly DependencyProperty AdornedTemplatePartNameProperty =
            DependencyProperty.Register("AdornedTemplatePartName", typeof(string), typeof(AdornedControl),
                new FrameworkPropertyMetadata(null));

        #endregion //Dependency Properties

        #region Commands

        public static readonly RoutedCommand ShowAdornerCommand = new RoutedCommand("ShowAdorner", typeof(AdornedControl));
        private static readonly CommandBinding ShowAdornerCommandBinding = new CommandBinding(ShowAdornerCommand, ShowAdornerCommand_Executed);

        private static void ShowAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl)target;
            c.ShowAdorner();
        }

        public static readonly RoutedCommand FadeInAdornerCommand = new RoutedCommand("FadeInAdorner", typeof(AdornedControl));
        private static readonly CommandBinding FadeInAdornerCommandBinding = new CommandBinding(FadeInAdornerCommand, FadeInAdornerCommand_Executed);

        private static void FadeInAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl)target;
            c.FadeOutAdorner();
        }
        
        public static readonly RoutedCommand HideAdornerCommand = new RoutedCommand("HideAdorner", typeof(AdornedControl));
        private static readonly CommandBinding HideAdornerCommandBinding = new CommandBinding(HideAdornerCommand, HideAdornerCommand_Executed);

        private static void HideAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl)target;
            c.HideAdorner();
        }
        
        public static readonly RoutedCommand FadeOutAdornerCommand = new RoutedCommand("FadeOutAdorner", typeof(AdornedControl));
        private static readonly CommandBinding FadeOutAdornerCommandBinding = new CommandBinding(FadeInAdornerCommand, FadeOutAdornerCommand_Executed);

        private static void FadeOutAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            var c = (AdornedControl)target;
            c.FadeOutAdorner();
        }

        #endregion Commands

        #region Public Methods

        /// <summary>
        /// Show the adorner.
        /// </summary>
        public void ShowAdorner()
        {
            IsAdornerVisible = true;
        }

        /// <summary>
        /// Hide the adorner.
        /// </summary>
        public void HideAdorner()
        {
            IsAdornerVisible = false;
        }

        /// <summary>
        /// Fade the adorner in and make it visible.
        /// </summary>
        public void FadeInAdorner()
        {
            if (_adornerShowState == AdornerShowState.Visible ||
                _adornerShowState == AdornerShowState.FadingIn ||
                _adorner == null)
            {
                // Already visible or fading in.
                return;
            }

            ShowAdorner();

            if (_adornerShowState != AdornerShowState.FadingOut)
            {
                _adorner.Opacity = 0.0;
            }

            _adornerShowState = AdornerShowState.FadingIn;

            var doubleAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(FadeInTime)));
            doubleAnimation.Completed += fadeInAnimation_Completed;
            doubleAnimation.Freeze();
                
            _adorner.BeginAnimation(OpacityProperty, doubleAnimation);
        }

        /// <summary>
        /// Fade the adorner out and make it invisible.
        /// </summary>
        public void FadeOutAdorner()
        {
            if (_adornerShowState == AdornerShowState.FadingOut ||
                _adornerShowState == AdornerShowState.Hidden ||
                _adorner == null)
            {
                // Already fading out or hidden.
                return;
            }

            _adornerShowState = AdornerShowState.FadingOut;

            var fadeOutAnimation = new DoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(FadeOutTime)));
            fadeOutAnimation.Completed += fadeOutAnimation_Completed;
            fadeOutAnimation.Freeze();

            _adorner.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        /// <summary>
        /// Called to build the visual tree.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ShowOrHideAdornerInternal();
        }

        /// <summary>
        /// Finds a child element in the visual tree that has the specified name.
        /// Returns null if no child with that name exists.
        /// </summary>
        public static FrameworkElement FindNamedChild(FrameworkElement rootElement, string childName)
        {
            int numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for(int i = 0; i < numChildren; ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(rootElement, i);
                var childElement = child as FrameworkElement;
                if(childElement != null && childElement.Name == childName)
                {
                    return childElement;
                }

                FrameworkElement foundElement = FindNamedChild(childElement, childName);
                if(foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }

        #endregion //Public Methods

        #region Private Methods

        /// <summary>
        /// Update the DataContext of the adorner from the adorned control.
        /// </summary>
        private void UpdateAdornerDataContext()
        {
            if(AdornerContent != null)
            {
                AdornerContent.DataContext = DataContext;
            }
        }

        /// <summary>
        /// Internal method to show or hide the adorner based on the value of IsAdornerVisible.
        /// </summary>
        private void ShowOrHideAdornerInternal()
        {
            NotifyPropertyChanged("IsAdornerVisible");
            if(IsAdornerVisible)
            {
                ShowAdornerInternal();
            }
            else
            {
                HideAdornerInternal();
            }
        }

        /// <summary>
        /// Internal method to show the adorner.
        /// </summary>
        private void ShowAdornerInternal()
        {
            if(_adorner != null)
            {
                // Already adorned.
                return;
            }

            if(AdornerContent != null)
            {
                if(_adornerLayer == null)
                {
                    UserControl linkedDisplay = this.GetAncestorObject<LinkedDisplayView>(); //GetLinkedDisplayView(this);
                    if (linkedDisplay != null)
                    {
                        _adornerLayer = AdornerLayer.GetAdornerLayer(linkedDisplay);
                    }
                }

                if(_adornerLayer != null)
                {
                    FrameworkElement adornedControl = this; // The control to be adorned defaults to 'this'.

                    if(!string.IsNullOrEmpty(AdornedTemplatePartName))
                    {
                        //
                        // If 'AdornedTemplatePartName' is set to a valid string then search the visual-tree
                        // for a UI element that has the specified part name.  If we find it then use it as the
                        // adorned control, otherwise throw an exception.
                        //
                        adornedControl = FindNamedChild(this, AdornedTemplatePartName);
                        if(adornedControl == null)
                        {
                            throw new ApplicationException("Failed to find a FrameworkElement in the visual-tree with the part name '" + AdornedTemplatePartName + "'.");
                        }
                    }

                    _adorner = new FrameworkElementAdorner(AdornerContent, adornedControl,
                                                               HorizontalAdornerPlacement, VerticalAdornerPlacement,
                                                               AdornerOffsetX, AdornerOffsetY);
                    _adornerLayer.Add(_adorner);

                    UpdateAdornerDataContext();
                }
            }

            _adornerShowState = AdornerShowState.Visible;
        }

        /// <summary>
        /// Internal method to hide the adorner.
        /// </summary>
        private void HideAdornerInternal()
        {
            if(_adornerLayer == null || _adorner == null)
            {
                // Not already adorned.
                return;
            }

            //
            // Stop the timer that might be about to fade out the adorner.
            //
            _closeAdornerTimer.Stop();
            _adornerLayer.Remove(_adorner);
            _adorner.DisconnectChild();

            _adorner = null;
            _adornerLayer = null;

            //
            // Ensure that the state of the adorned control reflects that
            // the the adorner is no longer.
            //
            _adornerShowState = AdornerShowState.Hidden;
        }

        /// <summary>
        /// Shared mouse enter code.
        /// </summary>
        private void MouseEnterLogic()
        {
            if(!IsMouseOverShowEnabled && !IsAdornerVisible)
            {
                return;
            }

            _closeAdornerTimer.Stop();

            _openAdornerTimer.Start();
        }

        /// <summary>
        /// Shared mouse leave code.
        /// </summary>
        private void MouseLeaveLogic()
        {
            if(!IsMouseOverShowEnabled)
            {
                return;
            }

            _openAdornerTimer.Stop();

            _closeAdornerTimer.Start();
        }

        /// <summary>
        /// Called when the mouse cursor enters the area of the adorned control.
        /// </summary>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            MouseEnterLogic();
            e.Handled = false;
        }

        /// <summary>
        /// Called when the mouse cursor leaves the area of the adorned control.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            MouseLeaveLogic();
            e.Handled = false;
        }

        /// <summary>
        /// Called when the open adorner time-out has ellapsed, the mouse has moved
        /// over from the adorned control and the adorner and it is time to open the adorner.
        /// </summary>
        private void openAdornerTimer_Tick(object sender, EventArgs e)
        {
            _openAdornerTimer.Stop();

            if (IsMouseOverShowEnabled)
            {
            	FadeInAdorner();
            }
        }

        /// <summary>
        /// Called when the close adorner time-out has ellapsed, the mouse has moved
        /// away from the adorned control and the adorner and it is time to close the adorner.
        /// </summary>
        private void closeAdornerTimer_Tick(object sender, EventArgs e)
        {
            _closeAdornerTimer.Stop();

            FadeOutAdorner();
        }

        #endregion //Private Methods

        #region Events

        /// <summary>
        /// Event raised when the DataContext of the adorned control changes.
        /// </summary>
        private void AdornedControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateAdornerDataContext();
        }

        /// <summary>
        /// Event raised when the mouse cursor enters the area of the adorner.
        /// </summary>
        private void adornerContent_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnterLogic();
            e.Handled = false;
        }

        /// <summary>
        /// Event raised when the mouse cursor leaves the area of the adorner.
        /// </summary>
        private void adornerContent_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveLogic();
            e.Handled = false;
        }

        /// <summary>
        /// Event raised when the fade in animation has completed.
        /// </summary>
        private void fadeInAnimation_Completed(object sender, EventArgs e)
        {
            _adornerShowState = AdornerShowState.Visible;
        }

        /// <summary>
        /// Event raised when the fade-out animation has completed.
        /// </summary>
        private void fadeOutAnimation_Completed(object sender, EventArgs e)
        {
            if(_adornerShowState == AdornerShowState.FadingOut)
            {
                // Still fading out, eg it wasn't aborted.
                HideAdorner();
            }
        }

        #endregion //Events

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion //INotifyPropertyChanged
  
    }
}
