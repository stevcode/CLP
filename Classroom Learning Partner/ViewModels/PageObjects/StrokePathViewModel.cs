﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class StrokePathViewModel : CLPPageObjectBaseViewModel
    {
        #region Constructor

        public StrokePathViewModel(Geometry pathData, SolidColorBrush pathColor, double width, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            _pathData = pathData;
            _pathColor = pathColor;
            _pathWidth = width;
        }

        #endregion //Constructor

        #region Bindings

        private Geometry _pathData;
        public Geometry PathData
        {
            get { return _pathData; }
        }

        private SolidColorBrush _pathColor;
        public SolidColorBrush PathColor
        {
            get { return _pathColor; }
        }

        private double _pathWidth;
        public double PathWidth
        {
            get { return _pathWidth; }
        }

        #endregion //Bindings
    }
}