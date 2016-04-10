using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class TemporaryShape : Shape
    {
        public TemporaryShape(CLPPage parentPage, ShapeType shapeType)
            : base(parentPage, shapeType)
        {
            
        }
    }
}