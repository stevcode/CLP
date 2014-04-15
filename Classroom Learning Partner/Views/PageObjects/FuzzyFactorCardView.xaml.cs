﻿using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for FuzzyFactorCardView.xaml
    /// </summary>
    public partial class FuzzyFactorCardView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzyFactorCardView" /> class.
        /// </summary>
        public FuzzyFactorCardView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(FuzzyFactorCardViewModel); }
    }
}