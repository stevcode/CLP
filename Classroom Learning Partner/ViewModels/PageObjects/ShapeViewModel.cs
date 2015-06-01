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
        /// <summary>
        /// Initializes a new instance of the ShapeViewModel class.
        /// </summary>
        public ShapeViewModel(Shape shape)
        {
            PageObject = shape;
            PageObject.IsManipulatableByNonCreator = true;

            ResizeShapeCommand = new Command<DragDeltaEventArgs>(OnResizeShapeCommandExecute);
            DuplicateShapeCommand = new Command(OnDuplicateShapeCommandExecute);
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Images/AddToDisplay.png", DuplicateShapeCommand, null, true));
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ShapeType ShapeType
        {
            get { return GetValue<ShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(ShapeType));

        public override string Title { get { return "ShapeVM"; } }

        /// <summary> 
        /// Gets the ResizeShapeCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeShapeCommand { get; set; }

        private void OnResizeShapeCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;
            var shape = PageObject as Shape;
            if(shape == null)
            {
                return;
            }

            const int MIN_WIDTH = 20;
            const int MIN_HEIGHT = 20;

            var newWidth = Math.Max(MIN_WIDTH, PageObject.Width + e.HorizontalChange);
            newWidth = shape.ShapeType == ShapeType.VerticalLine ? shape.Width : Math.Min(newWidth, parentPage.Width - PageObject.XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + e.VerticalChange);
            newHeight = shape.ShapeType == ShapeType.HorizontalLine ? shape.Height :  Math.Min(newHeight, parentPage.Height - PageObject.YPosition);

            // BUG: Steve - Protractor can be resized passed the Width of the page.
            if(shape.ShapeType == ShapeType.Protractor)
            {
                newWidth = 2.0 * newHeight;
            }
            
            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

        /// <summary>
        /// Duplicates the shape a given number of times.
        /// </summary>
        public Command DuplicateShapeCommand { get; private set; }

        private void OnDuplicateShapeCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
                         };
            keyPad.ShowDialog();
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfShapes = Int32.Parse(keyPad.NumbersEntered.Text);

            var xPosition = 10.0;
            var yPosition = 160.0;
            if(YPosition + 2 * Height + 10.0 < PageObject.ParentPage.Height)
            {
                yPosition = YPosition + Height + 10.0;
            }
            else if(XPosition + 2 * Width + 10.0 < PageObject.ParentPage.Width)
            {
                yPosition = YPosition;
                xPosition = XPosition + Width + 10.0;
            }
            
            var shapesToAdd = new List<Shape>();
            foreach(var index in Enumerable.Range(1, numberOfShapes))
            {
                var shape = PageObject.Duplicate() as Shape;
                shape.XPosition = xPosition;
                shape.YPosition = yPosition;

                if(xPosition + 2 * shape.Width <= PageObject.ParentPage.Width)
                {
                    xPosition += shape.Width;
                }
                    //If there isn't room, diagonally pile the rest
                else if((xPosition + shape.Width + 20.0 <= PageObject.ParentPage.Width) &&
                        (yPosition + shape.Height + 20.0 <= PageObject.ParentPage.Height))
                {
                    xPosition += 20.0;
                    yPosition += 20.0;
                }
                shapesToAdd.Add(shape);
            }

            if(shapesToAdd.Count == 1)
            {
                ACLPPageBaseViewModel.AddPageObjectToPage(shapesToAdd.First());
            }
            else
            {
                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, shapesToAdd);
            }
        }

        #region Static Methods

        public static void AddShapeToPage(CLPPage page, ShapeType shapeType)
        {
            var shape = new Shape(page, shapeType);
            ACLPPageBaseViewModel.AddPageObjectToPage(shape);
        } 

        #endregion //Static Methods
    }
}
