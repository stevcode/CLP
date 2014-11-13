﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
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
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

            //switch (_pageInteractionService.CurrentPageInteractionMode)
            //{
            //    case PageInteractionModes.Pen:
            //        SetPenContextButtons();
            //        break;
            //    case PageInteractionModes.Eraser:
            //        SetEraserContextButtons();
            //        break;
            //}
        }

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

        public void SetPenContextButtons()
        {
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            if (_pageInteractionService == null)
            {
                return;
            }

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

            Buttons.Clear();
            

            Buttons.Add(new RibbonButton("Pen Size", "pack://application:,,,/Resources/Images/PenSize32.png", null, null, true));
            var highlighterButton = new ToggleRibbonButton("Highlighter", "Highlighter", "pack://application:,,,/Resources/Images/Highlighter32.png", true)
                                    {
                                        IsChecked = _pageInteractionService.IsHighlighting
                                    };
            highlighterButton.Checked += highlighterButton_Checked;
            highlighterButton.Unchecked += highlighterButton_Checked;
            Buttons.Add(highlighterButton);

            Buttons.Add(MajorRibbonViewModel.Separater);

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

        private void highlighterButton_Checked(object sender, RoutedEventArgs e)
        {
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null ||
                _pageInteractionService == null)
            {
                return;
            }

            _pageInteractionService.ToggleHighlighter();
        }

        public void SetEraserContextButtons()
        {
            Buttons.Clear();

            var setEraseInkButton = new GroupedRibbonButton("Erase Ink",
                                                            "EraserModes",
                                                            "pack://application:,,,/Resources/Images/StrokeEraser32.png",
                                                            PageInteractionModes.Select.ToString(),
                                                            true);
            setEraseInkButton.Checked += _setEraseModeButton_Checked;

            var setErasePageObjectsButton = new GroupedRibbonButton("Erase PageObjects",
                                                                    "EraserModes",
                                                                    "pack://application:,,,/Resources/Images/ArrayCard32.png",
                                                                    PageInteractionModes.Select.ToString(),
                                                                    true);
            setErasePageObjectsButton.Checked += _setEraseModeButton_Checked;

            Buttons.Add(setEraseInkButton);
            Buttons.Add(setErasePageObjectsButton);

            setEraseInkButton.IsChecked = true;
        }

        private void _setEraseModeButton_Checked(object sender, RoutedEventArgs e) { }

        #endregion //Methods

        #region Commands

        #endregion //Commands
    }
}