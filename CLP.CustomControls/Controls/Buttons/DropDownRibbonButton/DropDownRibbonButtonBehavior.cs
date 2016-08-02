﻿using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Catel.Windows.Interactivity;
using Catel.Windows.Threading;

namespace CLP.CustomControls
{
    public class DropDownRibbonButtonBehavior : BehaviorBase<DropDownRibbonButton>
    {
        #region Methods
        protected override void OnAssociatedObjectLoaded()
        {
            base.OnAssociatedObjectLoaded();

            var binding = new Binding("DropDown.IsOpen")
            {
                Source = AssociatedObject,
                Mode = BindingMode.TwoWay
            };

            AssociatedObject.SetBinding(ToggleButton.IsCheckedProperty, binding);

            AssociatedObject.ToggleButton.Click += OnClick;

            var dropDown = AssociatedObject.DropDown;
            if (dropDown != null)
            {
                dropDown.Closed += OnDropDownClosed;
            }
        }

        protected override void OnAssociatedObjectUnloaded()
        {
            base.OnAssociatedObjectUnloaded();

            AssociatedObject.ToggleButton.Click -= OnClick;

            var dropDown = AssociatedObject.DropDown;
            if (dropDown != null)
            {
                dropDown.Closed -= OnDropDownClosed;
            }
        }

        private void OnDropDownClosed(object sender, RoutedEventArgs e)
        {
            AssociatedObject.ToggleButton.IsChecked = AssociatedObject.DropDown.IsOpen;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            var dropDown = AssociatedObject.DropDown;
            if (dropDown != null && (AssociatedObject.ToggleButton.IsChecked ?? false))
            {
                dropDown.Dispatcher.BeginInvoke(() =>
                {
                    dropDown.PlacementTarget = AssociatedObject;
                    dropDown.Placement = PlacementMode.Custom;
                    dropDown.MinWidth = AssociatedObject.ActualWidth;
                    dropDown.CustomPopupPlacementCallback = CustomPopupPlacementCallback;
                });

                dropDown.IsOpen = true;
            }
        }

        private static CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            var p = new Point
            {
                Y = targetSize.Height - offset.Y,
                X = -offset.X
            };

            return new[]
            {
                new CustomPopupPlacement(p, PopupPrimaryAxis.Horizontal)
            };
        }
        #endregion
    }
}
