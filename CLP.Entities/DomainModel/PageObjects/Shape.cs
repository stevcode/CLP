﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    public enum ShapeType
    {
        Rectangle,
        Ellipse,
        Triangle,
        HorizontalLine,
        VerticalLine,
        Protractor
    }

    [Serializable]
    [RedirectType("CLP.Entities", "Shape")]
    public class Shape : APageObjectBase, ICuttable, ICountable
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="Shape" /> from scratch.
        /// </summary>
        public Shape() { }

        /// <summary>
        /// Initializes <see cref="Shape" /> from <see cref="ShapeType" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Shape" /> belongs to.</param>
        /// <param name="shapeType">The <see cref="ShapeType" /> of the <see cref="Shape" />.</param>
        public Shape(CLPPage parentPage, ShapeType shapeType)
            : base(parentPage) { ShapeType = shapeType; }

        /// <summary>
        /// Initializes <see cref="Shape" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Shape(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        /// <summary>
        /// The <see cref="ShapeType" /> of the <see cref="Shape" />.
        /// </summary>
        public ShapeType ShapeType
        {
            get { return GetValue<ShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(ShapeType), ShapeType.Rectangle);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newShape = Clone() as Shape;
            if(newShape == null)
            {
                return null;
            }
            newShape.CreationDate = DateTime.Now;
            newShape.ID = Guid.NewGuid().ToCompactID();
            newShape.VersionIndex = 0;
            newShape.LastVersionIndex = null;
            newShape.ParentPage = ParentPage;

            return newShape;
        }

        #endregion //Methods

        #region Implementation of ICuttable

        public List<IPageObject> Cut(Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition;
            var cuttableBottom = YPosition + Height;
            var cuttableLeft = XPosition;
            var cuttableRight = XPosition + Width;

            var halvedPageObjects = new List<IPageObject>();
            const double MIN_THRESHHOLD = 5.0;

            if(Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
               strokeRight <= cuttableRight &&
               strokeLeft >= cuttableLeft &&
               strokeTop - cuttableTop <= MIN_THRESHHOLD &&
               cuttableBottom - strokeBottom <= MIN_THRESHHOLD) //Vertical Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeRight + strokeLeft) / 2;

                switch(ShapeType)
                {
                    case ShapeType.VerticalLine:
                    case ShapeType.Protractor:
                        break;
                    case ShapeType.Rectangle:
                    case ShapeType.Ellipse:
                    case ShapeType.Triangle:
                    case ShapeType.HorizontalLine:
                        var leftShape = new Shape(ParentPage, ShapeType)
                                        {
                                            XPosition = XPosition,
                                            YPosition = YPosition,
                                            Width = average - XPosition,
                                            Height = Height
                                        };
                        var rightShape = new Shape(ParentPage, ShapeType)
                                         {
                                             XPosition = average,
                                             YPosition = YPosition,
                                             Width = XPosition + Width - average,
                                             Height = Height
                                         };
                        halvedPageObjects.Add(leftShape);
                        halvedPageObjects.Add(rightShape);
                        break;
                }
            }
            else if(Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                    strokeBottom <= cuttableBottom &&
                    strokeTop >= cuttableTop &&
                    cuttableRight - strokeRight <= MIN_THRESHHOLD &&
                    strokeLeft - cuttableLeft <= MIN_THRESHHOLD) //Horizontal Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeTop + strokeBottom) / 2;

                switch(ShapeType)
                {
                    case ShapeType.HorizontalLine:
                    case ShapeType.Protractor:
                        break;
                    case ShapeType.Rectangle:
                    case ShapeType.Ellipse:
                    case ShapeType.Triangle:
                    case ShapeType.VerticalLine:
                        var topShape = new Shape(ParentPage, ShapeType)
                                       {
                                           XPosition = XPosition,
                                           YPosition = YPosition,
                                           Width = Width,
                                           Height = average - YPosition
                                       };
                        var bottomShape = new Shape(ParentPage, ShapeType)
                                          {
                                              XPosition = XPosition,
                                              YPosition = average,
                                              Width = Width,
                                              Height = YPosition + Height - average
                                          };
                        halvedPageObjects.Add(topShape);
                        halvedPageObjects.Add(bottomShape);
                        break;
                }
            }

            return halvedPageObjects;
        }

        #endregion

        #region Implementation of ICountable

        /// <summary>
        /// Number of Parts the <see cref="ICountable" /> represents. <see cref="Shape" /> is always 1 Part.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 1);

        /// <summary>
        /// Signifies the <see cref="ICountable" /> has been accepted by another <see cref="ICountable" />.
        /// </summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof(bool), false);

        #endregion
    }
}