using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.Views
{
    public class BindableInkCanvas : InkCanvas
    {

        //static BindableInkCanvas()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(BindableInkCanvas), new FrameworkPropertyMetadata(typeof(BindableInkCanvas)));
        //}

        public BindableInkCanvas()
            : base()
        {
        }

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

        //Called when a new value is set (through binding for example)
        protected static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //Gets the instance that changed the "local" value
            var instance = sender as BindableInkCanvas;

            ObservableCollection<Catel.Windows.Controls.UserControl> newCollection = args.NewValue as ObservableCollection<Catel.Windows.Controls.UserControl>;
            ObservableCollection<Catel.Windows.Controls.UserControl> oldCollection = args.OldValue as ObservableCollection<Catel.Windows.Controls.UserControl>;

            if(oldCollection != null)
            {
                oldCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);
            }

            //Clears all the previous children in the collection
            instance.Children.Clear();

            if(newCollection != null)
            {
                foreach(Catel.Windows.Controls.UserControl item in newCollection)
                {
                    AddControl(item, instance);
                }

                newCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(instance.collection_CollectionChanged);
            }
        }

        //Append when an Item in the collection is changed
        protected void collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(Catel.Windows.Controls.UserControl item in e.NewItems)
            {
                AddControl(item);
            }
        }

        protected void AddControl(Catel.Windows.Controls.UserControl diagramItem)
        {
            AddControl(diagramItem, this);
        }

        protected static void AddControl(Catel.Windows.Controls.UserControl diagramItem, InkCanvas parentControl)
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
