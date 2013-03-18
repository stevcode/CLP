using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for FilteredPagesSideBar.xaml.
    /// </summary>
    public partial class FilteredPagesSideBar : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredPagesSideBar"/> class.
        /// </summary>
        public FilteredPagesSideBar()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookWorkspaceViewModel);
        }

        private void toggle_Click_student_name(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {

                var bord = GetVisualChild<Border>(item);
                var border = bord as Border;
                if(border != null)
                {
                    if((bool)(sender as ToggleButton).IsChecked)
                    {

                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        hide_stacks(border, e);
                    }
                    else
                    {
                        show_stacks(border, true);
                    }

                }
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

        private void toggle_Click_Group_Individual(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                var bord = GetVisualChild<Border>(item);
                var border = bord as Border;
                    if((bool)(sender as ToggleButton).IsChecked)
                    {
                        if(border != null)
                        {

                            hide_stacks(border, e);
                        }
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if(border != null)
                        {

                            show_stacks(border, true);
                        }
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                        item.Visibility = Visibility.Collapsed;
                    }
                
            }
        }

        private void toggle_Click_group_group(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            var bord1 = GetVisualChild<Border>(sender as ToggleButton);
            var border1 = bord1 as Border;
            if(border1 != null)
            {

                show_stacks_for_group(border1, e);


            }

            foreach(UIElement item in vsp.Children)
            {
                if((bool)(sender as ToggleButton).IsChecked)
                {
                    if(item == vsp.Children[0])
                    {
                        //  ((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        // ((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {

                        item.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {


                    //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                    //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                    item.Visibility = Visibility.Collapsed;

                }
            }
        }

     


        private void allItems_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                var border = GetVisualChild<Border>(item);
                if(border != null)
                {
                    //var grid = GetVisualParent<Grid>(border);
                   // var stack = grid.Children[0];
                  //  stack.Visibility = Visibility.Visible;
                    show_stacks(border, true);
                }
                //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=
                if(item != vsp.Children[vsp.Children.Count - 1])
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void show_stacks(object sender, bool group)
        {
            var border = sender as Border;
            var grid = GetVisualParent<Grid>(border);
            var stack = grid.Children[0];
            var itemsPresenter = GetVisualParent<ItemsPresenter>(border);
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            var count = vsp.Children.Count;
            if(count <= 1 && group)
            {
                stack.Visibility = Visibility.Collapsed;
            }
            else
            {
                stack.Visibility = Visibility.Visible;
            }
        }
        private void show_all_stacks(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            var grid = GetVisualParent<Grid>(border);
            var stack = grid.Children[0];
            
                stack.Visibility = Visibility.Visible;
            
        }


        private void show_stacks_for_group(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            var g1 = GetVisualParent<Grid>(border);
            var grid = GetVisualParent<Grid>(g1);
            var stack = grid.Children[0];
            var ip = GetVisualParent<ItemsPresenter>(border);
            var itemsPresenter = GetVisualParent<ItemsPresenter> (ip);
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            var count = vsp.Children.Count;
            if(count <= 0)
            {
                stack.Visibility = Visibility.Collapsed;
            }
            else
            {
                stack.Visibility = Visibility.Visible;
            }
        }

        private void hide_stacks(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            var grid = GetVisualParent<Grid>(border);
            var stack = grid.Children[0];
            var itemsPresenter = GetVisualParent<ItemsPresenter>(border);
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            var count = vsp.Children.Count;
     
                stack.Visibility = Visibility.Collapsed;
          
        }

        private void allItems_Loaded2(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
   
            foreach(UIElement item in vsp.Children)
            {
                //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=
                if(item != vsp.Children[0])
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var label = GetVisualChild<ToggleButton>(item);
                    if(label != null)
                    {
                        label.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private void GroupItemsLoaded(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            var toggles = GetVisualChildren<ToggleButton>(vsp);

            foreach(UIElement item in toggles)
            {

                //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=

                item.Visibility = Visibility.Collapsed;
            }


            foreach(UIElement item in vsp.Children)
            {
                var p = GetVisualChild<WrapPanel>(item);

                var b = GetVisualChild<Border>(p);
                var border = b as Border;
                if(border != null)
                {

                    var grid = GetVisualParent<Grid>(border);
                    var stack = grid.Children[0];
                    stack.Visibility = Visibility.Visible;

                }
                if(item != vsp.Children[0])
                {
                    //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=

                    item.Visibility = Visibility.Collapsed;
                }

            }
        }

        private void ShowAllSiblings(object sender, RoutedEventArgs e)
        {
            var parent = ((sender as ToggleButton).Parent as Grid);
            var grid = GetVisualParent<Grid>(parent);
            var real = GetVisualParent<Grid>(grid);
            var itemsPresenter = real.Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            bool senderIsFirst = false;
            var timeToggle = GetVisualChild<ToggleButton>(vsp); //gets you the time toggle button of first item
            var firstItemParent = (timeToggle.Parent as Grid).Children[1] as ItemsPresenter; //gets you item presenter of first gorup
            var firstToggle = GetVisualChild<ToggleButton>(firstItemParent); //should get you name toggle of first student
            var i = sender as ToggleButton;
            if(i == firstToggle)
            {
                senderIsFirst = true;
            }

            foreach(UIElement item in vsp.Children)
            {
                var border = GetVisualChild<Border>(item);
                var stack = (border.Parent as Grid).Children[0];
                if((bool)(sender as ToggleButton).IsChecked)
                {
                    stack.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stack.Visibility = Visibility.Visible;
                }
                if(senderIsFirst)
                {
                    if((bool)(sender as ToggleButton).IsChecked)
                    {
                        //  ((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        // ((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if(item != vsp.Children[0])
                        {

                            //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                            //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                            item.Visibility = Visibility.Collapsed;
                        }
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
        private static T GetVisualParent<T>(DependencyObject child) where T : Visual
        {
            T parent = default(T);
            Visual v = (Visual)VisualTreeHelper.GetParent(child);
            if(v == null)
            {
                return parent;
            }
            parent = v as T;

            if(parent == null)
            {
                parent = GetVisualParent<T>(v);
            }
            if(parent != null)
            {
                return parent;
            }
            return parent;

        }
        private static List<T> GetVisualChildren<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            List<T> children = new List<T>();

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;

                if(child != null)
                {
                    children.Add(child);
                }
                else
                {
                    children.Add(GetVisualChild<T>(v));
                }
            }
            return children;
        }
    }
}
