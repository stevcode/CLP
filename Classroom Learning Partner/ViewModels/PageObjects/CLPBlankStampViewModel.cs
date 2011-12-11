﻿using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{

    public class CLPBlankStampViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPBlankStampViewModel class.
        /// </summary>
        public CLPBlankStampViewModel(CLPBlankStamp stamp, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            _isAnchored = stamp.IsAnchored;
            _parts = stamp.Parts;
            PageObjectStrokes = CLPPageViewModel.StringsToStrokes(stamp.PageObjectStrokes);
            PageObject = stamp;

            if (!_isAnchored)
            {
                ScribblesToStrokePaths();
            }
        }



        #region Bindings

        private ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        {
            get { return _strokePathViewModels; }
        }

        /// <summary>
        /// The <see cref="IsAnchor" /> property's name.
        /// </summary>
        public const string IsAnchorPropertyName = "IsAnchored";

        private bool _isAnchored = true;

        /// <summary>
        /// Sets and gets the IsAnchor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAnchored
        {
            get
            {
                return _isAnchored;
            }

            set
            {
                if (_isAnchored == value)
                {
                    return;
                }

                _isAnchored = value;
                RaisePropertyChanged(IsAnchorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Parts" /> property's name.
        /// </summary>
        public const string PartsPropertyName = "Parts";

        private int _parts = 0;

        /// <summary>
        /// Sets and gets the Parts property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Parts
        {
            get
            {
                return _parts;
            }

            set
            {
                if (_parts == value)
                {
                    return;
                }

                _parts = value;
                RaisePropertyChanged(PartsPropertyName);
            }
        }

        #endregion //Bindings

        #region Methods

        public void ScribblesToStrokePaths()
        {
            if (!_isAnchored)
            {
                foreach (Stroke stroke in PageObjectStrokes)
                {
                    StylusPoint firstPoint = stroke.StylusPoints[0];

                    StreamGeometry geometry = new StreamGeometry();
                    using (StreamGeometryContext geometryContext = geometry.Open())
                    {
                        geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
                        foreach (StylusPoint point in stroke.StylusPoints)
                        {
                            geometryContext.LineTo(new Point(point.X, point.Y), true, true);
                        }
                    }
                    geometry.Freeze();

                    StrokePathViewModel strokePathViewModel = new StrokePathViewModel(geometry, (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString()), stroke.DrawingAttributes.Width, PageViewModel);
                    StrokePathViewModels.Add(strokePathViewModel);
                }
            }
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            if (IsAnchored)
            {
                this.ProcessStrokes(addedStrokes, removedStrokes);
            }
        }


        #endregion //Methods
    }
}