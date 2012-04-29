using Classroom_Learning_Partner.ViewModels;
using System;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView : Catel.Windows.Controls.UserControl
    {
        public CLPPagePreviewView()
        {
            InitializeComponent();
            DataContextChanged += new System.Windows.DependencyPropertyChangedEventHandler(CLPPagePreviewView_DataContextChanged);
            Console.WriteLine(MainInkCanvas.Strokes.Count.ToString());
        }

        void CLPPagePreviewView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                Console.WriteLine((this.ViewModel as CLPPageViewModel).InkStrokes.Count.ToString());
                Console.WriteLine(MainInkCanvas.Strokes.Count.ToString());
            }
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }
    }
}
