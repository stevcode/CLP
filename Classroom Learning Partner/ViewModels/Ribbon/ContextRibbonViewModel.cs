using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

        private GroupedRibbonButton _inkEraserButton;
        private GroupedRibbonButton _pageObjectEraserButton;
        private GroupedRibbonButton _dividerEraserButton;

        #endregion //Buttons

        #region Bindings

        /// <summary>List of the buttons currently on the Ribbon.</summary>
        public ObservableCollection<UIElement> Buttons
        {
            get { return GetValue<ObservableCollection<UIElement>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons", typeof (ObservableCollection<UIElement>), () => new ObservableCollection<UIElement>());

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
            var penGroupName = "DrawModes" + catelHack;
            _penModeButton = new GroupedRibbonButton("Pen", penGroupName, "pack://application:,,,/Resources/Images/Pen32.png", DrawModes.Pen.ToString(), true);
            _penModeButton.Checked += _penButton_Checked;

            _markerModeButton = new GroupedRibbonButton("Marker", penGroupName, "pack://application:,,,/Resources/Images/Marker128.png", DrawModes.Marker.ToString(), true);
            _markerModeButton.Checked += _penButton_Checked;

            _highlighterModeButton = new GroupedRibbonButton("Highlighter",
                                                             penGroupName,
                                                             "pack://application:,,,/Resources/Images/Highlighter32.png",
                                                             DrawModes.Highlighter.ToString(),
                                                             true);
            _highlighterModeButton.Checked += _penButton_Checked;

            var eraserGroupName = "EraserModes" + catelHack;
            _inkEraserButton = new GroupedRibbonButton("Erase Ink", eraserGroupName, "pack://application:,,,/Resources/Images/StrokeEraser32.png", ErasingModes.Ink.ToString(), true);
            _inkEraserButton.Checked += _eraserButton_Checked;

            _pageObjectEraserButton = new GroupedRibbonButton("Erase Objects",
                                                              eraserGroupName,
                                                              "pack://application:,,,/Resources/Images/StrokeEraser32.png",
                                                              ErasingModes.PageObjects.ToString(),
                                                              true);
            _pageObjectEraserButton.Checked += _eraserButton_Checked;

            _dividerEraserButton = new GroupedRibbonButton("Erase Dividers",
                                                           eraserGroupName,
                                                           "pack://application:,,,/Resources/Images/StrokeEraser32.png",
                                                           ErasingModes.Dividers.ToString(),
                                                           true);
            _dividerEraserButton.Checked += _eraserButton_Checked;
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
                CurrentPenColors.Add(new ColorButton(Colors.White));
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

        private bool _isCheckedEventRunning;

        private void _penButton_Checked(object sender, RoutedEventArgs e)
        {
            _isCheckedEventRunning = true;
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            var checkedButton = sender as GroupedRibbonButton;
            if (checkedButton == null ||
                _pageInteractionService == null)
            {
                return;
            }

            var drawMode = (DrawModes)Enum.Parse(typeof (DrawModes), checkedButton.AssociatedEnumValue);
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

        private void _eraserButton_Checked(object sender, RoutedEventArgs e)
        {
            _isCheckedEventRunning = true;
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            var checkedButton = sender as GroupedRibbonButton;
            if (checkedButton == null ||
                _pageInteractionService == null)
            {
                return;
            }

            var eraserMode = (ErasingModes)Enum.Parse(typeof (ErasingModes), checkedButton.AssociatedEnumValue);
            switch (eraserMode)
            {
                case ErasingModes.Ink:
                    _pageInteractionService.SetInkEraserMode();
                    break;
                case ErasingModes.PageObjects:
                    _pageInteractionService.SetPageObjectEraserMode();
                    break;
                case ErasingModes.Dividers:
                    _pageInteractionService.SetDividerEraserMode();
                    break;
            }

            _isCheckedEventRunning = false;
        }

        public void SetEraserContextButtons()
        {
            Buttons.Clear();
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            if (_pageInteractionService == null)
            {
                return;
            }

            Buttons.Add(_inkEraserButton);
            //Buttons.Add(_pageObjectEraserButton);
            Buttons.Add(_dividerEraserButton);

            switch (_pageInteractionService.CurrentErasingMode)
            {
                case ErasingModes.Ink:
                    _inkEraserButton.IsChecked = true;
                    break;
                case ErasingModes.PageObjects:
                    _pageObjectEraserButton.IsChecked = true;
                    break;
                case ErasingModes.Dividers:
                    _dividerEraserButton.IsChecked = true;
                    break;
            }
        }

        #endregion //Methods
    }
}