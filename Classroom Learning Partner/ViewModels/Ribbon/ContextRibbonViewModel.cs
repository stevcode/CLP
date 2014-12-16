using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.CustomControls;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ContextRibbonViewModel : ViewModelBase
    {
        private IPageInteractionService _pageInteractionService;

        public ContextRibbonViewModel()
        {
            catelHack = UniqueIdentifier.ToString();
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            InitializeButtons();

            switch (_pageInteractionService.CurrentPageInteractionMode)
            {
                case PageInteractionModes.Draw:
                    SetPenContextButtons();
                    break;
                case PageInteractionModes.Erase:
                    SetEraserContextButtons();
                    break;
            }
        }

        #region Buttons

        private GroupedRibbonButton _penModeButton;
        private GroupedRibbonButton _markerModeButton;
        private GroupedRibbonButton _highlighterModeButton; 

        #endregion //Buttons

        #region Bindings

        /// <summary>List of the buttons currently on the Ribbon.</summary>
        public ObservableCollection<UIElement> Buttons
        {
            get { return GetValue<ObservableCollection<UIElement>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons",
                                                                               typeof (ObservableCollection<UIElement>),
                                                                               () => new ObservableCollection<UIElement>());

        /// <summary>Current Pen colors.</summary>
        public ObservableCollection<ColorButton> CurrentPenColors
        {
            get { return GetValue<ObservableCollection<ColorButton>>(CurrentPenColorsProperty); }
            set { SetValue(CurrentPenColorsProperty, value); }
        }

        public static readonly PropertyData CurrentPenColorsProperty = RegisterProperty("CurrentPenColors",
                                                                                        typeof (ObservableCollection<ColorButton>),
                                                                                        () => new ObservableCollection<ColorButton>());

        #endregion //Bindings

        #region Methods

        public static string catelHack = "1";

        private void InitializeButtons()
        {
            var groupName = "DrawModes" + catelHack;
            _penModeButton = new GroupedRibbonButton("Pen",
                                                        groupName,
                                                        "pack://application:,,,/Resources/Images/Pen32.png",
                                                        DrawModes.Pen.ToString(),
                                                        true);
            _penModeButton.Checked += _button_Checked;
            
            _markerModeButton = new GroupedRibbonButton("Marker",
                                                           groupName,
                                                           "pack://application:,,,/Resources/Images/Marker128.png",
                                                           DrawModes.Marker.ToString(),
                                                           true);
            _markerModeButton.Checked += _button_Checked;
            
            _highlighterModeButton = new GroupedRibbonButton("Highlighter",
                                                                groupName,
                                                                "pack://application:,,,/Resources/Images/Highlighter32.png",
                                                                DrawModes.Highlighter.ToString(),
                                                                true);
            _highlighterModeButton.Checked += _button_Checked;
        }

        public void SetPenContextButtons()
        {
            Buttons.Clear();
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            if (_pageInteractionService == null)
            {
                return;
            }

            Buttons.Add(_penModeButton);
            Buttons.Add(_markerModeButton);
            Buttons.Add(_highlighterModeButton);

            Buttons.Add(MajorRibbonViewModel.Separater);

            if (!CurrentPenColors.Any())
            {
                CurrentPenColors.Add(new ColorButton(Colors.Black));
                //CurrentPenColors.Add(new ColorButton(Colors.White));
                CurrentPenColors.Add(new ColorButton(Colors.Red));
                CurrentPenColors.Add(new ColorButton(Colors.DarkOrange));
                CurrentPenColors.Add(new ColorButton(Colors.Tan));
                CurrentPenColors.Add(new ColorButton(Colors.Gold));
                CurrentPenColors.Add(new ColorButton(Colors.SaddleBrown));
                CurrentPenColors.Add(new ColorButton(Colors.DarkGreen));
                CurrentPenColors.Add(new ColorButton(Colors.MediumSeaGreen));
                CurrentPenColors.Add(new ColorButton(Colors.Blue));
                CurrentPenColors.Add(new ColorButton(Colors.HotPink));
                CurrentPenColors.Add(new ColorButton(Colors.BlueViolet));
                CurrentPenColors.Add(new ColorButton(Colors.Aquamarine));
                CurrentPenColors.Add(new ColorButton(Colors.SlateGray));
                CurrentPenColors.Add(new ColorButton(Colors.SkyBlue));
                CurrentPenColors.Add(new ColorButton(Colors.DeepSkyBlue));
                CurrentPenColors.Add(new ColorButton(Color.FromRgb(0, 152, 247)));

                foreach (var colorButton in CurrentPenColors)
                {
                    colorButton.Checked += colorButton_Checked;
                }
            }

            Buttons.AddRange(CurrentPenColors);

            var currentColorButton = CurrentPenColors.FirstOrDefault(x => _pageInteractionService.PenColor == x.Color.Color);
            if (currentColorButton == null)
            {
                CurrentPenColors.First().IsChecked = true;
                _pageInteractionService.SetPenColor(Colors.Black);
            }
            else
            {
                currentColorButton.IsChecked = true;
            }

            switch (_pageInteractionService.CurrentDrawMode)
            {
                case DrawModes.Pen:
                    _penModeButton.IsChecked = true;
                    break;
                case DrawModes.Marker:
                    _markerModeButton.IsChecked = true;
                    break;
                case DrawModes.Highlighter:
                    _highlighterModeButton.IsChecked = true;
                    break;
            }
        }

        private bool _isCheckedEventRunning = false;

        private void _button_Checked(object sender, RoutedEventArgs e)
        {
            _isCheckedEventRunning = true;
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            var checkedButton = sender as GroupedRibbonButton;
            if (checkedButton == null ||
                _pageInteractionService == null)
            {
                return;
            }

            var drawMode = (DrawModes)Enum.Parse(typeof(DrawModes), checkedButton.AssociatedEnumValue);
            switch (drawMode)
            {
                case DrawModes.Pen:
                    _pageInteractionService.SetPenMode();
                    break;
                case DrawModes.Marker:
                    _pageInteractionService.SetMarkerMode();
                    break;
                case DrawModes.Highlighter:
                    _pageInteractionService.SetHighlighterMode();
                    break;
            }

            _isCheckedEventRunning = false;
        }

        private void colorButton_Checked(object sender, RoutedEventArgs e)
        {
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            var colorButton = sender as ColorButton;
            if (colorButton == null ||
                _pageInteractionService == null)
            {
                return;
            }

            _pageInteractionService.SetPenColor(colorButton.Color.Color);
        }

        public void SetEraserContextButtons()
        {
            Buttons.Clear();

            //var setEraseInkButton = new GroupedRibbonButton("Erase Ink",
            //                                                "EraserModes",
            //                                                "pack://application:,,,/Resources/Images/StrokeEraser32.png",
            //                                                PageInteractionModes.Select.ToString(),
            //                                                true);
            //setEraseInkButton.Checked += _setEraseModeButton_Checked;

            //var setErasePageObjectsButton = new GroupedRibbonButton("Erase PageObjects",
            //                                                        "EraserModes",
            //                                                        "pack://application:,,,/Resources/Images/ArrayCard32.png",
            //                                                        PageInteractionModes.Select.ToString(),
            //                                                        true);
            //setErasePageObjectsButton.Checked += _setEraseModeButton_Checked;

            //Buttons.Add(setEraseInkButton);
            //Buttons.Add(setErasePageObjectsButton);

            //setEraseInkButton.IsChecked = true;
        }

        private void _setEraseModeButton_Checked(object sender, RoutedEventArgs e) { }

        #endregion //Methods
    }
}