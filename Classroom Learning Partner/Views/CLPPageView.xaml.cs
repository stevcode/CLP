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
        public CLPPageView()
        {
            InitializeComponent();
            //Rectangle test = new Rectangle();
            //test.Width = 150;
            //test.Height = 150;
            //SolidColorBrush myBrush = new SolidColorBrush(Colors.Blue);
            //test.Fill = myBrush;
            //test.MouseEnter += new MouseEventHandler(test_MouseEnter);
            ////test.PreviewMouseMove += new MouseEventHandler(test_PreviewMouseMove);
            //test.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(test_PreviewMouseLeftButtonDown);
            //InkCanvas.SetTop(test, 300);
            //InkCanvas.SetLeft(test, 300);

            //MainInkCanvas.Children.Add(test);
        }

        //void test_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    Console.WriteLine("MouseOver");
        //    SolidColorBrush brushy = new SolidColorBrush(Colors.Red);
        //    (sender as Rectangle).Fill = brushy;
        //}

        //void test_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    MainInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        //}

        //void test_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    Console.WriteLine("MouseOver");
        //    SolidColorBrush brushy = new SolidColorBrush(Colors.Red);
        //    (sender as Rectangle).Fill = brushy;
        //}

        private void TopCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(TopCanvas);
            VisualTreeHelper.HitTest(TopCanvas, null, new HitTestResultCallback(noFocusCallback), new PointHitTestParameters(e.GetPosition(TopCanvas)));
        }

        private HitTestResultBehavior noFocusCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(Rectangle))
            {
                if ((result.VisualHit as Rectangle).Name == "HitBox")
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

    }
}
