using System.Collections.Generic;
using System.Linq;
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
            Argument.IsNotNull("viewModel", viewModel);

            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(viewModel).ToList();

            return views;
        }

        public static IView GetFirstView(this IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var views = viewModel.GetAllViews();

            return views.Any() ? views.First() : null;
        }

        public static bool? ShowWindow(this IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var uiVisualerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            return uiVisualerService.Show(viewModel);
        }

        public static bool? ShowWindowAsDialog(this IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var uiVisualerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            return uiVisualerService.ShowDialog(viewModel);
        }
    }
}