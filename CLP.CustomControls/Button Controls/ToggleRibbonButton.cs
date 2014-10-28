using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CLP.CustomControls
{
    /// <summary></summary>
    public class ToggleRibbonButton : ToggleButton
    {
        static ToggleRibbonButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ToggleRibbonButton), new FrameworkPropertyMetadata(typeof (ToggleRibbonButton)));
        }

        public ToggleRibbonButton(string unCheckedText, string checkedText, string packUri, bool isContextButton = false)
        {
            UnCheckedText = unCheckedText;
            CheckedText = checkedText;
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
                                                                                                         typeof (ImageSource),
                                                                                                         typeof (ToggleRibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>UnCheckedText for the button.</summary>
        public string UnCheckedText
        {
            get { return (string)GetValue(UnCheckedTextProperty); }
            set { SetValue(UnCheckedTextProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedTextProperty = DependencyProperty.Register("UnCheckedText",
                                                                                                      typeof (string),
                                                                                                      typeof (ToggleRibbonButton),
                                                                                                      new UIPropertyMetadata(null));

        /// <summary>CheckedText for the button.</summary>
        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.Register("CheckedText",
                                                                                                    typeof (string),
                                                                                                    typeof (ToggleRibbonButton),
                                                                                                    new UIPropertyMetadata(null));

        /// <summary>Text for the button.</summary>
        public bool IsContextButton
        {
            get { return (bool)GetValue(IsContextButtonProperty); }
            set { SetValue(IsContextButtonProperty, value); }
        }

        public static readonly DependencyProperty IsContextButtonProperty = DependencyProperty.Register("IsContextButton",
                                                                                                        typeof (bool),
                                                                                                        typeof (ToggleRibbonButton),
                                                                                                        new UIPropertyMetadata(null));

        #endregion //Dependency Properties
    }
}