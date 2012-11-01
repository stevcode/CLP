using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView : Catel.Windows.Controls.UserControl
    {
        private const double PAGE_OBJECT_CONTAINER_ADORNER_DELAY = 800; //time to wait until adorner appears
        private DispatcherTimer timer = null;
        private bool isStampAdornerSet = false;

        public PageObjectContainerView()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(PAGE_OBJECT_CONTAINER_ADORNER_DELAY);
            timer.Tick += new EventHandler(timer_Tick);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(ACLPPageObjectBaseViewModel);
        }

        protected override System.Type GetViewModelType(object dataContext)
        {
            if (dataContext is CLPAudio) return typeof(CLPAudioViewModel);
            if (dataContext is CLPDataTable) return typeof(CLPDataTableViewModel);
            if (dataContext is CLPHandwritingRegion) return typeof(CLPHandwritingRegionViewModel);
            if (dataContext is CLPImage) return typeof(CLPImageViewModel);
            if (dataContext is CLPInkShapeRegion) return typeof(CLPInkShapeRegionViewModel);
            if (dataContext is CLPShadingRegion) return typeof(CLPShadingRegionViewModel);
            if (dataContext is CLPShape) return typeof(CLPShapeViewModel);
            if (dataContext is CLPSnapTileContainer) return typeof(CLPSnapTileContainerViewModel);
            if (dataContext is CLPStamp) return typeof(CLPStampViewModel);
            if (dataContext is CLPStrokePathContainer) return typeof(CLPStrokePathContainerViewModel);
            if (dataContext is CLPTextBox) return typeof(CLPTextBoxViewModel);

            return null;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
                Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
                double x = pageObject.XPosition + e.HorizontalChange;
                double y = pageObject.YPosition + e.VerticalChange;
                if (x < 0)
                {
                    x = 0;
                }
                if (y < 0)
                {
                    y = 0;
                }
                if (x > 1056 - pageObject.Width)
                {
                    x = 1056 - pageObject.Width;
                }
                if (y > 816 - pageObject.Height)
                {
                    y = 816 - pageObject.Height;
                }

                Point pt = new Point(x, y);
                Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pt);
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
                double newHeight = pageObject.Height + e.VerticalChange;
                double newWidth = pageObject.Width + e.HorizontalChange;
                if (newHeight < 10)
                {
                    newHeight = 10;
                }
                if (newWidth < 10)
                {
                    newWidth = 10;
                }
                if (newHeight + pageObject.YPosition > 816)
                {
                    newHeight = pageObject.Height;
                }
                if (newWidth + pageObject.XPosition > 1056)
                {
                    newWidth = pageObject.Width;
                }

                Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectDimensions(pageObject, newHeight, newWidth);
            }
        }

        private void PageObjectContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            isStampAdornerSet = false;
            timer.Stop();
        }

        private void PageObjectContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if(App.MainWindowViewModel.IsAuthoring)
            {
                ////VisualTreeHelper.HitTest(PageObjectContainer, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(PageObjectContainer)));
            }
        }

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if (o.GetType() == typeof(Grid))
            {
                if ((o as Grid).Name == "StampObject")
                {
                    return HitTestFilterBehavior.ContinueSkipChildren;
                }
                else
                {
                    return HitTestFilterBehavior.Continue;
                }
            }
            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            if (result.VisualHit.GetType().BaseType == typeof(Shape))
            {
                if ((result.VisualHit as Shape).DataContext is CLPStamp)
                {
                    if (!isStampAdornerSet)
                        ////adornerCanvas.Visibility = Visibility.Hidden;
                        isStampAdornerSet = true;
                    if (!timer.IsEnabled)
                        timer.Start();
                    return HitTestResultBehavior.Stop;
                }
            }
            return HitTestResultBehavior.Continue;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            ////adornerCanvas.Visibility = Visibility.Visible;
        }

        private void PageObjectView_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Console.WriteLine("mousey leavey");
        }
    }
}
