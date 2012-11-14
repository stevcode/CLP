using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPTextBoxView.xaml
    /// </summary>
    public partial class CLPTextBoxView : Catel.Windows.Controls.UserControl
    {
        private MainWindowView ribbonView;

        public CLPTextBoxView()
        {
            InitializeComponent();

            ribbonView = Application.Current.MainWindow as MainWindowView;
            App.MainWindowViewModel.Ribbon.LastFocusedTextBox = this;

            TextBox.FontSize = App.MainWindowViewModel.Ribbon.CurrentFontSize;
            TextBox.FontFamily = App.MainWindowViewModel.Ribbon.CurrentFontFamily;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPTextBoxViewModel);
        }

        #region Font Style Methods

        private bool isSettingFont = false;
        public void SetFont(double fontSize, FontFamily font, Brush fontColor)
        {
            isSettingFont = true;
            Console.WriteLine("setfont called");
            // Make sure we have a selection. Should have one even if there is no text selected.
            if (TextBox.Selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (TextBox.Selection.IsEmpty)
                {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if (TextBox.Selection.Start.Paragraph == null)
                    {
                        // Add a new paragraph object to the richtextbox with the fontsize
                        Paragraph p = new Paragraph();
                        if (fontSize > 0) p.FontSize = fontSize;
                        if (font != null) p.FontFamily = font;
                        if (fontColor != null) p.Foreground = fontColor;
                        TextBox.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        TextPointer curCaret = TextBox.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = TextBox.Document.Blocks.Where(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                        if (curBlock != null)
                        {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            Run newRun = new Run();
                            if (fontSize > 0) newRun.FontSize = fontSize;
                            if (font != null) newRun.FontFamily = font;
                            if (fontColor != null) newRun.Foreground = fontColor;
                            curParagraph.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            TextBox.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the fontsize of the selection
                {
                    TextRange selectionTextRange = new TextRange(TextBox.Selection.Start, TextBox.Selection.End);
                    if (fontSize > 0) selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    if (font != null) selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
                    if (fontColor != null) selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, fontColor);
                }
            }
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            if(App.MainWindowViewModel.Ribbon.LastFocusedTextBox == this)
            {
                TextBox.Focus();
            }
            isSettingFont = false;
        }

        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!isSettingFont)
            {
                UpdateVisualState();
            }
        }

        public bool isUpdatingVisualState = false;
        public void UpdateVisualState()
        {
            if (!isUpdatingVisualState)
            {
                isUpdatingVisualState = true;
                UpdateToggleButtonState();
                UpdateSelectedFontFamily();
                UpdateSelectedFontSize();
                UpdateSelectedFontColor();
                isUpdatingVisualState = false;
            } 
        }

        private void UpdateToggleButtonState()
        {
            //UpdateItemCheckedState(ribbonView.BoldButton, TextElement.FontWeightProperty, FontWeights.Bold);
            //UpdateItemCheckedState(ribbonView.ItalicButton, TextElement.FontStyleProperty, FontStyles.Italic);
            //UpdateItemCheckedState(ribbonView.UnderlineButton, Inline.TextDecorationsProperty, TextDecorations.Underline);   
        }

        void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = TextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue == DependencyProperty.UnsetValue) ? false : currentValue != null && currentValue.Equals(expectedValue);
        }

        private void UpdateSelectedFontFamily()
        {
            object value = TextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
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
            object value = TextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            //ribbonView.FontSizeComboBox.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        private void UpdateSelectedFontColor()
        {
            object value = TextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            Brush currentFontColor = (Brush)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontColor != null)
            {
                //ribbonView.FontColorComboBox.SelectedItem = currentFontColor;
            }
        }

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);
            App.MainWindowViewModel.Ribbon.LastFocusedTextBox = this;
        }

        #endregion //Font Style Methods
    }
}
