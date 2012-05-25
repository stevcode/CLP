﻿using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Windows.Controls;
using System.Windows.Shapes;
using System;
using System.Windows;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class CLPShadingRegionView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPGridView"/> class.
        /// </summary>
        public CLPShadingRegionView()
        {
            InitializeComponent();
        }

        private void SetupGrid(object sender, DependencyPropertyChangedEventArgs e)
        {
            for (int i = 0; i < this.Grid.Rows * this.Grid.Columns; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = System.Windows.Media.Brushes.Black;
                rect.StrokeThickness = 0.5;
                this.Grid.Children.Add(rect);
            }
        }

        public void ShowContentsCommand(object sender, RoutedEventArgs e)
        {
            string result = (DataContext as CLPShadingRegionViewModel).GetStringRepresentation();
            MessageBox.Show(result);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPShadingRegionViewModel);
        }
    }
}