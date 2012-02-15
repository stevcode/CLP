
using System;
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
