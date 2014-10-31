using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
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
            
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
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

        private readonly ObservableCollection<ColorButton> _currentPenColors = new ObservableCollection<ColorButton>(); 
        public void SetPenContextButtons()
        {
            if (!_currentPenColors.Any())
            {
                _currentPenColors.Add(new ColorButton(Colors.Black));
                _currentPenColors.Add(new ColorButton(Colors.White));
                _currentPenColors.Add(new ColorButton(Colors.Red));
                _currentPenColors.Add(new ColorButton(Colors.DarkOrange));
                _currentPenColors.Add(new ColorButton(Colors.Tan));
                _currentPenColors.Add(new ColorButton(Colors.Gold));
                _currentPenColors.Add(new ColorButton(Colors.SaddleBrown));
                _currentPenColors.Add(new ColorButton(Colors.DarkGreen));
                _currentPenColors.Add(new ColorButton(Colors.MediumSeaGreen));
                _currentPenColors.Add(new ColorButton(Colors.Blue));
                _currentPenColors.Add(new ColorButton(Colors.HotPink));
                _currentPenColors.Add(new ColorButton(Colors.BlueViolet));
                _currentPenColors.Add(new ColorButton(Colors.Aquamarine));
                _currentPenColors.Add(new ColorButton(Colors.SlateGray));
                _currentPenColors.Add(new ColorButton(Colors.SkyBlue));
                _currentPenColors.Add(new ColorButton(Colors.DeepSkyBlue));
                _currentPenColors.Add(new ColorButton(Color.FromRgb(0, 152, 247)));

                foreach (var colorButton in _currentPenColors)
                {
                    colorButton.Checked += colorButton_Checked;
                }
            }

            Buttons.Clear();

            Buttons.Add(new RibbonButton("Pen Size", "pack://application:,,,/Resources/Images/PenSize32.png", null, null, true));
            var highlighterButton = new ToggleRibbonButton("Highlighter", "Highlighter", "pack://application:,,,/Resources/Images/Highlighter32.png", true)
            {
                IsChecked = App.MainWindowViewModel.MajorRibbon.DrawingAttributes.IsHighlighter
            };
            highlighterButton.Checked += highlighterButton_Checked;
            highlighterButton.Unchecked += highlighterButton_Checked;
            Buttons.Add(highlighterButton);

            Buttons.Add(MajorRibbonViewModel.Separater);

            Buttons.AddRange(_currentPenColors);

            var currentColorButton = _currentPenColors.FirstOrDefault(x => App.MainWindowViewModel.MajorRibbon.DrawingAttributes.Color == x.Color.Color);
            if (currentColorButton == null)
            {
                _currentPenColors.First().IsChecked = true;
                App.MainWindowViewModel.MajorRibbon.DrawingAttributes.Color = Colors.Black;
            }
            else
            {
                currentColorButton.IsChecked = true;
            }
        }

        void colorButton_Checked(object sender, RoutedEventArgs e)
        {
            var colorButton = sender as ColorButton;
            if (colorButton == null)
            {
                return;
            }

            App.MainWindowViewModel.MajorRibbon.DrawingAttributes.Color = colorButton.Color.Color;
        }

        private void highlighterButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            App.MainWindowViewModel.MajorRibbon.DrawingAttributes.IsHighlighter = (bool)toggleButton.IsChecked;
            App.MainWindowViewModel.MajorRibbon.DrawingAttributes.Height = (bool)toggleButton.IsChecked ? 12 : App.MainWindowViewModel.MajorRibbon.PenSize;
            App.MainWindowViewModel.MajorRibbon.DrawingAttributes.Width = (bool)toggleButton.IsChecked ? 12 : App.MainWindowViewModel.MajorRibbon.PenSize;
            App.MainWindowViewModel.MajorRibbon.DrawingAttributes.StylusTip = (bool)toggleButton.IsChecked ? StylusTip.Rectangle : StylusTip.Ellipse;
        }

        public void SetEraserContextButtons()
        {
            Buttons.Clear();

            var setEraseInkButton = new GroupedRibbonButton("Erase Ink",
                                                           "EraserModes",
                                                           "pack://application:,,,/Resources/Images/StrokeEraser32.png",
                                                           PageInteractionModes.Select.ToString());
            setEraseInkButton.Checked += _setEraseModeButton_Checked;

            var setErasePageObjectsButton = new GroupedRibbonButton("Erase PageObjects",
                                                           "EraserModes",
                                                           "pack://application:,,,/Resources/Images/ArrayCard32.png",
                                                           PageInteractionModes.Select.ToString());
            setErasePageObjectsButton.Checked += _setEraseModeButton_Checked;

            Buttons.Add(setEraseInkButton);
            Buttons.Add(setErasePageObjectsButton);

            setEraseInkButton.IsChecked = true;
        }

        void _setEraseModeButton_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion //Methods

        #region Commands

        #endregion //Commands
    }
}