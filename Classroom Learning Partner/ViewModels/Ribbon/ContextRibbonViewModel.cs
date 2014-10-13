using System.Collections.ObjectModel;
using System.Windows;
using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ContextRibbonViewModel : ViewModelBase
    {
        public ContextRibbonViewModel()
        {

        }

        #region Bindings

        /// <summary>List of the buttons currently on the Ribbon.</summary>
        public ObservableCollection<UIElement> Buttons
        {
            get { return GetValue<ObservableCollection<UIElement>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons",
                                                                               typeof(ObservableCollection<UIElement>),
                                                                               () => new ObservableCollection<UIElement>());

        #endregion //Bindings
    }
}