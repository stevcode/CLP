using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

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
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        public static readonly Dictionary<int, IViewModel> ModelViewModels = new Dictionary<int, IViewModel>();
        protected override IViewModel GetViewModelInstance(object dataContext)
        {
            if(dataContext == null)
            {
                // Let catel handle this one
                return null;
            }

            if(!CLPPagePreviewView.ModelViewModels.ContainsKey(dataContext.GetHashCode()))
            {
                var vm = new CLPPageViewModel(dataContext as CLPPage);
                CLPPagePreviewView.ModelViewModels.Add(dataContext.GetHashCode(), vm);
            }

            // Reuse VM
            return CLPPagePreviewView.ModelViewModels[dataContext.GetHashCode()];
        }
    
    }
}
