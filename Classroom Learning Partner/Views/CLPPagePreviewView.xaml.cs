using System;
using System.Collections.Generic;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView : Catel.Windows.Controls.UserControl
    {
        static public int count = 0;
        public int ID;

        public CLPPagePreviewView()
        {
            count++;
            ID = count;
            Console.WriteLine("PagePreviewView Created with ID: " + ID);
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();

            var clpPageViewModel = ViewModel as CLPPageViewModel;
            if (clpPageViewModel != null)
            {
                Console.WriteLine("PagePreviewVIEW ID " + ID + " viewModel has changed to ID " + clpPageViewModel.ID);
            }
            else
            {
                Console.WriteLine("PagePreviewVIEW ID " + ID + " has a viewModel set to null");
            }
        }

        public static readonly Dictionary<int, IViewModel> ModelViewModels = new Dictionary<int, IViewModel>();
        //protected override IViewModel GetViewModelInstance(object dataContext)
        //{
        //    if(dataContext == null)
        //    {
        //        // Let catel handle this one
        //        return null;
        //    }

        //    if(!ModelViewModels.ContainsKey(dataContext.GetHashCode()))
        //    {
        //        var vm = new CLPPageViewModel(dataContext as CLPPage);
        //        ModelViewModels.Add(dataContext.GetHashCode(), vm);
        //    }

        //    // Reuse VM
        //    return ModelViewModels[dataContext.GetHashCode()];
        //}
    
    }
}
