﻿using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Windows.Ink;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Collections.Generic;
using System;

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
    public class CLPPageViewModel : ViewModelBase
    {
        public static Guid StrokeIDKey = new Guid("03457307-3475-3450-3035-640435034540");

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel() : this(new CLPPage())
        {
        }

        public CLPPageViewModel(CLPPage page)
        {
            AppMessages.ChangeInkMode.Register(this, (newInkMode) =>
                                                                    {
                                                                        this.EditingMode = newInkMode;
                                                                    });

            Page = page;
            foreach (string stringStroke in page.Strokes)
            {
                Stroke stroke = StringToStroke(stringStroke);
                _strokes.Add(stroke);
            }
            foreach (var pageObject in page.PageObjects)
            {
                CLPPageObjectBaseViewModel pageObjectViewModel = null;
                if (pageObject is CLPImage)
                {
                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage);
                    
                }
                else if (pageObject is CLPImageStamp)
                {
                    pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp);
                }

                PageObjectContainerViewModel pageObjectContainer = new PageObjectContainerViewModel(pageObjectViewModel);

                PageObjectContainerViewModels.Add(pageObjectContainer);
            }

            _strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(_strokes_StrokesChanged);

            _historyVM = new CLPHistoryViewModel(page.PageHistory);
        }

        void _strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            List<string> removedStrokes = new List<string>();
            foreach (Stroke stroke in e.Removed)
            {

                string stringStroke = StrokeToString(stroke);
                if (Page.Strokes.Contains(stringStroke))
                {
                    removedStrokes.Add(stringStroke);
                    Page.Strokes.Remove(stringStroke);
                }
                else
                {
                    Console.WriteLine("Stroke does not exist on the CLPPage");
                }

            }

            List<string> addedStrokes = new List<string>();
            foreach (Stroke stroke in e.Added)
            {
                if (!stroke.ContainsPropertyData(StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    stroke.AddPropertyData(StrokeIDKey, newUniqueID);
                }
                string stringStroke = StrokeToString(stroke);
                addedStrokes.Add(stringStroke);
                Page.Strokes.Add(stringStroke);
            }
        }

        #endregion //Constructors

        #region Properties

        private CLPPage _page;
        public CLPPage Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }
        private CLPHistoryViewModel _historyVM = new CLPHistoryViewModel();
        public CLPHistoryViewModel HistoryVM
        {
            get
            {
                return _historyVM;
            }
            set
            {
                _historyVM = value;
            }
        }
        #endregion //Properties

        #region Bindings

        private StrokeCollection _strokes = new StrokeCollection();
        public StrokeCollection Strokes
        {
            get
            {
                return _strokes;
            }
        }

        private readonly ObservableCollection<PageObjectContainerViewModel> _pageObjectContainerViewModels = new ObservableCollection<PageObjectContainerViewModel>();
        public ObservableCollection<PageObjectContainerViewModel> PageObjectContainerViewModels
        {
            get
            {
                return _pageObjectContainerViewModels;
            }
        }

        /// <summary>
        /// The <see cref="EditingMode" /> property's name.
        /// </summary>
        public const string EditingModePropertyName = "EditingMode";

        private InkCanvasEditingMode _editingMode = InkCanvasEditingMode.None;

        /// <summary>
        /// Sets and gets the EditingMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get
            {
                return _editingMode;
            }

            set
            {
                if (_editingMode == value)
                {
                    return;
                }

                _editingMode = value;
                RaisePropertyChanged(EditingModePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DefaultDA" /> property's name.
        /// </summary>
        public const string DefaultDAPropertyName = "DefaultDA";

        private DrawingAttributes _defaultDrawingAttributes = new DrawingAttributes();

        /// <summary>
        /// Sets and gets the DefaultDA property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DrawingAttributes DefaultDA
        {
            get
            {
                return _defaultDrawingAttributes;
            }

            set
            {
                if (_defaultDrawingAttributes == value)
                {
                    return;
                }

                _defaultDrawingAttributes = value;
                RaisePropertyChanged(DefaultDAPropertyName);
            }
        }

        #endregion //Bindings

        #region Methods

        public static Stroke StringToStroke(string stroke)
        {
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            StrokeCollection sc = new StrokeCollection();
            sc = (StrokeCollection)converter.ConvertFromString(stroke);
            return sc[0];
        }

        public static string StrokeToString(Stroke stroke)
        {
            StrokeCollection sc = new StrokeCollection();
            sc.Add(stroke);
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            string stringStroke = (string)converter.ConvertToString(sc);
            return stringStroke;
        }

        /**
         * Helper method that converts a ObservableCollection of strings to a StrokeCollection
         */
        public static StrokeCollection StringsToStrokes(ObservableCollection<string> strings)
        {
            StrokeCollection strokes = new StrokeCollection();
            foreach (string s in strings)
            {
                strokes.Add(CLPPageViewModel.StringToStroke(s));
            }
            return strokes;
        }

        /**
         * Helper method that converts a StrokeCollection to an ObservableCollection of strings
         */
        public static ObservableCollection<string> StrokesToStrings(StrokeCollection strokes)
        {
            ObservableCollection<string> strings = new ObservableCollection<string>();
            foreach (Stroke stroke in strokes)
            {
                strings.Add(CLPPageViewModel.StrokeToString(stroke));
            }
            return strings;
        }

        #endregion //Methods
    }
}