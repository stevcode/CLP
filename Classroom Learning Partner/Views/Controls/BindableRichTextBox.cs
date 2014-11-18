using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Views
{
    public class BindableRichTextBox : RichTextBox
    {
        private readonly HashSet<Thread> _recursionProtection = new HashSet<Thread>();

        private const string BLANK_TEXTBOX_TEXT =
            @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Arial;}{\f2\fcharset0 Arial;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs51\f2\cf0 \cf0\ql\sl15\slmult0{\fs36\f0 {\ltrch}\li0\ri0\sa0\sb0\fi0\ql\sl15\slmult0\par}}}";

        public BindableRichTextBox()
        {
            TextChanged += BindableRTB_TextChanged;
            BorderThickness = new Thickness(0.0);
            Loaded += BindableRichTextBox_Loaded;
        }

        private void BindableRichTextBox_Loaded(object sender, RoutedEventArgs e) { RichTextBoxCommandBindings.IntializeCommandBindings(this); }

        private void BindableRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = new TextRange(Document.ContentStart, Document.ContentEnd);
            var stream = new MemoryStream();
            text.Save(stream, DataFormats.Rtf);

            RTFText = Encoding.UTF8.GetString(stream.ToArray());
        }

        public string RTFText
        {
            get { return (string)GetValue(RTFTextProperty); }
            set
            {
                _recursionProtection.Add(Thread.CurrentThread);
                SetValue(RTFTextProperty, value);
                _recursionProtection.Remove(Thread.CurrentThread);
            }
        }

        public static readonly DependencyProperty RTFTextProperty = DependencyProperty.RegisterAttached("RTFText",
                                                                                                        typeof (string),
                                                                                                        typeof (BindableRichTextBox),
                                                                                                        new FrameworkPropertyMetadata(BLANK_TEXTBOX_TEXT,
                                                                                                                                      FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                      FrameworkPropertyMetadataOptions
                                                                                                                                          .BindsTwoWayByDefault,
                                                                                                                                      CallBack));

        private static void CallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rtb = d as BindableRichTextBox;

            if (rtb == null ||
                rtb._recursionProtection.Contains(Thread.CurrentThread))
            {
                return;
            }

            try
            {
                var rtfText = e.NewValue as string;
                var doc = new FlowDocument();

                if (string.IsNullOrWhiteSpace(rtfText))
                {
                    rtfText = BLANK_TEXTBOX_TEXT;
                }

                var bytes = Encoding.UTF8.GetBytes(rtfText);
                var stream = new MemoryStream(bytes);

                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                range.Load(stream, DataFormats.Rtf);

                // Set the document
                rtb.Document = doc;
            }
            catch (Exception)
            {
                rtb.Document = new FlowDocument();
            }
        }
    }

    internal static class RichTextBoxCommandBindings
    {
        public static void IntializeCommandBindings(RichTextBox richTextBox)
        {
            if (richTextBox == null)
            {
                return;
            }

            richTextBox.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleBold,
                                                               (sender, e) => SetBold(richTextBox, !GetBold(richTextBox)),
                                                               (sender, e) =>
                                                               {
                                                                   CommandUtil.SetCurrentValue(e, GetBold(richTextBox));

                                                                   e.CanExecute = true;
                                                                   e.Handled = true;
                                                               }));

            richTextBox.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleItalic,
                                                               (sender, e) => SetItalic(richTextBox, !GetItalic(richTextBox)),
                                                               (sender, e) =>
                                                               {
                                                                   CommandUtil.SetCurrentValue(e, GetItalic(richTextBox));

                                                                   e.CanExecute = true;
                                                                   e.Handled = true;
                                                               }));
            richTextBox.CommandBindings.Add(new CommandBinding(MyCommands.SetFontSize,
                                                               (sender, e) =>
                                                               {
                                                                   if (e.Parameter is double)
                                                                   {
                                                                       SetFontSize(richTextBox, (double)e.Parameter);
                                                                   }
                                                               },
                                                               (sender, e) =>
                                                               {
                                                                   var value = new RangedValue(GetFontSize(richTextBox), 1, 120);
                                                                   CommandUtil.SetCurrentValue(e, value);
                                                                   e.CanExecute = true;
                                                                   e.Handled = true;
                                                               }));

            richTextBox.CommandBindings.Add(new CommandBinding(MyCommands.SetFontColor,
                                                               (sender, e) =>
                                                               {
                                                                   if (e.Parameter is Brush)
                                                                   {
                                                                       SetFontColor(richTextBox, (e.Parameter as SolidColorBrush).Color);
                                                                   }
                                                               },
                                                               (sender, e) =>
                                                               {
                                                                   CommandUtil.SetCurrentValue(e, GetFontColor(richTextBox));
                                                                   e.CanExecute = true;
                                                                   e.Handled = true;
                                                               }));

            richTextBox.CommandBindings.Add(new CommandBinding(MyCommands.SetFontFamily,
                                                               (sender, e) =>
                                                               {
                                                                   if (e.Parameter is FontFamily)
                                                                   {
                                                                       SetFontFamily(richTextBox, (e.Parameter as FontFamily));
                                                                   }
                                                               },
                                                               (sender, e) =>
                                                               {
                                                                   CommandUtil.SetCurrentValue(e, GetFontFamily(richTextBox));
                                                                   e.CanExecute = true;
                                                                   e.Handled = true;
                                                               }));
        }

        private static double GetFontSize(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontSizeProperty);
            return value != DependencyProperty.UnsetValue ? (double)value : 12;
        }

        private static void SetFontSize(RichTextBox richTextBox, double size) { richTextBox.Selection.ApplyPropertyValue(TextBlock.FontSizeProperty, size); }

        private static Color GetFontColor(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            var brush = value as SolidColorBrush;
            return brush == null ? Colors.Black : brush.Color;
        }

        private static void SetFontColor(RichTextBox richTextBox, Color c) { richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(c)); }

        private static FontFamily GetFontFamily(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontFamilyProperty);
            return value != DependencyProperty.UnsetValue ? (FontFamily)value : new FontFamily("Arial");
        }

        private static void SetFontFamily(RichTextBox richTextBox, FontFamily font) { richTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, font); }

        private static bool GetItalic(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontStyleProperty);
            return value != DependencyProperty.UnsetValue && (FontStyle)value == FontStyles.Italic;
        }

        private static void SetItalic(RichTextBox richTextBox, bool isItalic)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontStyleProperty, isItalic ? FontStyles.Italic : FontStyles.Normal);
        }

        private static bool GetBold(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontWeightProperty);
            return value != DependencyProperty.UnsetValue && (FontWeight)value == FontWeights.Bold;
        }

        private static void SetBold(RichTextBox richTextBox, bool isBold)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontWeightProperty, isBold ? FontWeights.Bold : FontWeights.Normal);
        }
    }

    public class XComboBox : ComboBox, ICommandSource
    {
        #region ICommandSource Members

        #region Command dependency property and routed event

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                                typeof (ICommand),
                                                                                                typeof (XComboBox),
                                                                                                new PropertyMetadata(default(ICommand),
                                                                                                                     new PropertyChangedCallback(OnCommandChanged)));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (XComboBox)d;
            owner.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        public static readonly RoutedEvent CommandChangedEvent = EventManager.RegisterRoutedEvent("CommandChangedEvent",
                                                                                                  RoutingStrategy.Bubble,
                                                                                                  typeof (RoutedPropertyChangedEventHandler<ICommand>),
                                                                                                  typeof (XComboBox));

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged
        {
            add { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<ICommand>(oldValue, newValue);
            args.RoutedEvent = CommandChangedEvent;
            RaiseEvent(args);

            if (cmd_CanExecuteChangedHandler == null)
            {
                cmd_CanExecuteChangedHandler = cmd_CanExecuteChanged;
            }
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= cmd_CanExecuteChangedHandler;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += cmd_CanExecuteChangedHandler;
            }
            else
            {
                cmd_CanExecuteChangedHandler = null;
            }

            UpdateCanExecute();
        }

        // hold a reference to it, it might me stored as a weak reference and never be called otherwise...
        private EventHandler cmd_CanExecuteChangedHandler;
        private void cmd_CanExecuteChanged(object sender, EventArgs e) { UpdateCanExecute(); }

        protected virtual void UpdateCanExecute()
        {
            if (IsCommandExecuting)
            {
                return;
            }

            var cmd = Command;
            if (cmd == null)
            {
                IsEnabled = true;
            }
            else
            {
                try
                {
                    IsCommandExecuting = true;

                    IsEnabled = CommandUtil.CanExecute(this, CommandParameter);
                    var ca = new CommandCanExecuteParameter(CommandParameter);
                    CommandUtil.CanExecute(this, ca);
                    var value = ca.CurrentValue;

                    if (value is RangedValue)
                    {
                        value = ((RangedValue)value).Value;
                    }

                    var o = FindItemByData(value);

                    if (o == null &&
                        value == null)
                    {
                        Text = null;
                        SelectedItem = null;
                    }
                    else if (o != null &&
                             !Equals(SelectedItem, o))
                    {
                        SelectedItem = o;
                    }
                    else if (IsEditable &&
                             !IsReadOnly &&
                             value != null &&
                             Text != value.ToString())
                    {
                        Text = value.ToString();
                    }
                }
                finally
                {
                    IsCommandExecuting = false;
                }
            }
        }

        private bool IsCommandExecuting { get; set; }

        #endregion

        #region CommandTarget dependency property and routed event

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                      typeof (IInputElement),
                                                                                                      typeof (XComboBox),
                                                                                                      new PropertyMetadata(default(IInputElement),
                                                                                                                           new PropertyChangedCallback(OnCommandTargetChanged)));

        private static void OnCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (XComboBox)d;
            owner.UpdateCanExecute();
        }

        #endregion

        public object CommandParameter
        {
            get { return GetItemData(SelectedItem); }
        }

        #endregion

        public object FindItemByData(object data)
        {
            foreach (var o in Items)
            {
                if (Equals(data, GetItemData(o)))
                {
                    return o;
                }
            }
            return null;
        }

        public static object GetItemData(object obj)
        {
            var cbi = obj as ComboBoxItem;
            if (cbi != null)
            {
                if (cbi.ReadLocalValue(TagProperty) != DependencyProperty.UnsetValue)
                {
                    return cbi.GetValue(TagProperty);
                }
                return cbi.Content;
            }
            return obj;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (!IsCommandExecuting)
            {
                try
                {
                    IsCommandExecuting = true;
                    CommandUtil.Execute(this);
                }
                finally
                {
                    IsCommandExecuting = false;
                }
            }
        }
    }

    public static class MyCommands
    {
        static MyCommands()
        {
            SetFontSize = new RoutedCommand();
            SetFontColor = new RoutedCommand();
            SetFontFamily = new RoutedCommand();
        }

        public static RoutedCommand SetFontSize { get; private set; }
        public static RoutedCommand SetFontColor { get; private set; }
        public static RoutedCommand SetFontFamily { get; private set; }
    }

    public struct RangedValue
    {
        public RangedValue(double value)
            : this(value, null, null) { }

        public RangedValue(double value, double? minimum, double? maximum)
        {
            _value = value;
            _minimum = minimum;
            _maximum = maximum;
        }

        private double _value;

        public double Value
        {
            get { return _value; }
        }

        private double? _minimum;

        public double? Minimum
        {
            get { return _minimum; }
        }

        private double? _maximum;

        public double? Maximum
        {
            get { return _maximum; }
        }
    }

    public class CommandCanExecuteParameter
    {
        public CommandCanExecuteParameter(object parameter) { Parameter = parameter; }

        public object Parameter { get; private set; }
        public object CurrentValue { get; set; }
    }

    public static class CommandUtil
    {
        public static bool CanExecute(ICommandSource source) { return CanExecute(source.Command, source.CommandParameter, source.CommandTarget); }

        public static bool CanExecute(ICommandSource source, object parameter)
        {
            if (source == null ||
                source.Command == null)
            {
                return false;
            }

            var target = source.CommandTarget;

            if (target == null &&
                Keyboard.FocusedElement == null)
            {
                target = source as IInputElement;
            }

            return CanExecute(source.Command, parameter, target);
        }

        public static bool CanExecute(ICommand command, object parameter, IInputElement target)
        {
            if (command == null)
            {
                return false;
            }

            var routedCommand = command as RoutedCommand;
            if (routedCommand == null)
            {
                return command.CanExecute(parameter);
            }

            return routedCommand.CanExecute(parameter, target);
        }

        public static void Execute(ICommandSource source) { Execute(source, source.CommandParameter); }

        public static void Execute(ICommandSource source, object parameter)
        {
            if (source == null)
            {
                return;
            }

            var cmd = source.Command;
            if (cmd == null)
            {
                return;
            }

            var target = source.CommandTarget;

            if (target == null &&
                Keyboard.FocusedElement == null)
            {
                target = source as IInputElement;
            }

            Execute(cmd, parameter, target);
        }

        public static void Execute(ICommand command, object parameter, IInputElement target)
        {
            if (command == null)
            {
                return;
            }

            var rcmd = command as RoutedCommand;
            if (rcmd == null)
            {
                command.Execute(parameter);
            }
            else
            {
                rcmd.Execute(parameter, target);
            }
        }

        public static void SetCurrentValue(CanExecuteRoutedEventArgs e, object value)
        {
            if (e.Parameter is CommandCanExecuteParameter)
            {
                (e.Parameter as CommandCanExecuteParameter).CurrentValue = value;
            }
        }
    }
}