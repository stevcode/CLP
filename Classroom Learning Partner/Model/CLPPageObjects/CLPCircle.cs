using System.Windows;
using System;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPCircle : CLPPageObjectBase
    {
        public CLPCircle()
        {
            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }
    }
}
