using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPShape : ACLPPageObjectBase
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

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPShape(CLPShapeType shapeType, ICLPPage page)
            : base(page)
        {
            ShapeType = shapeType;
            XPosition = 10;
            YPosition = 10;
            Height = shapeType == CLPShapeType.HorizontalLine? 20 : 200;
            Width = shapeType == CLPShapeType.VerticalLine ? 20 : 200;
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
        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(CLPShapeType));

        public override string PageObjectType
        {
            get { return "CLPShape"; }
        }

        public override ICLPPageObject Duplicate()
        {
            var newShape = Clone() as CLPShape;
            if(newShape == null)
            {
                return null;
            }
            newShape.UniqueID = Guid.NewGuid().ToString();
            newShape.ParentPage = ParentPage;

            return newShape;
        }

        public override List<ICLPPageObject> Cut(Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition;
            var cuttableBottom = YPosition + Height;
            var cuttableLeft = XPosition;
            var cuttableRight = XPosition + Width;

            var halvedPageObjects = new List<ICLPPageObject>();

            //TODO: Tim - This is fine for now, but you could have an instance where a really wide, but short rectangle is made
            // and a stroke could be made that was only a few pixels high, and quite wide, that would try to make a horizontal
            // cut instead of the vertical cut that was intended. See also Array.Cut().
            if(Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom))
            {
                var average = (strokeRight + strokeLeft)/2;

                if((cuttableLeft <= strokeLeft && cuttableRight >= strokeRight) &&
                   (strokeTop - cuttableTop < 15 && cuttableBottom - strokeBottom < 15))
                {
                    switch(ShapeType)
                    {
                            //TODO: Implement Cut functions for all instances of SHAPE
                        case CLPShapeType.Rectangle:
                            var leftSquare = new CLPShape(CLPShapeType.Rectangle, ParentPage)
                                             {
                                                 XPosition = XPosition,
                                                 YPosition = YPosition,
                                                 Width = average - XPosition,
                                                 Height = Height
                                             };
                            var rightSquare = new CLPShape(CLPShapeType.Rectangle, ParentPage)
                                              {
                                                  XPosition = average,
                                                  YPosition = YPosition,
                                                  Width = XPosition + Width - average,
                                                  Height = Height
                                              };
                            halvedPageObjects.Add(leftSquare);
                            halvedPageObjects.Add(rightSquare);
                            break;
                        case CLPShapeType.Ellipse:
                            break;
                        case CLPShapeType.Triangle:
                            break;
                        case CLPShapeType.HorizontalLine:
                            break;
                        case CLPShapeType.VerticalLine:
                            break;
                        case CLPShapeType.Protractor:
                            break;
                    }
                }
            }
            else
            {
                var average = (strokeTop + strokeBottom) / 2;

                if((cuttableTop <= strokeTop && cuttableBottom >= strokeBottom) &&
                   (strokeLeft - cuttableLeft < 15 && cuttableRight - strokeRight < 15))
                {
                    switch(ShapeType)
                    {
                        //TODO: Implement Cut functions for all instances of SHAPE
                        case CLPShapeType.Rectangle:
                            var leftSquare = new CLPShape(CLPShapeType.Rectangle, ParentPage)
                                             {
                                                 XPosition = XPosition,
                                                 YPosition = YPosition,
                                                 Width = Width,
                                                 Height = average - YPosition
                                             };
                            var rightSquare = new CLPShape(CLPShapeType.Rectangle, ParentPage)
                                              {
                                                  XPosition = XPosition,
                                                  YPosition = average,
                                                  Width = Width,
                                                  Height = YPosition + Height - average
                                              };
                            halvedPageObjects.Add(leftSquare);
                            halvedPageObjects.Add(rightSquare);
                            break;
                        case CLPShapeType.Ellipse:
                            break;
                        case CLPShapeType.Triangle:
                            break;
                        case CLPShapeType.HorizontalLine:
                            break;
                        case CLPShapeType.VerticalLine:
                            break;
                        case CLPShapeType.Protractor:
                            break;
                    }
                }
            }

            return halvedPageObjects;
        }
    }
}
