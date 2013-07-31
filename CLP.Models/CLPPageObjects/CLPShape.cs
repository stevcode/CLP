using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPShape : CLPPageObjectBase
    {
        public enum CLPShapeType
        {
            Rectangle,
            Ellipse,
            Triangle,
            HorizontalLine,
            VerticalLine,
            Protractor
        }

        public static string Type = "CLPShape";

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPShape(CLPShapeType shapeType, ICLPPage page)
            : base(page)
        {
            ShapeType = shapeType;
            XPosition = 10;
            YPosition = 10;
            Height = 200;
            Width = 200;
            if(shapeType == CLPShapeType.VerticalLine)
            {
                Width = 20;
            }
            if(shapeType == CLPShapeType.HorizontalLine)
            {
                Height = 20;
            }
            Parts = 1;

            ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPShape(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPShapeType ShapeType
        {
            get { return GetValue<CLPShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        /// <summary>
        /// Register the ShapeType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(CLPShapeType), null);

        public override string PageObjectType
        {
            get { return Type; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPShape newShape = this.Clone() as CLPShape;
            newShape.UniqueID = Guid.NewGuid().ToString();
            newShape.ParentPage = ParentPage;

            return newShape;
        }
        
        public override System.Collections.ObjectModel.ObservableCollection<ICLPPageObject> SplitAtX(double ave)
        {
            System.Collections.ObjectModel.ObservableCollection<ICLPPageObject> c = new System.Collections.ObjectModel.ObservableCollection<ICLPPageObject>();
            if(CLPShapeType.Rectangle.CompareTo(this.ShapeType) == 0)
            {
                Console.Write("Shape identified correctly");
                double shape1X = this.XPosition;
                double shape1Y = this.YPosition;
                double shape2Y = this.YPosition;
                double shape2X = ave;
                double shapeHeight = this.Height;
                double shape1Width = ave - shape1X;
                double shape2Width = this.XPosition + this.Width - ave;
                CLPShape square1 = new CLPShape(CLPShapeType.Rectangle, this.ParentPage);
                square1.XPosition = shape1X;
                square1.YPosition = shape1Y;
                square1.Width = shape1Width;
                square1.Height = shapeHeight;
                CLPShape square2 = new CLPShape(CLPShapeType.Rectangle, this.ParentPage);
                square2.XPosition = shape2X;
                square2.YPosition = shape2Y;
                square2.Width = shape2Width;
                square2.Height = shapeHeight;
                c.Add(square1);
                c.Add(square2);

            }
            return c;
            //throw new NotImplementedException();
        }

        public override System.Collections.ObjectModel.ObservableCollection<ICLPPageObject> SplitAtY(double Ave)
        {
            System.Collections.ObjectModel.ObservableCollection<ICLPPageObject> c = new System.Collections.ObjectModel.ObservableCollection<ICLPPageObject>();
            if(CLPShapeType.Rectangle.CompareTo(this.ShapeType) == 0)
            {
                Console.Write("Shape identified correctly");
                double shapeX = this.XPosition;
                double shape1Y = this.YPosition;
                double shape2Y = Ave;
                double shape1Height = Ave - shape1Y;
                double shape2Height = this.Height - shape1Height;
                double shapeWidth = this.Width;
                
                CLPShape square1 = new CLPShape(CLPShapeType.Rectangle, this.ParentPage);
                square1.XPosition = shapeX;
                square1.YPosition = shape1Y;
                square1.Width = shapeWidth;
                square1.Height = shape1Height;
                
                CLPShape square2 = new CLPShape(CLPShapeType.Rectangle, this.ParentPage);
                square2.XPosition = shapeX;
                square2.YPosition = shape2Y;
                square2.Width = shapeWidth;
                square2.Height = shape2Height;
                c.Add(square1);
                c.Add(square2);
            }
            return c;
        }  

    }
}
