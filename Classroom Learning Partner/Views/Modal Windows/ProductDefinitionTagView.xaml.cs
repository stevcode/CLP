using System.Windows;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for ProductDefinitionTagView.xaml.
    /// </summary>
    public partial class ProductDefinitionTagView : DataWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDefinitionTagView" /> class.
        /// </summary>
        public ProductDefinitionTagView(ProductDefinitionTagViewModel viewModel)
            : base(viewModel) { InitializeComponent(); }

        private void FirstFactor_OnChecked(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel as ProductDefinitionTagViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.UngivenProductPart = ProductPart.FirstFactor;
        }

        private void SecondFactor_OnChecked(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel as ProductDefinitionTagViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.UngivenProductPart = ProductPart.SecondFactor;
        }

        private void Product_OnChecked(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel as ProductDefinitionTagViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.UngivenProductPart = ProductPart.Product;
        }
    }
}