using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CLP.CustomControls
{
    /// <summary></summary>
    public class GroupedRibbonButton : RadioButton
    {
        static GroupedRibbonButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof (GroupedRibbonButton), new FrameworkPropertyMetadata(typeof (GroupedRibbonButton)));
        }

        public GroupedRibbonButton(string text, string groupName, string packUri, string enumValue)
        {
            Text = text;
            GroupName = groupName;
            LargeImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            AssociatedEnumValue = enumValue;

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
                                                                                                         typeof (GroupedRibbonButton),
                                                                                                         new UIPropertyMetadata(null));

        /// <summary>Text for the button.</summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                                             typeof (string),
                                                                                             typeof (GroupedRibbonButton),
                                                                                             new UIPropertyMetadata(null));

        /// <summary>EnumValue compared against when IsChecked == true.</summary>
        public string AssociatedEnumValue
        {
            get { return (string)GetValue(AssociatedEnumValueProperty); }
            set { SetValue(AssociatedEnumValueProperty, value); }
        }

        public static readonly DependencyProperty AssociatedEnumValueProperty = DependencyProperty.Register("AssociatedEnumValue",
                                                                                                            typeof (string),
                                                                                                            typeof (GroupedRibbonButton),
                                                                                                            new UIPropertyMetadata(null));

        #endregion //Dependency Properties
    }
}