
using System;
using System.Runtime.Serialization;
namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPSquareShape : CLPPageObjectBase, ICLPPageObject
    {
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPSquareShape()
            : base()
        {
            Position = new System.Windows.Point(10, 10);
            Height = 300;
            Width = 300;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPSquareShape(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string PageObjectType
        {
            get { return "CLPSquareShape"; }
        }

        public ICLPPageObject Duplicate()
        {
            CLPSquareShape newSquareShape = this.Clone() as CLPSquareShape;
            newSquareShape.UniqueID = Guid.NewGuid().ToString();

            return newSquareShape;
        }
    }
}
