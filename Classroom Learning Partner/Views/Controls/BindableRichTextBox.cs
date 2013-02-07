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
        private HashSet<Thread> _recursionProtection = new HashSet<Thread>();

        public BindableRichTextBox()
        {
            this.TextChanged += BindableRTB_TextChanged;
            BorderThickness = new Thickness(0.0);
            this.Loaded += BindableRichTextBox_Loaded;
        }

        void BindableRichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            RichTextBoxCommandBindings.IntializeCommandBindings(this);
        }

        void BindableRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange text = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();
            text.Save(stream, DataFormats.Rtf);

            this.RTFText = Encoding.UTF8.GetString(stream.ToArray());
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

        public static readonly DependencyProperty RTFTextProperty = DependencyProperty.RegisterAttached(
            "RTFText",
            typeof(string),
            typeof(BindableRichTextBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CallBack)
        );

        private static void CallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableRichTextBox rtb = d as BindableRichTextBox;

            if(rtb._recursionProtection.Contains(Thread.CurrentThread))
                return;

            try
            {
                string rtfText = e.NewValue as string;
                var doc = new FlowDocument();

                if(rtfText == "")
                {
                    //create rtf version of ""
                    //RichTextBox richTextBox = new RichTextBox();
                    //richTextBox.Text = "Européen";
                    //string rtfFormattedString = richTextBox.Rtf;
                }

                var bytes = Encoding.UTF8.GetBytes(rtfText);
                var stream = new MemoryStream(bytes);

                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                range.Load(stream, DataFormats.Rtf);

                // Set the document
                rtb.Document = doc;
            }
            catch(Exception)
            {
                //TODO: Steve - First Chance Exceptions are occurring here.
                rtb.Document = new FlowDocument();
            }
        }
    }

    static class RichTextBoxCommandBindings
    {
        public static void IntializeCommandBindings(RichTextBox richTextBox)
        {
            if(richTextBox == null) return;

            richTextBox.CommandBindings.Add(new System.Windows.Input.CommandBinding(
                                        EditingCommands.ToggleBold,
                                        (sender, e) => SetBold(richTextBox, !GetBold(richTextBox)),
                                        (sender, e) =>
                                        {
                                            CommandUtil.SetCurrentValue(e, GetBold(richTextBox));

                                            e.CanExecute = true;
                                            e.Handled = true;
                                        }));

            richTextBox.CommandBindings.Add(new CommandBinding(
                                        EditingCommands.ToggleItalic,
                                        (sender, e) => SetItalic(richTextBox, !GetItalic(richTextBox)),
                                        (sender, e) =>
                                        {
                                            CommandUtil.SetCurrentValue(e, GetItalic(richTextBox));

                                            e.CanExecute = true;
                                            e.Handled = true;
                                        }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                                MyCommands.SetFontSize,
                                                (sender, e) =>
                                                {
                                                    if(e.Parameter is double)
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

            richTextBox.CommandBindings.Add(new CommandBinding(
                                                MyCommands.SetFontColor,
                                                (sender, e) =>
                                                {
                                                    if(e.Parameter is Brush)
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

            richTextBox.CommandBindings.Add(new CommandBinding(
                                                MyCommands.SetFontFamily,
                                                (sender, e) =>
                                                {
                                                    if(e.Parameter is FontFamily)
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
            if(value != DependencyProperty.UnsetValue)
            {
                return (double)value;
            }
            return 12;
        }

        private static Color GetFontColor(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            if(value != DependencyProperty.UnsetValue)
            {
                if(value is SolidColorBrush)
                    return (value as SolidColorBrush).Color;
            }
            return Colors.Black;
        }

        private static void SetFontColor(RichTextBox richTextBox, Color c)
        {
            richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty,
                                                     new SolidColorBrush(c));
        }

        private static FontFamily GetFontFamily(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontFamilyProperty);
            if(value != DependencyProperty.UnsetValue)
            {
                return (FontFamily)value;
            }
            return new FontFamily("Arial");
        }

        private static void SetFontFamily(RichTextBox richTextBox, FontFamily font)
        {
            richTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty,
                                                     font);
        }

        private static void SetItalic(RichTextBox richTextBox, bool isItalic)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontStyleProperty,
                isItalic ? FontStyles.Italic : FontStyles.Normal);
        }

        private static bool GetItalic(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontStyleProperty);
            if(value != DependencyProperty.UnsetValue)
            {
                return (FontStyle)value == FontStyles.Italic;
            }
            return false;
        }

        private static void SetBold(RichTextBox richTextBox, bool isBold)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontWeightProperty,
                 isBold ? FontWeights.Bold : FontWeights.Normal);
        }

        private static bool GetBold(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontWeightProperty);
            if(value != DependencyProperty.UnsetValue)
            {
                return (FontWeight)value == FontWeights.Bold;
            }
            return false;
        }

        private static void SetFontSize(RichTextBox richTextBox, double size)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontSizeProperty, size);
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

        public readonly static DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(XComboBox),
            new PropertyMetadata(default(ICommand), new PropertyChangedCallback(OnCommandChanged)));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XComboBox owner = (XComboBox)d;
            owner.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        public static readonly RoutedEvent CommandChangedEvent = EventManager.RegisterRoutedEvent(
            "CommandChangedEvent",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<ICommand>),
            typeof(XComboBox));

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged
        {
            add { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            RoutedPropertyChangedEventArgs<ICommand> args = new RoutedPropertyChangedEventArgs<ICommand>(oldValue, newValue);
            args.RoutedEvent = XComboBox.CommandChangedEvent;
            RaiseEvent(args);

            if(cmd_CanExecuteChangedHandler == null)
                cmd_CanExecuteChangedHandler = cmd_CanExecuteChanged;
            if(oldValue != null)
                oldValue.CanExecuteChanged -= cmd_CanExecuteChangedHandler;
            if(newValue != null)
                newValue.CanExecuteChanged += cmd_CanExecuteChangedHandler;
            else
                cmd_CanExecuteChangedHandler = null;

            UpdateCanExecute();
        }
        // hold a reference to it, it might me stored as a weak reference and never be called otherwise...
        EventHandler cmd_CanExecuteChangedHandler;
        void cmd_CanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        protected virtual void UpdateCanExecute()
        {
            if(IsCommandExecuting)
                return;

            ICommand cmd = Command;
            if(cmd == null)
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
                    object value = ca.CurrentValue;

                    if(value is RangedValue)
                        value = ((RangedValue)value).Value;

                    object o = FindItemByData(value);

                    if(o == null && value == null)
                    {
                        Text = null;
                        SelectedItem = null;
                    }
                    else if(o != null && !Equals(SelectedItem, o))
                    {
                        SelectedItem = o;
                    }
                    else if(IsEditable && !IsReadOnly
                        && value != null && Text != value.ToString())
                    {
                        Text = value.ToString();
                    }
                }
                finally { IsCommandExecuting = false; }
            }
        }

        bool IsCommandExecuting { get; set; }

        #endregion

        #region CommandTarget dependency property and routed event

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public readonly static DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(XComboBox),
            new PropertyMetadata(default(IInputElement), new PropertyChangedCallback(OnCommandTargetChanged)));

        private static void OnCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XComboBox owner = (XComboBox)d;
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
            foreach(object o in Items)
                if(Equals(data, GetItemData(o)))
                    return o;
            return null;
        }

        public static object GetItemData(object obj)
        {
            ComboBoxItem cbi = obj as ComboBoxItem;
            if(cbi != null)
            {
                if(cbi.ReadLocalValue(FrameworkElement.TagProperty) != DependencyProperty.UnsetValue)
                    return cbi.GetValue(FrameworkElement.TagProperty);
                return cbi.Content;
            }
            return obj;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if(!IsCommandExecuting)
                try
                {
                    IsCommandExecuting = true;
                    CommandUtil.Execute(this);
                }
                finally { IsCommandExecuting = false; }
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
            : this(value, null, null)
        {
        }
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
        public CommandCanExecuteParameter(object parameter)
        {
            Parameter = parameter;
        }

        public object Parameter { get; private set; }
        public object CurrentValue { get; set; }
    }

    public static class CommandUtil
    {
        public static bool CanExecute(ICommandSource source)
        {
            return CanExecute(source.Command, source.CommandParameter, source.CommandTarget);
        }

        public static bool CanExecute(ICommandSource source, object parameter)
        {
            if(source == null
                || source.Command == null)
                return false;

            IInputElement target = source.CommandTarget;

            if(target == null && Keyboard.FocusedElement == null)
                target = source as IInputElement;

            return CanExecute(source.Command, parameter, target);
        }

        public static bool CanExecute(ICommand command, object parameter, IInputElement target)
        {
            if(command == null)
                return false;

            var routedCommand = command as RoutedCommand;
            if(routedCommand == null)
            {
                return command.CanExecute(parameter);
            }

            return routedCommand.CanExecute(parameter, target);
        }

        public static void Execute(ICommandSource source)
        {
            Execute(source, source.CommandParameter);
        }

        public static void Execute(ICommandSource source, object parameter)
        {
            if(source == null)
                return;

            ICommand cmd = source.Command;
            if(cmd == null)
                return;

            IInputElement target = source.CommandTarget;

            if(target == null && Keyboard.FocusedElement == null)
                target = source as IInputElement;

            Execute(cmd, parameter, target);
        }

        public static void Execute(ICommand command, object parameter, IInputElement target)
        {
            if(command == null)
                return;

            RoutedCommand rcmd = command as RoutedCommand;
            if(rcmd == null)
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
            if(e.Parameter is CommandCanExecuteParameter)
            {
                (e.Parameter as CommandCanExecuteParameter).CurrentValue = value;
            }
        }
    }
}
