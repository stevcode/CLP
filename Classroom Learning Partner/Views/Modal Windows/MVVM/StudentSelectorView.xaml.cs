using System;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StudentSelectorView.xaml
    /// </summary>
    public partial class StudentSelectorView : DataWindow
    {
        public StudentSelectorView(StudentSelectorViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
