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

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : UserControl
    {
        private bool isMouseDown = false;

        public CLPPageView()
        {
            InitializeComponent();
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
                if ((result.VisualHit as Grid).Name == "HitBox")
                {
                    //Add timer to delay appearance of adorner
                    //are these necessary?
                    //PageObjectContainerView pageObjectContainerView = (LogicalTreeHelper.GetParent(LogicalTreeHelper.GetParent(LogicalTreeHelper.GetParent(result.VisualHit))) as PageObjectContainerView);
                    //PageObjectContainerViewModel pageObjectContainerViewModel = (pageObjectContainerView.DataContext as PageObjectContainerViewModel);
                    MainInkCanvas.IsHitTestVisible = false;
                }
                return HitTestResultBehavior.Stop;
                
            }
            else
            {
                MainInkCanvas.IsHitTestVisible = true;
                return HitTestResultBehavior.Continue;
            }

            
            
        }

        private void TopCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = false;
            isMouseDown = true;
        }

        private void TopCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

    }
}
