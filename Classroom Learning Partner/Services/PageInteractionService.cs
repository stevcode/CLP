using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public enum PageInteractionModes
    {
        None,
        Select,
        Pen,
        Eraser,
        Lasso,
        Cut,
        DividerCreation
    }

    public enum ErasingModes
    {
        Ink,
        PageObjects,
        InkAndPageObjects
    }

    public class PageInteractionService : IPageInteractionService
    {
        private const double HIGHLIGHTER_SIZE = 12;

        public PageInteractionService() { InitializeDefaultInteractionMode(); }

        private void InitializeDefaultInteractionMode()
        {
            ActivePageViewModels = new List<ACLPPageBaseViewModel>();
            CurrentPageInteractionMode = PageInteractionModes.Pen;
            CurrentErasingMode = ErasingModes.Ink;
            StrokeEraserMode = InkCanvasEditingMode.EraseByStroke;
            PenSize = 2;
            PenColor = Colors.Black;
            IsHighlighting = false;
        }

        #region Properties

        public PageInteractionModes CurrentPageInteractionMode { get; private set; }

        public ErasingModes CurrentErasingMode { get; private set; }

        public InkCanvasEditingMode StrokeEraserMode { get; private set; }

        public double PenSize { get; private set; }

        public Color PenColor { get; private set; }

        public bool IsHighlighting { get; private set; }

        public List<ACLPPageBaseViewModel> ActivePageViewModels { get; private set; }

        #endregion //Properties

        #region Methods

        #region Set PageInteractionModes

        public void SetPageInteractionMode(PageInteractionModes pageInteractionMode)
        {
            switch (pageInteractionMode)
            {
                case PageInteractionModes.None:
                    SetNoInteractionMode();
                    break;
                case PageInteractionModes.Select:
                    SetSelectMode();
                    break;
                case PageInteractionModes.Pen:
                    SetPenMode();
                    break;
                case PageInteractionModes.Eraser:
                    SetEraserMode();
                    break;
                case PageInteractionModes.Lasso:
                    SetLassoMode();
                    break;
                case PageInteractionModes.Cut:
                    SetCutMode();
                    break;
                case PageInteractionModes.DividerCreation:
                    SetDividerCreationMode();
                    break;
            }
        }

        public void SetNoInteractionMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.None;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.None;
                pageViewModel.IsUsingCustomCursors = true;
                pageViewModel.PageCursor = Cursors.No;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetSelectMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Select;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = false;
                pageViewModel.IsUsingCustomCursors = true;
                pageViewModel.PageCursor = Cursors.Hand;
            }
        }

        public void SetPenMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Pen;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = IsHighlighting;
                pageViewModel.DefaultDA.Height = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
                pageViewModel.DefaultDA.Width = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
                pageViewModel.DefaultDA.FitToCurve = true;
                pageViewModel.DefaultDA.Color = PenColor;
                pageViewModel.DefaultDA.StylusTip = IsHighlighting ? StylusTip.Rectangle : StylusTip.Ellipse;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetEraserMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Eraser;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = StrokeEraserMode;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = PenSize;
                pageViewModel.DefaultDA.Width = PenSize;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetLassoMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Lasso;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = true;
                var lassoStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/LassoCursor.cur", UriKind.Relative));
                if (lassoStream != null)
                {
                    pageViewModel.PageCursor = new Cursor(lassoStream.Stream);
                }

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = 2.0;
                pageViewModel.DefaultDA.Width = 2.0;
                pageViewModel.DefaultDA.FitToCurve = false;
                pageViewModel.DefaultDA.Color = Colors.DarkGoldenrod;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetCutMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Cut;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = true;
                var scissorsStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/ScissorsCursor.cur", UriKind.Relative));
                if (scissorsStream != null)
                {
                    pageViewModel.PageCursor = new Cursor(scissorsStream.Stream);
                }

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = 2.0;
                pageViewModel.DefaultDA.Width = 2.0;
                pageViewModel.DefaultDA.FitToCurve = false;
                pageViewModel.DefaultDA.Color = Colors.Black;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                pageViewModel.ClearAdorners();
                if (!pageViewModel.IsPagePreview)
                {
                    foreach (var array in pageViewModel.PageObjects.OfType<CLPArray>().Where(array => array.Rows < 71 && array.Columns < 71))
                    {
                        array.IsGridOn = true;
                    }
                }
            }
        }

        public void SetDividerCreationMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.DividerCreation;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.PageCursor = Cursors.UpArrow;

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = 2.0;
                pageViewModel.DefaultDA.Width = 2.0;
                pageViewModel.DefaultDA.FitToCurve = false;
                pageViewModel.DefaultDA.Color = Colors.Black;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                pageViewModel.ClearAdorners();
                if (!pageViewModel.IsPagePreview)
                {
                    foreach (var array in pageViewModel.PageObjects.OfType<CLPArray>().Where(array => array.Rows < 71 && array.Columns < 71))
                    {
                        array.IsGridOn = true;
                    }
                }
            }
        }

        #endregion //Set PageInteractionModes

        #region Set Pen Properties

        public void SetPenSize(double penSize)
        {
            PenSize = penSize;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.DefaultDA.Height = penSize;
                pageViewModel.DefaultDA.Width = penSize;
            }
        }

        public void SetPenColor(Color color)
        {
            PenColor = color;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.DefaultDA.Color = PenColor;
            }
        }

        public void ToggleHighlighter()
        {
            IsHighlighting = !IsHighlighting;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.DefaultDA.IsHighlighter = IsHighlighting;
                pageViewModel.DefaultDA.Height = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
                pageViewModel.DefaultDA.Width = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
                pageViewModel.DefaultDA.FitToCurve = !IsHighlighting;
                pageViewModel.DefaultDA.StylusTip = IsHighlighting ? StylusTip.Rectangle : StylusTip.Ellipse;
            }
        }

        #endregion //Set Pen Properties

        public void SetErasingMode(ErasingModes erasingMode) { CurrentErasingMode = erasingMode; }

        #endregion //Methods
    }
}