using Classroom_Learning_Partner.Views.PageObjects;
using Classroom_Learning_Partner.ViewModels;
using System.Windows.Threading;
using Classroom_Learning_Partner.Model;
using System.Threading;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;
using System.Windows.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : Catel.Windows.Controls.UserControl
    {
        private const double PAGE_OBJECT_CONTAINER_ADORNER_DELAY = 800; //time to wait until adorner appears

        private bool isMouseDown = false;
        private DispatcherTimer timer = null;
        private int DirtyHitbox = 0;

        public CLPPageView()
        {           
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(PAGE_OBJECT_CONTAINER_ADORNER_DELAY);
            timer.Tick += new EventHandler(timer_Tick);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        private void TopCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.StylusDevice != null && e.StylusDevice.Inverted)
            {
                switch (App.MainWindowViewModel.PageEraserInteractionMode)
                {
                    case PageEraserInteractionMode.ObjectEraser:
                        VisualTreeHelper.HitTest(TopCanvas, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(EraserHitResult), new PointHitTestParameters(e.GetPosition(TopCanvas)));
                        break;
                    default:
                        break;
                }
            }
            else 
            {
                if (!(isMouseDown))
                {
                    VisualTreeHelper.HitTest(TopCanvas, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(TopCanvas)));
                }
            }
        }

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if (o.GetType() == typeof(Grid))
            {
                if ((o as Grid).Name == "ContainerHitBox" && App.MainWindowViewModel.IsAuthoring)
                {
                    return HitTestFilterBehavior.ContinueSkipChildren;
                }
                else
                {
                    return HitTestFilterBehavior.Continue;
                }
            }
            else if (o.GetType().BaseType == typeof(Shape))
            {
                if ((o as Shape).Name == "PageObjectHitBox")
                {
                    return HitTestFilterBehavior.ContinueSkipChildren;
                }
                else
                {
                    return HitTestFilterBehavior.Continue;
                }
            }
            else
            {
                return HitTestFilterBehavior.Continue;
            }
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(Grid))
            {
                if ((result.VisualHit as Grid).DataContext is CLPStamp)
                {
                    MainInkCanvas.IsHitTestVisible = false;
                }
                if ((result.VisualHit as Grid).Name == "ContainerHitBox")
                {
                    if (App.MainWindowViewModel.IsAuthoring)
                    {
                        if (DirtyHitbox > 3)
                        {
                            timer.Interval = TimeSpan.FromMilliseconds(PAGE_OBJECT_CONTAINER_ADORNER_DELAY);
                            timer.Start();
                        }
                        DirtyHitbox = 0;
                        return HitTestResultBehavior.Stop;
                    }
                }
                return HitTestResultBehavior.Continue; 
            }
            else if (result.VisualHit.GetType().BaseType == typeof(Shape))
            {
                if ((result.VisualHit as Shape).Name == "PageObjectHitBox")
                {
                    if (DirtyHitbox > 3)
                    {
                        double timer_delay = 0;
                        if ((result.VisualHit as Shape).Tag != null)
                        {
                            timer_delay = double.Parse((result.VisualHit as Shape).Tag as string);
                            
                        }
                        timer.Interval = TimeSpan.FromMilliseconds(timer_delay);
                        if ((result.VisualHit as Shape).DataContext is CLPStrokePathContainerViewModel)
                        {
                            if (((result.VisualHit as Shape).DataContext as CLPStrokePathContainerViewModel).IsStamped && !((result.VisualHit as Shape).DataContext as CLPStrokePathContainerViewModel).PageObject.IsBackground)
                            {
                                timer.Start();
                            }
                        }
                        else if ((result.VisualHit as Shape).DataContext is CLPSnapTileContainerViewModel)
                        {
                            if (!((result.VisualHit as Shape).DataContext as CLPSnapTileContainerViewModel).IsBackground)
                            {
                                timer.Start();
                            }
                        }
                        else
                        {
                            timer.Start();
                        }
                    }
                    DirtyHitbox = 0;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }
            else
            {
                if (DirtyHitbox > 100)
                {
                    DirtyHitbox = 20; //stops DirtyHitbox from exceeding bounds of int
                }
                DirtyHitbox++;
                if (DirtyHitbox > 3 || isMouseDown)
                {
                    timer.Stop();
                    MainInkCanvas.IsHitTestVisible = true;
                }
                
                return HitTestResultBehavior.Continue;
            }
        }

        private HitTestResultBehavior EraserHitResult(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(Grid))
            {
                if ((result.VisualHit as Grid).Name == "ContainerHitBox")
                {
                    if ((result.VisualHit as Grid).DataContext is CLPStrokePathContainer)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPStrokePathContainer).IsBackground || 
                            (((result.VisualHit as Grid).DataContext as CLPStrokePathContainer).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPStrokePathContainer);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPSnapTileContainer)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPSnapTileContainer).IsBackground || 
                            (((result.VisualHit as Grid).DataContext as CLPSnapTileContainer).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPSnapTileContainer);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPShape)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPShape).IsBackground || 
                            (((result.VisualHit as Grid).DataContext as CLPShape).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPShape);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPStamp)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPStamp).IsBackground || 
                            (((result.VisualHit as Grid).DataContext as CLPStamp).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPStamp);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPAudio)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPAudio).IsBackground ||
                            (((result.VisualHit as Grid).DataContext as CLPAudio).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPAudio);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPImage)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPImage).IsBackground ||
                            (((result.VisualHit as Grid).DataContext as CLPImage).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPImage);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPTextBox)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPTextBox).IsBackground ||
                            (((result.VisualHit as Grid).DataContext as CLPTextBox).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPTextBox);
                        }
                    }
                    else if ((result.VisualHit as Grid).DataContext is CLPInkRegion)
                    {
                        if (!((result.VisualHit as Grid).DataContext as CLPInkRegion).IsBackground ||
                            (((result.VisualHit as Grid).DataContext as CLPInkRegion).IsBackground && App.MainWindowViewModel.IsAuthoring))
                        {
                            CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Grid).DataContext as CLPInkRegion);
                        }
                    }
                }
                return HitTestResultBehavior.Stop; 
            }
            else if (result.VisualHit.GetType().BaseType == typeof(Shape))
            {
                if ((result.VisualHit as Shape).DataContext is CLPSnapTileContainerViewModel)
                {
                    if (!((result.VisualHit as Shape).DataContext as CLPSnapTileContainerViewModel).PageObject.IsBackground || 
                        (((result.VisualHit as Shape).DataContext as CLPSnapTileContainerViewModel).PageObject.IsBackground && App.MainWindowViewModel.IsAuthoring))
                    {
                        CLPServiceAgent.Instance.RemovePageObjectFromPage(((result.VisualHit as Shape).DataContext as CLPSnapTileContainerViewModel).PageObject);
                    }
                }
                if ((result.VisualHit as Shape).DataContext is CLPStrokePathContainerViewModel)
                {
                    if (!((result.VisualHit as Shape).DataContext as CLPStrokePathContainerViewModel).PageObject.IsBackground ||
                        (((result.VisualHit as Shape).DataContext as CLPStrokePathContainerViewModel).PageObject.IsBackground && App.MainWindowViewModel.IsAuthoring))
                    {
                        CLPServiceAgent.Instance.RemovePageObjectFromPage(((result.VisualHit as Shape).DataContext as CLPStrokePathContainerViewModel).PageObject);
                    }
                }
                if ((result.VisualHit as Shape).DataContext is CLPAudio)
                {
                    if (!((result.VisualHit as Shape).DataContext as CLPAudio).IsBackground ||
                        (((result.VisualHit as Shape).DataContext as CLPAudio).IsBackground && App.MainWindowViewModel.IsAuthoring))
                    {
                        CLPServiceAgent.Instance.RemovePageObjectFromPage((result.VisualHit as Shape).DataContext as CLPAudio);
                    }
                }
                if ((result.VisualHit as Shape).DataContext is CLPStampViewModel)
                {
                    if (!((result.VisualHit as Shape).DataContext as CLPStampViewModel).IsBackground ||
                        (((result.VisualHit as Shape).DataContext as CLPStampViewModel).IsBackground && App.MainWindowViewModel.IsAuthoring))
                    {
                        CLPServiceAgent.Instance.RemovePageObjectFromPage(((result.VisualHit as Shape).DataContext as CLPStampViewModel).PageObject);
                    }
                }
                return HitTestResultBehavior.Stop;
            }
            return HitTestResultBehavior.Continue;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            MainInkCanvas.IsHitTestVisible = false;

        }

        private void TopCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            isMouseDown = true;
            timer.Stop();
            //STeve - if inUpMode clpservice.up(pos)
        }

        private void TopCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;

            Point pt = e.GetPosition(this.TopCanvas);
            if (pt.X > 1056) pt.X = 1056;
            if (pt.Y > 816) pt.Y = 816;

            // Don't want to add objects if in inverted mode
            if (!(e.StylusDevice != null && e.StylusDevice.Inverted))
            {
                switch (App.MainWindowViewModel.PageInteractionMode)
                {
                    case PageInteractionMode.None:
                        break;
                    case PageInteractionMode.SnapTile:
                        CLPSnapTileContainer snapTile = new CLPSnapTileContainer(pt);
                        CLPServiceAgent.Instance.AddPageObjectToPage((this.DataContext as CLPPageViewModel).Page, snapTile);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
