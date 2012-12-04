using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;
using System.Threading;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStampViewModel);
        }

        private void StampHandleHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Green);
        }

        private void StampHandleHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if(ViewModel != null)
            {
                (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Black);
            }
        }

        private void AdornerClose_Click(object sender, RoutedEventArgs e)
        {
            //foreach(CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(PageObject.ParentPage))
            //{
            //    pageVM.IsInkCanvasHitTestVisible = true;
            //}
            CLPServiceAgent.Instance.RemovePageObjectFromPage((ViewModel as CLPStampViewModel).PageObject);
        }
        /*
            if (HasParts()) {
                (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Green);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        CopyStamp();
                        return null;
                    }, null);
            } else {
                MessageBox.Show("What are you counting on the stamp?  Please write the number on the line below the stamp before making copies.", "What are you counting?");
            }
            if (HasParts())
            {
        }
            }*/
    }
}
