using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

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
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageViewModel);
        }

        void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach(ICLPPageObject pageObject in e.NewItems)
                {
                    PageObjectContainerView pageObjectContainerView = new PageObjectContainerView();
                    pageObjectContainerView.DataContext = pageObject;
                    MainInkCanvas.Children.Add(pageObjectContainerView);
                }
            }

            if(e.OldItems != null)
            {
                List<PageObjectContainerView> viewsToRemove = new List<PageObjectContainerView>();
                foreach(ICLPPageObject pageObject in e.OldItems)
                {
                    foreach(PageObjectContainerView pageObjectView in MainInkCanvas.Children)
                    {
                        if(pageObject.UniqueID == (pageObjectView.ViewModel as ACLPPageObjectBaseViewModel).PageObject.UniqueID)
                        {
                            viewsToRemove.Add(pageObjectView);
                        }
                    }
                }
                foreach(PageObjectContainerView pageObjectView in viewsToRemove)
                {
                    MainInkCanvas.Children.Remove(pageObjectView);
                }
            }
        }

        [ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewModelToView)]
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return (ObservableCollection<ICLPPageObject>)GetValue(PageObjectsProperty); }
            set
            {
                PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
                MainInkCanvas.Children.Clear();
                SetValue(PageObjectsProperty, value);
                foreach(ICLPPageObject pageObject in PageObjects)
                {
                    PageObjectContainerView pageObjectContainerView = new PageObjectContainerView();
                    pageObjectContainerView.DataContext = pageObject;
                    MainInkCanvas.Children.Add(pageObjectContainerView);
                }
                PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            }
        }

        // Using a DependencyProperty as the backing store for PageObjectsProperty.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageObjectsProperty =
            DependencyProperty.Register("PageObjects",
            typeof(ObservableCollection<ICLPPageObject>), typeof(CLPPagePreviewView), new UIPropertyMetadata(new ObservableCollection<ICLPPageObject>()));

    }
}
