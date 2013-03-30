using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView : Catel.Windows.Controls.UserControl
    {
        public CLPPageView()
        {
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
