using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ContextRibbonViewModel : ViewModelBase
    {
        public ContextRibbonViewModel()
        {
            if (App.MainWindowViewModel.MajorRibbon.PageInteractionMode == PageInteractionModes.Pen)
            {
                SetPenContextButtons();
            }
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

        #region Methods

        public ObservableCollection<UIElement> CurrentPenColors = new ObservableCollection<UIElement>(); 
        public void SetPenContextButtons()
        {
            if (!CurrentPenColors.Any())
            {
                CurrentPenColors.Add(new ColorButton(Colors.Black));
                CurrentPenColors.Add(new ColorButton(Colors.Red));
                CurrentPenColors.Add(new ColorButton(Colors.DarkOrange));
                CurrentPenColors.Add(new ColorButton(Colors.Tan));
                CurrentPenColors.Add(new ColorButton(Colors.Gold));
                CurrentPenColors.Add(new ColorButton(Colors.DarkGreen));
                CurrentPenColors.Add(new ColorButton(Colors.Blue));
                CurrentPenColors.Add(new ColorButton(Colors.HotPink));
                CurrentPenColors.Add(new ColorButton(Colors.BlueViolet));
                CurrentPenColors.Add(new ColorButton(Colors.LightSlateGray));
            }

            Buttons.Clear();

            Buttons.Add(new RibbonButton("Pen Size", "pack://application:,,,/Resources/Images/PenSize32.png", null, null, true));
            Buttons.Add(new RibbonButton("Highlighter", "pack://application:,,,/Resources/Images/Highlighter32.png", null, null, true));
            Buttons.Add(new RibbonButton("Eraser", "pack://application:,,,/Resources/Images/StrokeEraser32.png", null, null, true));

            Buttons.Add(MajorRibbonViewModel.Separater);

            Buttons.AddRange(CurrentPenColors);
        }

        #endregion //Methods
    }
}