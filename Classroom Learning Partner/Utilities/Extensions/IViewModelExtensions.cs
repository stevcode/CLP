using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Services;

namespace Classroom_Learning_Partner
{
    public static class IViewModelExtensions
    {
        public static List<IView> GetAllViews(this IViewModel viewModel)
        {
            Argument.IsNotNull(() => viewModel);

            var dependencyResolver = viewModel.GetDependencyResolver();
            var viewManager = dependencyResolver.Resolve<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(viewModel).ToList();

            return views;
        }

        public static IView GetFirstView(this IViewModel viewModel)
        {
            Argument.IsNotNull(() => viewModel);

            var views = viewModel.GetAllViews();

            return views.Any() ? views.First() : null;
        }

        public static async Task<bool?> ShowWindowAsync(this IViewModel viewModel)
        {
            Argument.IsNotNull(() => viewModel);

            var dependencyResolver = viewModel.GetDependencyResolver();
            var uiVisualerService = dependencyResolver.Resolve<IUIVisualizerService>();

            return await uiVisualerService.ShowAsync(viewModel);
        }

        public static async Task<bool?> ShowWindowAsDialogAsync(this IViewModel viewModel)
        {
            Argument.IsNotNull(() => viewModel);

            var dependencyResolver = viewModel.GetDependencyResolver();
            var uiVisualerService = dependencyResolver.Resolve<IUIVisualizerService>();

            return await uiVisualerService.ShowDialogAsync(viewModel);
        }

        public static T CreateViewModel<T>(this IViewModel viewModel, object dataContext) where T : ViewModelBase
        {
            Argument.IsNotNull(() => viewModel);

            var dependencyResolver = viewModel.GetDependencyResolver();
            var viewModelFactory = dependencyResolver.Resolve<IViewModelFactory>();

            return viewModelFactory.CreateViewModel<T>(dataContext);
        }
    }
}