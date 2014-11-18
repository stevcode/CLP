using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPTextBoxViewModel : APageObjectBaseViewModel
    {
        public CLPTextBoxView TextBoxView = null;

        #region Constructor

        public CLPTextBoxViewModel(CLPTextBox textBox)
        {
            PageObject = textBox; 
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            // Bold, Italics, Underline
            _contextButtons.Add(MajorRibbonViewModel.Separater);
            _toggleBoldButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Bold16.png", true)
            {
                IsChecked = IsBold
            };
            _toggleBoldButton.Checked += toggleBoldButton_Checked;
            _toggleBoldButton.Unchecked += toggleBoldButton_Checked;
            _contextButtons.Add(_toggleBoldButton);

            _toggleItalicsButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Italic16.png", true)
            {
                IsChecked = IsItalic
            };
            _toggleItalicsButton.Checked += toggleItalicsButton_Checked;
            _toggleItalicsButton.Unchecked += toggleItalicsButton_Checked;
            _contextButtons.Add(_toggleItalicsButton);

            _toggleUnderlineButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Underline16.png", true)
            {
                IsChecked = IsUnderlined
            };
            _toggleUnderlineButton.Checked += toggleUnderlineButton_Checked;
            _toggleUnderlineButton.Unchecked += toggleUnderlineButton_Checked;
            _contextButtons.Add(_toggleUnderlineButton);

            // Font Family, Size, Color
            _contextButtons.Add(MajorRibbonViewModel.Separater);
        }

        private bool _isUpdatingSelection = false;

        void toggleBoldButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null ||
                _isUpdatingSelection)
            {
                return;
            }

            IsBold = button.IsChecked != null && (bool)button.IsChecked;
            TextBoxView.SetFont(0, null, null, IsBold, IsItalic, IsUnderlined);
        }

        void toggleItalicsButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null ||
                _isUpdatingSelection)
            {
                return;
            }

            IsItalic = button.IsChecked != null && (bool)button.IsChecked;
            TextBoxView.SetFont(0, null, null, IsBold, IsItalic, IsUnderlined);
        }

        void toggleUnderlineButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null ||
                _isUpdatingSelection)
            {
                return;
            }

            IsUnderlined = button.IsChecked != null && (bool)button.IsChecked;
            TextBoxView.SetFont(0, null, null, IsBold, IsItalic, IsUnderlined);
        }

        public override string Title
        {
            get { return "TextBoxVM"; }
        }

        #endregion //Constructor

        #region Buttons

        private ToggleRibbonButton _toggleBoldButton;
        private ToggleRibbonButton _toggleItalicsButton;
        private ToggleRibbonButton _toggleUnderlineButton;

        #endregion //Buttons

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Toggles selected text boldness.
        /// </summary>
        public bool IsBold
        {
            get { return GetValue<bool>(IsBoldProperty); }
            set { SetValue(IsBoldProperty, value); }
        }

        public static readonly PropertyData IsBoldProperty = RegisterProperty("IsBold", typeof (bool), false);

        /// <summary>
        /// Toggles selected text italics.
        /// </summary>
        public bool IsItalic
        {
            get { return GetValue<bool>(IsItalicProperty); }
            set { SetValue(IsItalicProperty, value); }
        }

        public static readonly PropertyData IsItalicProperty = RegisterProperty("IsItalic", typeof (bool), false);

        /// <summary>
        /// Toggles selected text underlined.
        /// </summary>
        public bool IsUnderlined
        {
            get { return GetValue<bool>(IsUnderlinedProperty); }
            set { SetValue(IsUnderlinedProperty, value); }
        }

        public static readonly PropertyData IsUnderlinedProperty = RegisterProperty("IsUnderlined", typeof (bool), false);

        /// <summary>
        /// Selected text's FontFamily.
        /// </summary>
        public FontFamily CurrentFontFamily
        {
            get { return GetValue<FontFamily>(CurrentFontFamilyProperty); }
            set { SetValue(CurrentFontFamilyProperty, value); }
        }

        public static readonly PropertyData CurrentFontFamilyProperty = RegisterProperty("CurrentFontFamily", typeof (FontFamily), () => new FontFamily("Arial"));

        /// <summary>
        /// Selected text's font size.
        /// </summary>
        public double CurrentFontSize
        {
            get { return GetValue<double>(CurrentFontSizeProperty); }
            set { SetValue(CurrentFontSizeProperty, value); }
        }

        public static readonly PropertyData CurrentFontSizeProperty = RegisterProperty("CurrentFontSize", typeof (double), 34.0);

        /// <summary>
        /// Selected text's font color.
        /// </summary>
        public Brush CurrentFontColor
        {
            get { return GetValue<Brush>(CurrentFontColorProperty); }
            set { SetValue(CurrentFontColorProperty, value); }
        }

        public static readonly PropertyData CurrentFontColorProperty = RegisterProperty("CurrentFontColor", typeof (Brush), Colors.Black);

        #endregion //Bindings

        #region Static Properties

        private static readonly List<FontFamily> _availableFontFamilies = new List<FontFamily>(Fonts.SystemFontFamilies);

        public static List<FontFamily> AvailableFontFamilies
        {
            get { return _availableFontFamilies.OrderBy(x => x.Source).ToList(); }
        }

        private static readonly List<double> _availableFontSizes = new List<double>
                                                            {
                                                                3.0,
                                                                4.0,
                                                                5.0,
                                                                6.0,
                                                                6.5,
                                                                7.0,
                                                                7.5,
                                                                8.0,
                                                                8.5,
                                                                9.0,
                                                                9.5,
                                                                10.0,
                                                                10.5,
                                                                11.0,
                                                                11.5,
                                                                12.0,
                                                                12.5,
                                                                13.0,
                                                                13.5,
                                                                14.0,
                                                                15.0,
                                                                16.0,
                                                                17.0,
                                                                18.0,
                                                                19.0,
                                                                20.0,
                                                                22.0,
                                                                24.0,
                                                                26.0,
                                                                28.0,
                                                                30.0,
                                                                32.0,
                                                                34.0,
                                                                36.0,
                                                                38.0,
                                                                40.0,
                                                                44.0,
                                                                48.0,
                                                                52.0,
                                                                56.0,
                                                                60.0,
                                                                64.0,
                                                                68.0,
                                                                72.0,
                                                                76.0,
                                                                80.0,
                                                                88.0,
                                                                96.0,
                                                                104.0,
                                                                112.0,
                                                                120.0,
                                                                128.0,
                                                                136.0,
                                                                144.0
                                                            };

        public static List<double> AvailableFontSizes
        {
            get { return _availableFontSizes; }
        }

        private static readonly List<Brush> _availableFontColors = new List<Brush>
                                                   {
                                                       new SolidColorBrush(Colors.Black),
                                                       new SolidColorBrush(Colors.Red),
                                                       new SolidColorBrush(Colors.DarkOrange),
                                                       new SolidColorBrush(Colors.Tan),
                                                       new SolidColorBrush(Colors.Gold),
                                                       new SolidColorBrush(Colors.SaddleBrown),
                                                       new SolidColorBrush(Colors.DarkGreen),
                                                       new SolidColorBrush(Colors.MediumSeaGreen),
                                                       new SolidColorBrush(Colors.Blue),
                                                       new SolidColorBrush(Colors.HotPink),
                                                       new SolidColorBrush(Colors.BlueViolet),
                                                       new SolidColorBrush(Colors.Aquamarine),
                                                       new SolidColorBrush(Colors.SlateGray),
                                                       new SolidColorBrush(Colors.SkyBlue),
                                                       new SolidColorBrush(Colors.DeepSkyBlue),
                                                       new SolidColorBrush(Color.FromRgb(0, 152, 247))
                                                   };

        public static List<Brush> AvailableFontColors
        {
            get
            {
                return _availableFontColors;
            }
        }

        #endregion //Static Properties

        #region Methods

        public void UpdateContextRibbonButtons()
        {
            _isUpdatingSelection = true;
            UpdateToggleButtonState();
            UpdateSelectedFontFamily();
            UpdateSelectedFontSize();
            UpdateSelectedFontColor();
            _isUpdatingSelection = false;
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(_toggleBoldButton, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(_toggleItalicsButton, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateItemCheckedState(_toggleUnderlineButton, Inline.TextDecorationsProperty, TextDecorations.Underline);
        }

        private void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            var currentValue = TextBoxView.TextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue != DependencyProperty.UnsetValue) && (currentValue != null && currentValue.Equals(expectedValue));
        }

        private void UpdateSelectedFontFamily()
        {
            var value = TextBoxView.TextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            var currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null)
            {
                //ribbonView.FontFamilyComboBox.SelectedItem = currentFontFamily;
            }
            else
            {
                //ribbonView.FontFamilyComboBox.SelectedIndex = -1;
            }
        }

        private void UpdateSelectedFontSize()
        {
            var value = TextBoxView.TextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            //ribbonView.FontSizeComboBox.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        private void UpdateSelectedFontColor()
        {
            var value = TextBoxView.TextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            var currentFontColor = (Brush)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontColor != null)
            {
                //ribbonView.FontColorComboBox.SelectedItem = currentFontColor;
            }
        }

        protected override void PopulateContextRibbon(bool isChangedValueMeaningful)
        {
            if (isChangedValueMeaningful)
            {
                ContextRibbon.Buttons.Clear();
            }

            if (!IsAdornerVisible)
            {
                return;
            }

            TextBoxView.TextBox.Focus();
            ContextRibbon.Buttons = new ObservableCollection<UIElement>(_contextButtons);
        } 

        #endregion //Methods

        #region Static Methods

        public static void AddTextBoxToPage(CLPPage page)
        {
            var textBox = new CLPTextBox(page, string.Empty);
            ACLPPageBaseViewModel.AddPageObjectToPage(textBox);
        }

        #endregion //Static Methods
    }
}