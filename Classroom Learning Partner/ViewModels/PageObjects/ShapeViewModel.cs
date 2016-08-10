using System;
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

            _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Resources/Images/AddToDisplay.png", DuplicateShapeCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _toggleIsDashedButton = new ToggleRibbonButton("Solid", "Dashed", "pack://application:,,,/Resources/Images/ToggleDashedStroke64.png", true)
                                    {
                                        IsChecked = IsStrokeDashed
                                    };
            _toggleIsDashedButton.Checked += toggleIsDashedButton_Checked;
            _toggleIsDashedButton.Unchecked += toggleIsDashedButton_Checked;
            _contextButtons.Add(_toggleIsDashedButton);

            if (ShapeType != ShapeType.HorizontalLine)
            {
                return;
            }

            _contextButtons.Add(new RibbonButton("Rotate", "pack://application:,,,/Resources/Images/AdornerImages/ArrayRotate64.png", RotateShapeCommand, null, true));
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
            double newWidth;
            double newHeight;

            if (ShapeType == ShapeType.Triangle ||
                ShapeType == ShapeType.LeftDiagonal ||
                ShapeType == ShapeType.RightDiagonal)
            {
                var diagonalChange = Math.Max(e.HorizontalChange, e.VerticalChange);
                newWidth = Math.Max(MIN_WIDTH, PageObject.Width + diagonalChange);
                newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + diagonalChange);
                var sideOverlap = PageObject.XPosition + newWidth - parentPage.Width;
                var bottomOverlap = PageObject.YPosition + newHeight - parentPage.Height;
                if (sideOverlap > 0 ||
                    bottomOverlap > 0)
                {
                    var overlapOffset = Math.Max(sideOverlap, bottomOverlap);
                    newWidth -= overlapOffset;
                    newHeight -= overlapOffset;
                }
            }
            else
            {
                newWidth = Math.Max(MIN_WIDTH, PageObject.Width + e.HorizontalChange);
                newWidth = shape.ShapeType == ShapeType.VerticalLine ? shape.Width : Math.Min(newWidth, parentPage.Width - PageObject.XPosition);
                newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + e.VerticalChange);
                newHeight = shape.ShapeType == ShapeType.HorizontalLine ? shape.Height : Math.Min(newHeight, parentPage.Height - PageObject.YPosition);
            }

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
            if (ShapeType == ShapeType.HorizontalLine)
            {
                ShapeType = ShapeType.RightDiagonal;
                Height = Math.Sqrt(Math.Pow(Width, 2) / 2);
                Width = Height;
            }
            else if (ShapeType == ShapeType.RightDiagonal)
            {
                ShapeType = ShapeType.VerticalLine;
                Height = Math.Sqrt(2 * Math.Pow(Width, 2));
                Width = Shape.MIN_LINE_DEPTH;
            }
            else if (ShapeType == ShapeType.VerticalLine)
            {
                ShapeType = ShapeType.LeftDiagonal;
                Width = Math.Sqrt(Math.Pow(Height, 2) / 2);
                Height = Width;
            }
            else
            {
                ShapeType = ShapeType.HorizontalLine;
                Width = Math.Sqrt(2 * Math.Pow(Height, 2));
                Height = Shape.MIN_LINE_DEPTH;
            }

            if (XPosition + Width > PageObject.ParentPage.Width)
            {
                XPosition = PageObject.ParentPage.Width - Width;
            }
            if (YPosition + Height > PageObject.ParentPage.Height)
            {
                YPosition = PageObject.ParentPage.Height - Height;
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