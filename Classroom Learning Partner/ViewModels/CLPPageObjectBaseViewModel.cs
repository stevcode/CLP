﻿using GalaSoft.MvvmLight;
using System.Windows.Ink;
using Classroom_Learning_Partner.Model;
using System.Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    abstract public class CLPPageObjectBaseViewModel : ViewModelBase
    {
        protected CLPPageObjectBaseViewModel()
        {
        }

        private CLPPageObjectBase _pageObject;
        public CLPPageObjectBase PageObject
        {
            get
            {
                return _pageObject;
            }
            set
            {
                _pageObject = value;
                this.Position = _pageObject.Position;
                this.Height = _pageObject.Height;
                this.Width = _pageObject.Width;
            }
        }

        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get { return _pageObjectStrokes; }
            protected set
            {
                _pageObjectStrokes = value;
            }
        }

        #region Bindings

        /// <summary>
        /// The <see cref="Height" /> property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        private double _height = 0;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                _height = value;
                RaisePropertyChanged(HeightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Width" /> property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        private double _width = 0;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
                RaisePropertyChanged(WidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Position" /> property's name.
        /// </summary>
        public const string PositionPropertyName = "Position";

        private Point _position = new Point(0,0);

        /// <summary>
        /// Sets and gets the Position property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Point Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                RaisePropertyChanged(PositionPropertyName);
            }
        }

        #endregion //Bindings

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedScribbles)
        {

        }

        protected virtual void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
        //    StrokeCollection strokesToRemove = new StrokeCollection();
        //    foreach (Stroke objectStroke in PageObjectStrokes)
        //    {

        //        string objectStrokeUniqueID = objectStroke.GetPropertyData(CLPPageViewModel.StrokeIDKey).ToString();
        //        foreach (Stroke pageStroke in removedStrokes)
        //        {
        //            string pageStrokeUniqueID = pageStroke.GetPropertyData(CLPPageViewModel.StrokeIDKey).ToString();
        //            if (objectStrokeUniqueID == pageStrokeUniqueID)
        //            {
        //                strokesToRemove.Add(objectStroke);
        //            }
        //        }
        //    }

        //    foreach (Stroke stroke in strokesToRemove)
        //    {
        //        PageObjectStrokes.Remove(stroke);
        //    }


        //    foreach (Stroke stroke in addedStrokes)
        //    {
        //        Stroke newStroke = stroke.Clone();
        //        Matrix transform = new Matrix();
        //        transform.Translate(OffsetX, OffsetY);
        //        newStroke.Transform(transform, true);
        //        PageObjectStrokes.Add(newStroke);
        //    }
        }
    }
}