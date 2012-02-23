using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPSquare : CLPPageObjectBase
    {
        public CLPSquare()
        {
            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }
    }
}
