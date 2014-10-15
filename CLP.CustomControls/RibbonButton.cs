using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CLP.CustomControls
{
    /// <summary></summary>
    public class RibbonButton : Button
    {
        static RibbonButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof (RibbonButton), new FrameworkPropertyMetadata(typeof (RibbonButton)));
        }

        #region Dependency Properties

        /// <summary>SUMMARY</summary>
        public ImageSource LargeImageSource
        {
            get { return (ImageSource)GetValue(LargeImageSourceProperty); }
            set { SetValue(LargeImageSourceProperty, value); }
        }

        public static readonly DependencyProperty LargeImageSourceProperty = DependencyProperty.Register("LargeImageSource",
                                                                                                         typeof (ImageSource),
                                                                                                         typeof (RibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>SUMMARY</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                                             typeof (string),
                                                                                             typeof (RibbonButton),
                                                                                             new UIPropertyMetadata(null));

        #endregion //Dependency Properties
    }
}