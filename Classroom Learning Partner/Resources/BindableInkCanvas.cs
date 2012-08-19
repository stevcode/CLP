using System.Windows.Controls;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Resources
{
    //Not Usable until possibly Catel 3.1 upgrade
    public class BindableInkCanvas : InkCanvas
    {

        //Just a simple INotifyCollectionChanged collection
        public ObservableCollection<Catel.Windows.Controls.UserControl> Source
        {
            get { return (ObservableCollection<Catel.Windows.Controls.UserControl>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }


        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
            typeof(ObservableCollection<Catel.Windows.Controls.UserControl>),
            typeof(BindableInkCanvas),
            new FrameworkPropertyMetadata(new ObservableCollection<Catel.Windows.Controls.UserControl>(),
            new PropertyChangedCallback(SourceChanged)));

        //called when a new value is set (through binding for example)
        protected static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //gets the instance that changed the "local" value
            var instance = sender as BindableInkCanvas;
            //the new collection that will be set
            ObservableCollection<Catel.Windows.Controls.UserControl> newCollection = args.NewValue as ObservableCollection<Catel.Windows.Controls.UserControl>;
            //the previous collection that was set
            ObservableCollection<Catel.Windows.Controls.UserControl> oldCollection = args.OldValue as ObservableCollection<Catel.Windows.Controls.UserControl>;

            if (oldCollection != null)
            {
                //removes the CollectionChangedEventHandler from the old collection
                oldCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);
            }

            //clears all the previous children in the collection
            instance.Children.Clear();

            if (newCollection != null)
            {
                //adds all the children of the new collection
                foreach (Catel.Windows.Controls.UserControl item in newCollection)
                {
                    AddControl(item, instance);
                }

                //adds a new CollectionChangedEventHandler to the new collection
                newCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);

            }

        }


        //append when an Item in the collection is changed
        protected void collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //adds the new items in the children collection
            foreach (Catel.Windows.Controls.UserControl item in e.NewItems)
            {
                AddControl(item);
            }
        }


        protected static void AddControl(Catel.Windows.Controls.UserControl diagramItem, InkCanvas parentControl)
        {
            Catel.Windows.Controls.UserControl ret = new Catel.Windows.Controls.UserControl();
            //ret.DataContext = diagramItem;
            parentControl.Children.Add(ret);

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

        protected void AddControl(Catel.Windows.Controls.UserControl diagramItem)
        {
            AddControl(diagramItem, this);
        }


        static BindableInkCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BindableInkCanvas), new FrameworkPropertyMetadata(typeof(BindableInkCanvas)));
        }

        public BindableInkCanvas()
            : base()
        {

        }

    }
}
