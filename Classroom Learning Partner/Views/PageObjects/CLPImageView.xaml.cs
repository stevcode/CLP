﻿using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPImageView.xaml
    /// </summary>
    public partial class CLPImageView : Catel.Windows.Controls.UserControl
    {
        public CLPImageView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPImageViewModel);
        }
       
    }
}
