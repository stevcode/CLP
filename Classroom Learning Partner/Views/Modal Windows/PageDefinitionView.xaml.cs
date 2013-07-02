using System;
using System.Windows;
using System.Windows.Controls;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for PageDefinitionView.xaml.
    /// </summary>
    public partial class PageDefinitionView : DataWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageDefinitionView"/> class.
        /// </summary>
        public PageDefinitionView(ProductRelationViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
