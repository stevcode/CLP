using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public RibbonButton(string text, string packUri, ICommand command)
        {
            Text = text;
            LargeImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            Command = command;

            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["MainColor"] as Brush;
            Background = color;
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
                                                                                                         typeof (RibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>Text for the button.</summary>
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