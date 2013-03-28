using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : Catel.Windows.Controls.UserControl
    {
        static public int count = 0;
        public int ID;

        public CLPPageView()
        {
            count++;
            ID = count;
            Console.WriteLine("I am a PageVIEW, my ID is " + ID);
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPPageViewModel)
            {
                (ViewModel as CLPPageViewModel).TopCanvas = TopCanvas;
            }
            
            base.OnViewModelChanged();

            var clpPageViewModel = ViewModel as CLPPageViewModel;
            if (clpPageViewModel != null)
            {
                Console.WriteLine("PageVIEW ID " + ID + " viewModel has changed to ID " + clpPageViewModel.ID);
            }
            else
            {
                Console.WriteLine("PageVIEW ID " + ID + " has a viewModel set to null");
            }
        }

        //protected override IViewModel GetViewModelInstance(object dataContext)
        //{
        //    if(dataContext == null)
        //    {
        //        // Let catel handle this one
        //        return null;
        //    }

        //    if(!CLPPagePreviewView.ModelViewModels.ContainsKey(dataContext.GetHashCode()))
        //    {
        //        var vm = new CLPPageViewModel(dataContext as CLPPage);
        //        CLPPagePreviewView.ModelViewModels.Add(dataContext.GetHashCode(), vm);
        //    }

        //    // Reuse VM
        //    return CLPPagePreviewView.ModelViewModels[dataContext.GetHashCode()];
        //}
    }
}
