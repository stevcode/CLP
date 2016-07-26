using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CLP.CustomControls
{
    /// <summary></summary>
    public class DropDownRibbonButton : ToggleButton
    {
        static DropDownRibbonButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonButton), new FrameworkPropertyMetadata(typeof(RibbonButton)));
        }

        public DropDownRibbonButton() { SetResourceReference(BackgroundProperty, "DynamicMainColor"); }

        public DropDownRibbonButton(string text, string packUri, bool isContextButton = false)
        {
            Text = text;
            LargeImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            IsContextButton = isContextButton;
            SetResourceReference(BackgroundProperty, "DynamicMainColor");
        }

        #region Dependency Properties

        /// <summary>24x24 pixel image for the button.</summary>
        public ImageSource LargeImageSource
        {
            get { return (ImageSource)GetValue(LargeImageSourceProperty); }
            set { SetValue(LargeImageSourceProperty, value); }
        }

        public static readonly DependencyProperty LargeImageSourceProperty = DependencyProperty.Register("LargeImageSource",
                                                                                                         typeof(ImageSource),
                                                                                                         typeof(RibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>Text for the button.</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(RibbonButton), new UIPropertyMetadata(string.Empty));

        /// <summary>Text for the button.</summary>
        public bool IsContextButton
        {
            get { return (bool)GetValue(IsContextButtonProperty); }
            set { SetValue(IsContextButtonProperty, value); }
        }

        public static readonly DependencyProperty IsContextButtonProperty = DependencyProperty.Register("IsContextButton",
                                                                                                        typeof(bool),
                                                                                                        typeof(RibbonButton),
                                                                                                        new UIPropertyMetadata(false));

        #endregion //Dependency Properties
    }
}