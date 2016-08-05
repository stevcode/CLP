using System.Collections.Generic;
using System.Linq;
using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;

namespace Classroom_Learning_Partner
{
    public static class IModelExtensions
    {
        public static List<IViewModel> GetAllViewModels(this IModel model)
        {
            Argument.IsNotNull("model", model);

            var viewModelManger = ServiceLocator.Default.ResolveType<IViewModelManager>();
            var viewModels = viewModelManger.GetViewModelsOfModel(model).ToList();

            return viewModels;
        }

        public static IViewModel GetFirstViewModel(this IModel model)
        {
            Argument.IsNotNull("model", model);

            var viewModels = model.GetAllViewModels();

            return viewModels.Any() ? viewModels.First() : null;
        }
    }
}