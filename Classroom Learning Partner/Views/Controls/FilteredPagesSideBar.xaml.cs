using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

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

        private void toggle_Click2(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                if(item != vsp.Children[0])
                {
                    if((bool)(sender as ToggleButton).IsChecked)
                    {
                      //  ((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                       // ((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

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

        private void toggle_Click3(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);

            foreach(UIElement item in vsp.Children)
            {
                    if((bool)(sender as ToggleButton).IsChecked)
                    {
                        //  ((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        // ((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                       if(item == vsp.Children[0])
                        {
                            //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                            //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                            var label = GetVisualChild<ToggleButton>(item);
                            label.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if(item != vsp.Children[0])
                        {
                            //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                            //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            var label = GetVisualChild<ToggleButton>(item);
                            label.Visibility = Visibility.Collapsed;

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
            }
        }
        private void allItems_Loaded3(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<WrapPanel>(itemsPresenter);
            var first = (Visual)vsp.Children[0];
            var label = GetVisualChild<ToggleButton>(first);
            label.Visibility = Visibility.Collapsed;
            foreach(UIElement item in vsp.Children)
            {
                if(item != vsp.Children[0])
                {
                    //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=

                    item.Visibility = Visibility.Collapsed;
                }

            }
        }
    }
}
