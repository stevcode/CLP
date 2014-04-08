using System;
using System.Windows;
using System.Windows.Controls;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

// TODO (someday): Automatically populate combobox with all possible relation types

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

            //ProductRelation.ProductRelationTypes type = viewModel.RelationType;
            //switch(type)
            //{
            //    case ProductRelation.ProductRelationTypes.GenericProduct:
            //        TypeComboBox.SelectedValue = "Generic Product";
            //        break;
            //    case ProductRelation.ProductRelationTypes.EqualGroups:
            //        TypeComboBox.SelectedValue = "Equal Groups";
            //        break;
            //    case ProductRelation.ProductRelationTypes.Area:
            //        TypeComboBox.SelectedValue = "Area";
            //        break;
            //    default:
            //        break;
            //}
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This condition will fire when the dialog is first loaded
            if(TypeComboBox.SelectedValue == null) { return; }

            var newType = TypeComboBox.SelectedValue.ToString();

            //if (newType.Equals("Generic Product"))
            //{
            //    ((ProductRelationViewModel)this.ViewModel).RelationType = ProductRelation.ProductRelationTypes.GenericProduct;
            //}
            //else if(newType.Equals("Equal Groups"))
            //{
            //    ((ProductRelationViewModel)this.ViewModel).RelationType = ProductRelation.ProductRelationTypes.EqualGroups;
            //}
            //else if(newType.Equals("Area"))
            //{
            //    ((ProductRelationViewModel)this.ViewModel).RelationType = ProductRelation.ProductRelationTypes.Area;
            //}
            //else
            //{
            //    Logger.Instance.WriteToLog("Something went horribly wrong");
            //}
        }
    }
}
