using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    public class BindableInkCanvas : InkCanvas
    {
        public BindableInkCanvas()
            : base()
        {
        }

        public ObservableCollection<PageObjectContainerView> Source
        {
            get { return (ObservableCollection<PageObjectContainerView>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
            typeof(ObservableCollection<PageObjectContainerView>),
            typeof(BindableInkCanvas),
            new FrameworkPropertyMetadata(new ObservableCollection<PageObjectContainerView>(),
            new PropertyChangedCallback(SourceChanged)));

        //Called when a new value is set (through binding for example)
        protected static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //Gets the instance that changed the "local" value
            var instance = sender as BindableInkCanvas;



            ObservableCollection<PageObjectContainerView> newCollection = args.NewValue as ObservableCollection<PageObjectContainerView>;
            ObservableCollection<PageObjectContainerView> oldCollection = args.OldValue as ObservableCollection<PageObjectContainerView>;

            if(oldCollection != null)
            {
                oldCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);
            }

            //Clears all the previous children in the collection
            instance.Children.Clear();

            if(newCollection != null)
            {
                foreach(PageObjectContainerView item in newCollection)
                {
                    AddControl(item, instance);
                }

                newCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);
            }
        }

        //Append when an Item in the collection is changed
        protected void collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(PageObjectContainerView item in e.NewItems)
            {
                AddControl(item);
            }
        }

        protected void AddControl(PageObjectContainerView diagramItem)
        {
            AddControl(diagramItem, this);
        }

        protected static void AddControl(PageObjectContainerView diagramItem, InkCanvas parentControl)
        {
            Catel.Windows.Controls.UserControl ret = new Catel.Windows.Controls.UserControl();

            parentControl.Children.Add(diagramItem);

            //ret.DataContext = diagramItem;
            //parentControl.Children.Add(ret);

            //binds to the control to the properties X and Y of the viewModel
            //Binding XBinding = new Binding("X");
            //XBinding.Source = diagramItem.Position.X;
            //XBinding.Mode = BindingMode.TwoWay;

            //Binding YBinding = new Binding("Y");
            //YBinding.Source = diagramItem.Position.Y;
            //YBinding.Mode = BindingMode.TwoWay;

            //ret.SetBinding(InkCanvas.LeftProperty, XBinding);
            //ret.SetBinding(InkCanvas.TopProperty, YBinding);
        }

    }
}
