using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
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

    public class Shape : APageObjectBase
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
            newShape.ID = Guid.NewGuid().ToString();
            newShape.ParentPageID = ParentPageID;
            newShape.ParentPage = ParentPage;

            return newShape;
        }

        #endregion //Methods
    }
}