using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStrokePathContainerView.xaml
    /// </summary>
    public partial class CLPStrokePathContainerView : Catel.Windows.Controls.UserControl
    {
        public CLPStrokePathContainerView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStrokePathContainerViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPStrokePathContainerViewModel).PageObject;

            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (ViewModel as CLPStrokePathContainerViewModel).PageObject;

            double x = pageObject.XPosition + e.HorizontalChange;
            double y = pageObject.YPosition + e.VerticalChange;
            if(x < 0)
            {
                x = 0;
            }
            if(y < 0)
            {
                y = 0;
            }
            if(x > 1056 - pageObject.Width)
            {
                x = 1056 - pageObject.Width;
            }
            if(y > 816 - pageObject.Height)
            {
                y = 816 - pageObject.Height;
            }

            //Console.WriteLine("PageObject pageObjects moving: " + pageObject.PageObjectObjectParentIDs.Count);
            if (pageObject.PageObjectObjectParentIDs.Count > 0)
            {
                double xDelta = x - pageObject.XPosition;
                double yDelta = y - pageObject.YPosition;

                ObservableCollection<ICLPPageObject> pageObjectsOverPageObject = pageObject.GetPageObjectsOverPageObject();
                foreach (ICLPPageObject po in pageObjectsOverPageObject)
                {
                    //Console.WriteLine(po.UniqueID);
                    Point pageObjectPt = new Point((xDelta + po.XPosition), (yDelta + po.YPosition));
                    Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(po, pageObjectPt);
                }
            }

            Point pt = new Point(x, y);
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pt);
        }

    }
}
