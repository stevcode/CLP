using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SideBarView.xaml
    /// </summary>
    public partial class SideBarView : UserControl
    {
        public SideBarView()
        {
            InitializeComponent();
        }

        private void SideBarToggle_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SideBarBorder.Visibility == System.Windows.Visibility.Collapsed)
            {
                SideBarBorder.Visibility = System.Windows.Visibility.Visible;
                SideBarToggle.Content = "<";
            }
            else
            {
                SideBarBorder.Visibility = System.Windows.Visibility.Collapsed;
                SideBarToggle.Content = ">";
            }
        }

        private void ToggleSubmissionsSideBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SubmissionsSideBar.Visibility == System.Windows.Visibility.Collapsed)
            {
                SubmissionsSideBar.Visibility = System.Windows.Visibility.Visible;
                TempRectangle.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                SubmissionsSideBar.Visibility = System.Windows.Visibility.Collapsed;
                TempRectangle.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CLPPagePreviewView pagePreviewView = (((sender as Button).Parent as Grid).Parent as Grid).Children[0] as CLPPagePreviewView;
            CLPPageViewModel pageViewModel = pagePreviewView.DataContext as CLPPageViewModel;
            string pageID = pageViewModel.Page.UniqueID;
            if (App.CurrentNotebookViewModel.SubmissionViewModels.ContainsKey(pageID))
            {
                SideBarViewModel sideBarViewModel = this.DataContext as SideBarViewModel;
                sideBarViewModel.SubmissionViewModels = App.CurrentNotebookViewModel.SubmissionViewModels[pageID];
            }
        }
    }
}
