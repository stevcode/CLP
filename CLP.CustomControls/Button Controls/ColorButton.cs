﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CLP.CustomControls.Button_Controls;

namespace CLP.CustomControls
{
    public class ColorButton : RadioButton, IPreferenceButton
    {
        private string ID;
        public string buttonID
        {
            get { return ID; }
            set { ID = value; }
        }

        static ColorButton()
        {
            // Initialize as a lookless control.
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ColorButton), new FrameworkPropertyMetadata(typeof (ColorButton)));
        }

        public ColorButton(Color color, string groupName = "PenColor")
        {
            Color = new SolidColorBrush(color);
            GroupName = groupName;
        }

        #region Dependency Properties

        /// <summary>Color of the Button</summary>
        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color",
                                                                                              typeof (SolidColorBrush),
                                                                                              typeof (ColorButton),
                                                                                              new UIPropertyMetadata(null));

        #endregion //Dependency Properties
    }
}