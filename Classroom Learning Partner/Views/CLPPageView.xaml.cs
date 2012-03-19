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
                if ((result.VisualHit as Grid).Name == "ContainerHitBox")
                {
                    if (App.MainWindowViewModel.IsAuthoring)
                    {
                        if (DirtyHitbox > 3)
                        {
                            //timer.Start();
                            MainInkCanvas.IsHitTestVisible = false;
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
                        //timer.Start();
                        MainInkCanvas.IsHitTestVisible = false;
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
                    //timer.Stop();
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

            switch (App.MainWindowViewModel.PageObjectAddMode)
            {
                case PageObjectAddMode.None:
                    break;
                case PageObjectAddMode.SnapTile:
                    CLPSnapTileContainer snapTile = new CLPSnapTileContainer(pt, "SpringGreen");
                    CLPServiceAgent.Instance.AddPageObjectToPage((this.DataContext as CLPPageViewModel).Page, snapTile);
                    break;
                default:
                    break;
            }
        }
    }
}
