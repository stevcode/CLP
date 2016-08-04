﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ShapeViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>Initializes a new instance of the ShapeViewModel class.</summary>
        public ShapeViewModel(Shape shape)
        {
            PageObject = shape;
            PageObject.IsManipulatableByNonCreator = true;

            ResizeShapeCommand = new Command<DragDeltaEventArgs>(OnResizeShapeCommandExecute);
            DuplicateShapeCommand = new Command(OnDuplicateShapeCommandExecute);
            RotateShapeCommand = new Command(OnRotateShapeCommandExecute);
            InitializeButtons();
        }

        public override string Title
        {
            get { return "ShapeVM"; }
        }

        #endregion // Constructor

        #region Buttons

        private ToggleRibbonButton _toggleIsDashedButton;

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Images/AddToDisplay.png", DuplicateShapeCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Rotate", "pack://application:,,,/Resources/Images/AdornerImages/ArrayRotate64.png", RotateShapeCommand, null, true));

            _toggleIsDashedButton = new ToggleRibbonButton("Solid", "Dashed", "pack://application:,,,/Resources/Images/ToggleDashedStroke64.png", true)
                                    {
                                        IsChecked = IsStrokeDashed
                                    };
            _toggleIsDashedButton.Checked += toggleIsDashedButton_Checked;
            _toggleIsDashedButton.Unchecked += toggleIsDashedButton_Checked;
            _contextButtons.Add(_toggleIsDashedButton);
        }

        private void toggleIsDashedButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var shape = PageObject as Shape;
            if (shape == null)
            {
                return;
            }

            shape.IsStrokeDashed = (bool)toggleButton.IsChecked;
        }

        #endregion // Buttons

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public ShapeType ShapeType
        {
            get { return GetValue<ShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(ShapeType));

        /// <summary>Determines if the stroke used to make the shape is a dashed line or solid.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsStrokeDashed
        {
            get { return GetValue<bool>(IsStrokeDashedProperty); }
            set { SetValue(IsStrokeDashedProperty, value); }
        }

        public static readonly PropertyData IsStrokeDashedProperty = RegisterProperty("IsStrokeDashed", typeof(bool));

        /// <summary>Degree the shape has been rotated.</summary>
        [ViewModelToModel("PageObject")]
        public double RotationDegree
        {
            get { return GetValue<double>(RotationDegreeProperty); }
            set { SetValue(RotationDegreeProperty, value); }
        }

        public static readonly PropertyData RotationDegreeProperty = RegisterProperty("RotationDegree", typeof(double));

        #endregion // Model

        #region Commands

        /// <summary>Gets the ResizeShapeCommand command.</summary>
        public Command<DragDeltaEventArgs> ResizeShapeCommand { get; set; }

        private void OnResizeShapeCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;
            var shape = PageObject as Shape;
            if (shape == null)
            {
                return;
            }

            const int MIN_WIDTH = 20;
            const int MIN_HEIGHT = 20;

            var newWidth = Math.Max(MIN_WIDTH, PageObject.Width + e.HorizontalChange);
            newWidth = shape.ShapeType == ShapeType.VerticalLine ? shape.Width : Math.Min(newWidth, parentPage.Width - PageObject.XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + e.VerticalChange);
            newHeight = shape.ShapeType == ShapeType.HorizontalLine ? shape.Height : Math.Min(newHeight, parentPage.Height - PageObject.YPosition);

            // BUG: Steve - Protractor can be resized passed the Width of the page.
            if (shape.ShapeType == ShapeType.Protractor)
            {
                newWidth = 2.0 * newHeight;
            }

            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

        /// <summary>Duplicates the shape a given number of times.</summary>
        public Command DuplicateShapeCommand { get; private set; }

        private void OnDuplicateShapeCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfShapes = int.Parse(keyPad.NumbersEntered.Text);

            var xPosition = 10.0;
            var yPosition = 160.0;
            if (YPosition + 2 * Height + 10.0 < PageObject.ParentPage.Height)
            {
                yPosition = YPosition + Height + 10.0;
            }
            else if (XPosition + 2 * Width + 10.0 < PageObject.ParentPage.Width)
            {
                yPosition = YPosition;
                xPosition = XPosition + Width + 10.0;
            }

            var shapesToAdd = new List<Shape>();
            foreach (var index in Enumerable.Range(1, numberOfShapes))
            {
                var shape = PageObject.Duplicate() as Shape;
                shape.XPosition = xPosition;
                shape.YPosition = yPosition;

                if (xPosition + 2 * shape.Width <= PageObject.ParentPage.Width)
                {
                    xPosition += shape.Width;
                }
                //If there isn't room, diagonally pile the rest
                else if ((xPosition + shape.Width + 20.0 <= PageObject.ParentPage.Width) &&
                         (yPosition + shape.Height + 20.0 <= PageObject.ParentPage.Height))
                {
                    xPosition += 20.0;
                    yPosition += 20.0;
                }
                shapesToAdd.Add(shape);
            }

            if (shapesToAdd.Count == 1)
            {
                ACLPPageBaseViewModel.AddPageObjectToPage(shapesToAdd.First());
            }
            else
            {
                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, shapesToAdd);
            }
        }

        /// <summary>Rotates the shape by 45 degrees.</summary>
        public Command RotateShapeCommand { get; private set; }

        private void OnRotateShapeCommandExecute()
        {
            if (RotationDegree >= 360)
            {
                RotationDegree = 0.0;
            }
            else
            {
                RotationDegree += 45.0;
            }
        }

        #endregion // Commands

        #region Static Methods

        public static void AddShapeToPage(CLPPage page, ShapeType shapeType)
        {
            var shape = new Shape(page, shapeType);
            ApplyDistinctPosition(shape);
            ACLPPageBaseViewModel.AddPageObjectToPage(shape);
        }

        #endregion //Static Methods
    }
}