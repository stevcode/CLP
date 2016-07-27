using Catel.IoC;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPPageView.xaml</summary>
    public partial class CLPPageView
    {
        public CLPPageView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged()
        {
            if (ViewModel is ACLPPageBaseViewModel)
            {
                (ViewModel as ACLPPageBaseViewModel).TopCanvas = TopCanvas;
                (ViewModel as ACLPPageBaseViewModel).IsPagePreview = false;
                var pageInteractionService = ServiceLocator.Default.ResolveType<IPageInteractionService>();
                pageInteractionService.ActivePageViewModels.Clear();
                pageInteractionService.ActivePageViewModels.Add(ViewModel as ACLPPageBaseViewModel);
                pageInteractionService.SetPageInteractionMode(pageInteractionService.CurrentPageInteractionMode);
            }

            base.OnViewModelChanged();
        }
    }
}