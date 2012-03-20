﻿using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView : Catel.Windows.Controls.UserControl
    {
        public CLPPagePreviewView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }
    }
}
