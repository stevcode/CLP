using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.Views.PageObjects;
using Classroom_Learning_Partner.ViewModels;
using System.Windows.Threading;
using Classroom_Learning_Partner.Model;
using System.Threading;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : UserControl
    {
        private const double ADORNER_DELAY = 800; //time to wait until adorner appears

        private bool isMouseDown = false;
        private DispatcherTimer timer = null;
        private int DirtyHitbox = 0;
        public CLPServiceAgent CLPService;

        public CLPPageView()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(ADORNER_DELAY);
            timer.Tick += new EventHandler(timer_Tick);
            this.CLPService = new CLPServiceAgent();

            // Register so that we send mouse coordinates for the laser to the projector
            // When the laser is enabled, add a listener to MouseMove so that sendLaserPointerPosition is called
            AppMessages.SetLaserPointerMode.Register(this, (isLaserEnabled) =>
            {
                if (isLaserEnabled) RootGrid.MouseMove += sendLaserPointerPosition;
                else RootGrid.MouseMove -= sendLaserPointerPosition;
            });

            //Register so we receive mouse coordinates for the laser on the projector
            AppMessages.UpdateLaserPointerPosition.Register(this, (pt) =>
            {
                updateLaserPointerPosition(pt);
            });
            
        }

        private void TopCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDown)
            {
                VisualTreeHelper.HitTest(TopCanvas, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(TopCanvas)));
            }
        }

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if (o.GetType() == typeof(Grid))
            {
                if ((o as Grid).Name == "HitBox")
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
                //Console.WriteLine("over any grid");
                if ((result.VisualHit as Grid).Name == "HitBox")
                {
                    //Add timer to delay appearance of adorner
                    if (DirtyHitbox > 3)
                    {
                        timer.Start();
                    }
                    DirtyHitbox = 0;
                    
                }
                return HitTestResultBehavior.Stop;
                
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
                
                Console.WriteLine(DirtyHitbox.ToString());
                return HitTestResultBehavior.Continue;
            }

            
            
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("tick fired");
            timer.Stop();
            MainInkCanvas.IsHitTestVisible = false;

        }

        private LaserPoint _laserPoint = new LaserPoint();
        public void updateLaserPointerPosition(Point pt)
        {
            //place the red dot at the coordinates, LaserPoint.xaml
            Thread t = new Thread(new ThreadStart(
                delegate
                {
                    Dispatcher.Invoke(DispatcherPriority.Render, new Action<Point>(setUILaserPointerValue), pt);
                }
            ));
            t.Start();
        }

        private void setUILaserPointerValue(Point pt)
        {
            if (RootGrid.Children.Contains(_laserPoint)) RootGrid.Children.Remove(_laserPoint);
            RootGrid.Children.Add(_laserPoint);
            _laserPoint.RootGrid.Margin = new Thickness(pt.X, pt.Y, 0, 0);
        }

        private void sendLaserPointerPosition(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point pt = e.GetPosition(this.RootGrid);
                if (pt.X > 816) pt.X = 816;
                if (pt.Y > 1056) pt.Y = 1056;
                CLPService.SendLaserPosition(e.GetPosition(this.RootGrid));
            }
        }

        private void TopCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {   
            isMouseDown = true;
            timer.Stop();
        }

        private void TopCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

    }
}
