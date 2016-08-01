// --------------------------------------------------------------------------------------------------------------------
// Based on code from WildGums:
// https://github.com/WildGums/Orc.Controls/blob/develop/src/Orc.Controls/Orc.Controls.Shared/Controls/DropDownButton/Views/DropDownButton.xaml.cs
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace CLP.CustomControls
{
    /// <summary>Interaction logic for DropDownRibbonButton.xaml</summary>
    public class DropDownRibbonButton : ToggleButton
    {
        #region Constructors

        static DropDownRibbonButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownRibbonButton), new FrameworkPropertyMetadata(typeof(DropDownRibbonButton)));
        }

        public DropDownRibbonButton()
        {
            SetResourceReference(BackgroundProperty, "DynamicMainColor");

            var behavior = new DropDownRibbonButtonBehavior();
            Interaction.GetBehaviors(this).Add(behavior);
        }

        public DropDownRibbonButton(string text, string packUri, bool isContextButton = false)
            : this()
        {
            Text = text;
            LargeImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            IsContextButton = isContextButton;
        }

        #endregion

        #region Properties

        /// <summary>24x24 pixel image for the button.</summary>
        public ImageSource LargeImageSource
        {
            get { return (ImageSource)GetValue(LargeImageSourceProperty); }
            set { SetValue(LargeImageSourceProperty, value); }
        }

        public static readonly DependencyProperty LargeImageSourceProperty = DependencyProperty.Register("LargeImageSource",
                                                                                                         typeof(ImageSource),
                                                                                                         typeof(DropDownRibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>Text for the button.</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropDownRibbonButton), new UIPropertyMetadata(string.Empty));

        public ContextMenu DropDown
        {
            get { return (ContextMenu)GetValue(DropDownProperty); }
            set { SetValue(DropDownProperty, value); }
        }

        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown", typeof(ContextMenu), typeof(DropDownRibbonButton), new UIPropertyMetadata(null));

        /// <summary>Allows for special behavior if button is in the Context Ribbon.</summary>
        public bool IsContextButton
        {
            get { return (bool)GetValue(IsContextButtonProperty); }
            set { SetValue(IsContextButtonProperty, value); }
        }

        public static readonly DependencyProperty IsContextButtonProperty = DependencyProperty.Register("IsContextButton", typeof(bool), typeof(DropDownRibbonButton), new UIPropertyMetadata(false));

        #endregion
    }
}