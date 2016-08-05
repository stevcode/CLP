using System.Collections.Generic;
using System.Linq;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;

namespace Classroom_Learning_Partner
{
    public static class ViewModelBaseExtensions
    {
        public static List<IView> GetAllViews(this IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(viewModel);

            return views.ToList();
        }

        public static IView GetFirstView(this IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var views = viewModel.GetAllViews();

            return views.Any() ? views.First() : null;
        }
    }
}