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

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : Catel.Windows.Controls.UserControl
    {
        private const double ADORNER_DELAY = 800; //time to wait until adorner appears

        private bool isMouseDown = false;
        private DispatcherTimer timer = null;
        private int DirtyHitbox = 0;
        private bool isSnapTileEnabled = false;

        public CLPPageView()
        {           
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(ADORNER_DELAY);
            timer.Tick += new EventHandler(timer_Tick);
            //We need the inkCanvas in HistoryVM to start a new thread to enable playback
            CLPServiceAgent.Instance.SendInkCanvas(MainInkCanvas);
            
            AppMessages.SetSnapTileMode.Register(this, (setSnapTileEnabled) =>
                {
                    isSnapTileEnabled = setSnapTileEnabled;
                });

            // Register so that we send mouse coordinates for the laser to the projector
            // When the laser is enabled, add a listener to MouseMove so that sendLaserPointerPosition is called
            //AppMessages.SetLaserPointerMode.Register(this, (isLaserEnabled) =>
            //{
            //    if (isLaserEnabled) RootGrid.MouseMove += sendLaserPointerPosition;
            //    else
            //    {
            //        RootGrid.MouseMove -= sendLaserPointerPosition;
            //        CLPService.TurnOffLaser();  
            //    }
            //});

            //if (App.CurrentUserMode == App.UserMode.Projector)
            //{
            //    TopCanvas.Children.Add(_laserPoint);
            //    _laserPoint.Visibility = Visibility.Collapsed;

            //    AppMessages.TurnOffLaser.Register(this, (action) =>
            //    {
            //        _laserPoint.Visibility = Visibility.Collapsed;
            //    });
            //}

            ////Register so we receive mouse coordinates for the laser on the projector
            //AppMessages.UpdateLaserPointerPosition.Register(this, (pt) =>
            //{
            //    updateLaserPointerPosition(pt);
            //});

            
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
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


                    if (App.MainWindowViewModel.IsAuthoring)
                    {
                        //Add timer to delay appearance of adorner
                        if (DirtyHitbox > 3)
                        {
                            timer.Start();
                        }
                        DirtyHitbox = 0;
                    }
                    
                    
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
                
                return HitTestResultBehavior.Continue;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            MainInkCanvas.IsHitTestVisible = false;

        }

        //private LaserPoint _laserPoint = new LaserPoint();
        //private Thickness _laserPointMargins = new Thickness();

        //// Does the actual updating of the LaserPoint
        //private void updateLaserPointerPosition(Point pt)
        //{
        //    //if (RootGrid.Children.Contains(_laserPoint)) RootGrid.Children.Remove(_laserPoint);
        //    _laserPoint.Visibility = Visibility.Visible;
        //    _laserPointMargins.Left = pt.X;
        //    _laserPointMargins.Top = pt.Y;
        //    _laserPoint.RootGrid.Margin = _laserPointMargins;
        //}

        //// use this variable so we're not sending redundant info over the network for TurnOffLaser()
        //private bool _isLaserOn;
        //private void sendLaserPointerPosition(object sender, MouseEventArgs e)
        //{
        //    if (isMouseDown)
        //    {
        //        Point pt = e.GetPosition(this.TopCanvas);
        //        if (pt.X > 1056) pt.X = 1056;
        //        if (pt.Y > 816) pt.Y = 816;
        //        CLPService.SendLaserPosition(pt);
        //        _isLaserOn = true;
        //    }
        //    else
        //    {
        //        if (_isLaserOn)
        //        {
        //            CLPService.TurnOffLaser();
        //            _isLaserOn = false;
        //        }
        //    }
        //}

        private void TopCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            isMouseDown = true;
            timer.Stop();
            //STeve - if inUpMode clpservice.up(pos)
        }

        private void TopCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;

            //STeve - if inDownMode clpservice.down(pos)
            if (isSnapTileEnabled)
            {
                Point pt = e.GetPosition(this.TopCanvas);
                if (pt.X > 1056) pt.X = 1056;
                if (pt.Y > 816) pt.Y = 816;
                CLPServiceAgent.Instance.AddPageObjectToPage(new CLPSnapTileContainer(pt, "SpringGreen"));
            }
        }

    }
}
