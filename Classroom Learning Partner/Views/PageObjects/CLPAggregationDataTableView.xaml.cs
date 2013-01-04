﻿using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPAggregationDataTableView.xaml.
    /// </summary>
    public partial class CLPAggregationDataTableView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPAggregationDataTableView"/> class.
        /// </summary>
        public CLPAggregationDataTableView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPAggregationDataTableViewModel);
        }
    }
}