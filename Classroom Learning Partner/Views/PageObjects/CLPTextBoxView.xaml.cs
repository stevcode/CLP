using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPTextBoxView.xaml</summary>
    public partial class CLPTextBoxView
    {
        public CLPTextBoxView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged()
        {
            if (ViewModel is CLPTextBoxViewModel)
            {
                (ViewModel as CLPTextBoxViewModel).TextBoxView = this;
            }

            base.OnViewModelChanged();
        }

        private bool _isSettingFont;

        public void SetFont(double fontSize, FontFamily font, Brush fontColor, bool? isBold, bool? isItalic, bool? isUnderLined)
        {
            _isSettingFont = true;
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
                        var p = new Paragraph();
                        if (fontSize > 0)
                        {
                            p.FontSize = fontSize;
                        }
                        if (font != null)
                        {
                            p.FontFamily = font;
                        }
                        if (fontColor != null)
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
                            p.TextDecorations = (bool)isUnderLined ? TextDecorations.Underline : null;
                        }
                        TextBox.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        var curCaret = TextBox.CaretPosition;
                        // Get the current block object that the cursor is in
                        var curBlock = TextBox.Document.Blocks.FirstOrDefault(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1);
                        if (curBlock != null)
                        {
                            var curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            var newRun = new Run();
                            if (fontSize > 0)
                            {
                                newRun.FontSize = fontSize;
                            }
                            if (font != null)
                            {
                                newRun.FontFamily = font;
                            }
                            if (fontColor != null)
                            {
                                newRun.Foreground = fontColor;
                            }
                            if (isBold != null)
                            {
                                newRun.FontWeight = (bool)isBold ? FontWeights.Bold : FontWeights.Normal;
                            }
                            if (isItalic != null)
                            {
                                newRun.FontStyle = (bool)isItalic ? FontStyles.Italic : FontStyles.Normal;
                            }
                            if (isUnderLined != null)
                            {
                                newRun.TextDecorations = (bool)isUnderLined ? TextDecorations.Underline : null;
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
                    if (fontSize > 0)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    }
                    if (font != null)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
                    }
                    if (fontColor != null)
                    {
                        selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, fontColor);
                    }
                    if (isBold != null)
                    {
                        var newFontWeight = (bool)isBold ? FontWeights.Bold : FontWeights.Normal;
                        selectionTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, newFontWeight);
                    }
                    if (isItalic != null)
                    {
                        var newFontStyle = (bool)isItalic ? FontStyles.Italic : FontStyles.Normal;
                        selectionTextRange.ApplyPropertyValue(TextElement.FontStyleProperty, newFontStyle);
                    }
                    if (isUnderLined != null)
                    {
                        var newTextDecorations = (bool)isUnderLined ? TextDecorations.Underline : null;
                        selectionTextRange.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecorations);
                    }
                }
            }

            TextBox.Focus();
            _isSettingFont = false;
        }

        private bool _isUpdatingVisualState = false;

        private void UpdateVisualState()
        {
            if (_isUpdatingVisualState)
            {
                return;
            }

            _isUpdatingVisualState = true;
            if (ViewModel is CLPTextBoxViewModel)
            {
                (ViewModel as CLPTextBoxViewModel).UpdateContextRibbonButtons();
            }
            _isUpdatingVisualState = false;
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