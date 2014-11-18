using System;
using System.Linq;
using System.Windows;
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
    public partial class CLPTextBoxView
    {
        public CLPTextBoxView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType() { return typeof(CLPTextBoxViewModel); }

        protected override void OnViewModelChanged()
        {
            if (ViewModel is CLPTextBoxViewModel)
            {
                (ViewModel as CLPTextBoxViewModel).TextBox = this;
            }

            base.OnViewModelChanged();
        }

        #region Font Style Methods

        private bool _isSettingFont;

        public void SetFont(double fontSize, FontFamily font, Brush fontColor, bool? isBold, bool? isItalic, bool? isUnderLined)
        {
            _isSettingFont = true;
            // Make sure we have a selection. Should have one even if there is no text selected.
            if(TextBox.Selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if(TextBox.Selection.IsEmpty)
                {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if(TextBox.Selection.Start.Paragraph == null)
                    {
                        // Add a new paragraph object to the richtextbox with the fontsize
                        var p = new Paragraph();
                        if(fontSize > 0)
                        {
                            p.FontSize = fontSize;
                        }
                        if(font != null)
                        {
                            p.FontFamily = font;
                        }
                        if(fontColor != null)
                        {
                            p.Foreground = fontColor;
                        }
                        if (isBold != null)
                        {
                            p.FontWeight = (bool)isBold ? FontWeights.Bold : FontWeights.Normal;
                        }
                        if (isItalic != null)
                        {
                            p.FontStyle = (bool)isItalic ? FontStyles.Italic : FontStyles.Normal;
                        }
                        if (isUnderLined != null)
                        {
                            
                            p.FontWeight = (bool)isUnderLined ? FontWeights.UltraBlack : FontWeights.Normal;
                        }
                        TextBox.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        var curCaret = TextBox.CaretPosition;
                        // Get the current block object that the cursor is in
                        var curBlock = TextBox.Document.Blocks.FirstOrDefault(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1);
                        if(curBlock != null)
                        {
                            var curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            var newRun = new Run();
                            if(fontSize > 0)
                            {
                                newRun.FontSize = fontSize;
                            }
                            if(font != null)
                            {
                                newRun.FontFamily = font;
                            }
                            if(fontColor != null)
                            {
                                newRun.Foreground = fontColor;
                            }
                            if (isBold != null)
                            {
                                newRun.FontWeight = (bool)isBold ? FontWeights.Bold : FontWeights.Normal;
                            }
                            curParagraph.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            TextBox.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the fontsize of the selection
                {
                    var selectionTextRange = new TextRange(TextBox.Selection.Start, TextBox.Selection.End);
                    if(fontSize > 0)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    }
                    if(font != null)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
                    }
                    if(fontColor != null)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, fontColor);
                    }
                    if (isBold != null)
                    {
                        var newFontWeight = (bool)isBold ? FontWeights.Bold : FontWeights.Normal;
                        selectionTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, newFontWeight);
                    }
                }
            }

            TextBox.Focus();
            _isSettingFont = false;
        }

        public bool isUpdatingVisualState = false;

        public void UpdateVisualState()
        {
            if(!isUpdatingVisualState)
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

        private void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            var currentValue = TextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue == DependencyProperty.UnsetValue) ? false : currentValue != null && currentValue.Equals(expectedValue);
        }

        private void UpdateSelectedFontFamily()
        {
            var value = TextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            var currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if(currentFontFamily != null)
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
            var value = TextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            //ribbonView.FontSizeComboBox.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        private void UpdateSelectedFontColor()
        {
            var value = TextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            var currentFontColor = (Brush)((value == DependencyProperty.UnsetValue) ? null : value);
            if(currentFontColor != null)
            {
                //ribbonView.FontColorComboBox.SelectedItem = currentFontColor;
            }
        }

        #endregion //Font Style Methods

        private void TextBox_OnIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsHitTestVisible)
            {
                TextBox.Focus();
            }
        }

        private void TextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isSettingFont)
            {
                UpdateVisualState();
            }
        }
    }
}