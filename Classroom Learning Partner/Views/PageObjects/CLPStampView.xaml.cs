using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : Catel.Windows.Controls.UserControl
    {
        private const double PAGE_OBJECT_CONTAINER_ADORNER_DELAY = 800; //time to wait until adorner appears
        private DispatcherTimer timer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(PAGE_OBJECT_CONTAINER_ADORNER_DELAY);
            timer.Tick += new EventHandler(timer_Tick);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStampViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            CLPServiceAgent.Instance.RemovePageObjectFromPage(stamp.PageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);

            double x = stamp.XPosition + e.HorizontalChange;
            double y = stamp.YPosition + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 1056 - stamp.Width)
            {
                x = 1056 - stamp.Width;
            }
            if (y > 816 - stamp.Height)
            {
                y = 816 - stamp.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(stamp.PageObject, pt);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            double newHeight = stamp.Height + e.VerticalChange;
            double newWidth = stamp.Width + e.HorizontalChange;
            if (newHeight < 10)
            {
                newHeight = 10;
            }
            if (newWidth < 10)
            {
                newWidth = 10;
            }
            if (newHeight + stamp.YPosition > 816)
            {
                newHeight = stamp.Height;
            }
            if (newWidth + stamp.XPosition > 1056)
            {
                newWidth = stamp.Width;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(stamp.PageObject, newHeight, newWidth);
        }

        private void PageObjectHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
            adornerCanvas.Visibility = Visibility.Hidden;
        }

        private void PageObjectHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }

        private void StrokeContainerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
        }

        private void StrokeContainerwMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            CLPStampViewModel stamp = (this.ViewModel as CLPStampViewModel);
            adornerCanvas.Visibility = Visibility.Visible;
        }

        private void StampObject_MouseMove(object sender, MouseEventArgs e)
        {
            CLPStampViewModel stamp = (this.ViewModel as CLPStampViewModel);
            if (!stamp.PageObject.IsBackground)
            {
                VisualTreeHelper.HitTest(StampObject, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(StampObject)));
            }
            else if (stamp.PageObject.IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                adornerCanvas.Visibility = Visibility.Hidden;
            }
        }

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if (o.GetType().BaseType == typeof(Shape))
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
            if (result.VisualHit.GetType().BaseType == typeof(Shape))
            {
                if ((result.VisualHit as Shape).DataContext is CLPStampViewModel)
                {
                    if (!timer.IsEnabled)
                        timer.Start();
                    return HitTestResultBehavior.Stop;
                }
                else if ((result.VisualHit as Shape).DataContext is CLPStrokePathContainerViewModel)
                {
                    timer.Stop();
                    return HitTestResultBehavior.Stop;
                }
                else
                {
                    timer.Stop();
                    return HitTestResultBehavior.Continue;
                }
            }
            return HitTestResultBehavior.Continue;
        }
    }
}
