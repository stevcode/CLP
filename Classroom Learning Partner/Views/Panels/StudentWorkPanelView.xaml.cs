using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StudentWorkPanelView.xaml
    /// </summary>
    public partial class StudentWorkPanelView
    {
        public StudentWorkPanelView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(StudentWorkPanelViewModel);
        }
    }
}
