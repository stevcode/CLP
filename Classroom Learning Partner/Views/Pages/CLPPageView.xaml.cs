using System;
using Catel.IoC;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPageView.xaml
    /// </summary>
    public partial class CLPPageView
    {
        public CLPPageView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(ACLPPageBaseViewModel); }

        protected override Type GetViewModelType(object dataContext)
        {
            var page = dataContext as CLPPage;
            if(page == null)
            {
                return null;
            }

            switch(page.PageType)
            {
                case PageTypes.Default:
                    return typeof(CLPPageViewModel);
                case PageTypes.Animation:
                    return typeof(CLPAnimationPageViewModel);
                default:
                    return null;
            }
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is ACLPPageBaseViewModel)
            {
                (ViewModel as ACLPPageBaseViewModel).TopCanvas = TopCanvas;
                (ViewModel as ACLPPageBaseViewModel).IsPagePreview = false;
                var pageInteractionService = ServiceLocator.Default.ResolveType<IPageInteractionService>();
                pageInteractionService.ActivePageViewModels.Clear();
                pageInteractionService.ActivePageViewModels.Add(ViewModel as ACLPPageBaseViewModel);
            }

            base.OnViewModelChanged();
        }
    }
}