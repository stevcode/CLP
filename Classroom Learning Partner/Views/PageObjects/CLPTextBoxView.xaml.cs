using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Microsoft.Windows.Controls.Ribbon;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPTextBoxView.xaml
    /// </summary>
    public partial class CLPTextBoxView : UserControl
    {
        private RibbonWindow mainWindow;
        private RibbonView ribbonView;

        public CLPTextBoxView()
        {
            InitializeComponent();
            
            mainWindow = Application.Current.MainWindow as RibbonWindow;
            ribbonView = LogicalTreeHelper.FindLogicalNode(mainWindow, "RibbonView") as RibbonView;

            AppMessages.UpdateFont.Register(this, (t) =>
                                                        {
                                                            if (!isUpdatingState)
                                                            {
                                                                isUpdatingState = true;
                                                                SetFont(t.Item1, t.Item2, t.Item3);
                                                                isUpdatingState = false;
                                                            }
                                                             
                                                         });
            
        }

        private void SetFont(double fontSize, FontFamily font, Brush fontColor)
        {
            // Make sure we have a selection. Should have one even if there is no text selected.
            if (RichTextBox.Selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (RichTextBox.Selection.IsEmpty)
                {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if (RichTextBox.Selection.Start.Paragraph == null)
                    {
                        // Add a new paragraph object to the richtextbox with the fontsize
                        Paragraph p = new Paragraph();
                        if (fontSize > 0) p.FontSize = fontSize;
                        if (font != null) p.FontFamily = font;
                        if (fontColor != null) p.Foreground = fontColor;
                        RichTextBox.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        TextPointer curCaret = RichTextBox.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = RichTextBox.Document.Blocks.Where
                            (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
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
                            RichTextBox.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the fontsize of the selection
                {
                    TextRange selectionTextRange = new TextRange(RichTextBox.Selection.Start, RichTextBox.Selection.End);
                    if (fontSize > 0) selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    if (font != null) selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
                    if (fontColor != null) selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, fontColor);
                }
            }
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            if ((ribbonView.DataContext as RibbonViewModel).LastFocusedTextBox == this)
            {
                RichTextBox.Focus();
            }
        }

        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!isUpdatingState)
            {
                UpdateVisualState();
            }
        }

        private bool isUpdatingState = false;
        public void UpdateVisualState()
        {
            UpdateToggleButtonState();
            UpdateSelectedFontFamily();
            UpdateSelectedFontSize();
            UpdateSelectedFontColor();
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(ribbonView.BoldButton, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(ribbonView.ItalicButton, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateItemCheckedState(ribbonView.UnderlineButton, Inline.TextDecorationsProperty, TextDecorations.Underline);   
        }

        void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = RichTextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue == DependencyProperty.UnsetValue) ? false : currentValue != null && currentValue.Equals(expectedValue);
        }

        private void UpdateSelectedFontFamily()
        {
            object value = RichTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null)
            {
                ribbonView.FontFamilyComboBox.SelectedItem = currentFontFamily;
            }
        }

        private void UpdateSelectedFontSize()
        {
            object value = RichTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            ribbonView.FontSizeComboBox.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        private void UpdateSelectedFontColor()
        {
            object value = RichTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            Brush currentFontColor = (Brush)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontColor != null)
            {
                ribbonView.FontColorComboBox.SelectedItem = currentFontColor;
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            (ribbonView.DataContext as RibbonViewModel).LastFocusedTextBox = this;
        }

        private void RichTextBox_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
