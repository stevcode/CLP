using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPShape : CLPPageObjectBase
    {
        public enum CLPShapeType
        {
            Rectangle,
            Ellipse,
            Triangle,
            HorizontalLine
        }

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPShape(CLPShapeType shapeType)
            : base()
        {
            ShapeType = shapeType;
            Position = new System.Windows.Point(10, 10);
            Height = 100;
            Width = 100;
        }

        //Parameterless constructor for Protobuf
        private CLPShape()
            : base()
        { }

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
            get { return "CLPShape"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPShape newShape = this.Clone() as CLPShape;
            newShape.UniqueID = Guid.NewGuid().ToString();

            return newShape;
        }
    }
}
