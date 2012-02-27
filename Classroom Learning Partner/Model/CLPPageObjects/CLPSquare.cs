﻿using System.Windows;
using System;

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
