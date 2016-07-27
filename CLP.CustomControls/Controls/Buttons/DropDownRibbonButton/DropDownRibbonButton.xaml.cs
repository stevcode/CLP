// --------------------------------------------------------------------------------------------------------------------
// Based on code from WildGums:
// https://github.com/WildGums/Orc.Controls/blob/develop/src/Orc.Controls/Orc.Controls.Shared/Controls/DropDownButton/Views/DropDownButton.xaml.cs
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Catel;

namespace CLP.CustomControls
{

    /// <summary>
    /// Interaction logic for DropDownRibbonButton.xaml
    /// </summary>
    public partial class DropDownRibbonButton
    {
        #region Constructors
        public DropDownRibbonButton()
        {
            InitializeComponent();

            LayoutUpdated += OnLayoutUpdated;
        }
        #endregion

        #region Properties
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object),
            typeof(DropDownRibbonButton), new FrameworkPropertyMetadata(string.Empty));

        public ContextMenu DropDown
        {
            get { return (ContextMenu)GetValue(DropDownProperty); }
            set { SetValue(DropDownProperty, value); }
        }

        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown", typeof(ContextMenu),
            typeof(DropDownRibbonButton), new UIPropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand),
            typeof(DropDownRibbonButton), new UIPropertyMetadata(null));

        public Brush AccentColorBrush
        {
            get { return (Brush)GetValue(AccentColorBrushProperty); }
            set { SetValue(AccentColorBrushProperty, value); }
        }

        public static readonly DependencyProperty AccentColorBrushProperty = DependencyProperty.Register("AccentColorBrush", typeof(Brush),
            typeof(DropDownRibbonButton), new PropertyMetadata(Brushes.LightGray, (sender, e) => ((DropDownRibbonButton)sender).OnAccentColorBrushChanged()));

        public bool ShowDefaultButton
        {
            get { return (bool)GetValue(ShowDefaultButtonProperty); }
            set { SetValue(ShowDefaultButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowDefaultButtonProperty = DependencyProperty.Register("ShowDefaultButton", typeof(bool),
            typeof(DropDownRibbonButton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool EnableTransparentBackground
        {
            get { return (bool)GetValue(EnableTransparentBackgroundProperty); }
            set { SetValue(EnableTransparentBackgroundProperty, value); }
        }

        public static readonly DependencyProperty EnableTransparentBackgroundProperty = DependencyProperty.Register("EnableTransparentBackground", typeof(bool),
            typeof(DropDownRibbonButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region Events
        public event EventHandler<EventArgs> ContentLayoutUpdated;
        #endregion

        #region Methods
        private void OnAccentColorBrushChanged()
        {
            var solidColorBrush = AccentColorBrush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var accentColor = ((SolidColorBrush)AccentColorBrush).Color;
                accentColor.CreateAccentColorResourceDictionary("DropDownButton");
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            ContentLayoutUpdated.SafeInvoke(this, e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AccentColorBrush = TryFindResource("AccentColorBrush") as SolidColorBrush;
        }
        #endregion
    }
}