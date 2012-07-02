using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.ViewModels;
using System;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Views.Workspaces
{

    /// <summary>
    /// Interaction logic for NotebookWorkspaceView.xaml.
    /// </summary>
    public partial class NotebookWorkspaceView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceView"/> class.
        /// </summary>
        public NotebookWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookWorkspaceViewModel);
        }

        private ToggleButton currentToggledButton = null;
        private void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if(currentToggledButton != null && currentToggledButton != button)
            {
                currentToggledButton.IsChecked = false;
                ((currentToggledButton.Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
            }
            currentToggledButton = button;
            if((bool)currentToggledButton.IsChecked)
            {
                SubmissionsSideBar.Visibility = Visibility.Visible;
                CLPPage page = (((((sender as ToggleButton).Parent as Grid).Parent as Grid).Children[0] as Border).Child as ContentPresenter).Content as CLPPage;
                string pageID = page.UniqueID;
                var viewModel = this.ViewModel as NotebookWorkspaceViewModel;
                if(viewModel.Notebook.Submissions.ContainsKey(pageID))
                {
                    viewModel.SubmissionPages = viewModel.Notebook.Submissions[pageID];
                }
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Lavender);
            }
            else
            {
                SubmissionsSideBar.Visibility = Visibility.Collapsed;
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
            }

        }

        private void toggle_Click(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                if(item != vsp.Children[vsp.Children.Count - 1])
                {
                    if((bool)(sender as ToggleButton).IsChecked)
                    {
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                        item.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if(child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if(child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void allItems_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=
                if(item != vsp.Children[vsp.Children.Count - 1])
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }


        private void SortComboBox_SelectionChanged(object seender, SelectionChangedEventArgs e)
        {
            SetNewSortOrder();
        }


        private void SetNewSortOrder()
        {
            string newSortOrder = ((ComboBoxItem)SortComboBox.SelectedItem).Name;
            SortDescription sortDesc = new SortDescription(newSortOrder, ListSortDirection.Ascending);
            CollectionViewSource src = (CollectionViewSource)FindResource("SortedCollectionView");
            src.SortDescriptions.Clear();
            src.SortDescriptions.Add(sortDesc);
        }


        //Making use of sorts defined in SortDescriptions
        //ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
        //if (cvTasks != null && cvTasks.CanSort == true)
        //{
        //        .SortDescriptions.Clear();
        //    cvTasks.SortDescriptions.Add(new SortDescription("NameSortedCollectionView", ListSortDirection.Ascending));
        //    cvTasks.SortDescriptions.Add(new SortDescription("Complete", ListSortDirection.Ascending));
        //    cvTasks.SortDescriptions.Add(new SortDescription("DueDate", ListSortDirection.Ascending));
        //}



        //#region Sorting (Bindings)

        //private List<Category> _alphabeticalSortCategories;
        //public List<Category> AlphabeticalSortCategories
        //{
        //    get
        //    {
        //        if(_alphabeticalSortCategories == null)
        //        {
        //            _alphabeticalSortCategories = new List<Category>();
        //        }
        //        return _alphabeticalSortCategories;
        //    }
        //    set
        //    {
        //        _alphabeticalSortCategories = value;
        //        base.OnPropertyChanged("AlphabeticalSortCategories");
        //    }
        //}

        //private Category _alphabeticalSortingCondition;
        //public Category AlphabeticalSortingCondition
        //{
        //    get
        //    {
        //        if(_alphabeticalSortingCondition == null)
        //        {
        //            _alphabeticalSortingCondition = new Category("Time in", new List<CategoryValue>(), false);
        //        }
        //        return _alphabeticalSortingCondition;
        //    }
        //    set
        //    {
        //        _alphabeticalSortingCondition = value;
        //        AlphabeticalSortingConditionChanged();
        //        base.OnPropertyChanged("AlphabeticalSortingCondition");
        //    }
        //}
        //#endregion

        //#region Sorting (Methods)

        //void AlphabeticalSortingConditionChanged()
        //{
        //    if(AphabeticalSortingCondition.Name == "Alphabetical Order")
        //    {
        //        SubmissionsGroup.RemoveSubGroups();
        //    }
        //    else { UpdateGroupsWithNewFirstCondition(AlphabeticalSortingCondition); }

        //    //if(!(SecondSortingCondition.Name == "")) SecondSortingConditionChanged();
        //}
        //#endregion //Sorting (Methods)

        //#region Groups (Methods)

        //void UpdateGroupsWithNewFirstCondition(Category condition)
        //{
        //    List<string> categoryValues = new List<string>();
        //    foreach(CategoryValue value in condition.Values)
        //    {
        //        categoryValues.Add(value.Text);
        //    }
        //    categoryValues.Sort();
        //    SubmissionsGroup.CreateNewSubGroups(condition.Name, categoryValues);
        //}
        //#endregion
    }
}
