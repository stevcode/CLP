namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels.PageObjects;
    using Classroom_Learning_Partner.Model;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using System.Windows;
    using System.Windows.Input;
    using System;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using System.Threading;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : Catel.Windows.Controls.UserControl
    {
        private bool isMouseDown = false;
        private bool isHidden = true;
        private const double PAGE_OBJECT_CONTAINER_ADORNER_DELAY = 800; //time to wait until adorner appears
        private DispatcherTimer timer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;

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

            double x = stamp.Position.X + e.HorizontalChange;
            double y = stamp.Position.Y + e.VerticalChange;
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
            if (newHeight + stamp.Position.Y > 816)
            {
                newHeight = stamp.Height;
            }
            if (newWidth + stamp.Position.X > 1056)
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

/*        private void PageObjectHitBox_StylusEnter(object sender, StylusEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
        }

        private void PageObjectHitBox_StylusLeave(object sender, StylusEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }*/

        private void StrokeContainerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            timer.Stop();
        }

        private void StrokeContainerwMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            Console.WriteLine("Woot");
            //if (!stamp.PageObject.IsBackground)
            //{
                adornerCanvas.Visibility = Visibility.Visible;
            //}
        }

        private void StampObject_MouseMove(object sender, MouseEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            if ((stamp.PageObject.IsBackground && App.MainWindowViewModel.IsAuthoring) || (!stamp.PageObject.IsBackground))
            {
                VisualTreeHelper.HitTest(StampObject, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(StampObject)));
            }
            else
            {
                adornerCanvas.Visibility = Visibility.Hidden;
            }
             //   StylusEnter="PageObjectHitBox_StylusEnter"  StylusInRange="PageObjectHitBox_StylusEnter"  StylusLeave="PageObjectHitBox_StylusLeave"  StylusOutOfRange="PageObjectHitBox_StylusLeave"
             
        }

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            Console.WriteLine("Filter" + o.GetType());
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
            Console.WriteLine("Start: " + result.VisualHit.GetType());
            Console.WriteLine("Base: " + result.VisualHit.GetType().BaseType);
            if (result.VisualHit.GetType().BaseType == typeof(Shape))
            {
                Console.WriteLine("Shape: " + (result.VisualHit as Shape).DataContext);
                Console.WriteLine("Shape Name: " + (result.VisualHit as Shape).Name);
                if ((result.VisualHit as Shape).DataContext is CLPStampViewModel)
                {
                    if (!timer.IsEnabled)
                        timer.Start();
                    Console.WriteLine("Handle");
                    //adornerCanvas.Visibility = Visibility.Hidden;
                    return HitTestResultBehavior.Stop;
                }
                else if ((result.VisualHit as Shape).DataContext is CLPStrokePathContainerViewModel)
                {
                    timer.Stop();
                    Console.WriteLine("Stroke Me!");
                    return HitTestResultBehavior.Stop;
                }
                else
                {
                    Console.WriteLine("Boo!");
                    timer.Stop();
                    return HitTestResultBehavior.Continue;
                }
            }
            return HitTestResultBehavior.Continue;
        }
    }
}
