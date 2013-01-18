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
            VerticalLine
        }

        public static string Type = "CLPShape";

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPShape(CLPShapeType shapeType, CLPPage page)
            : base(page)
        {
            ShapeType = shapeType;
            XPosition = 10;
            YPosition = 10;
            Height = 100;
            Width = 100;
            if(shapeType == CLP.Models.CLPShape.CLPShapeType.VerticalLine)
            {
                Width = 20;
            }
            if(shapeType == CLP.Models.CLPShape.CLPShapeType.HorizontalLine)
            {
                Height = 20;
            }
            Parts = 1;

            CLPPageObjectBase.ApplyDistinctPosition(this);
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
    }
}
