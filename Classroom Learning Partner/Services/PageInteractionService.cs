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
        Draw,
        Erase,
        Lasso,
        Cut,
        DividerCreation
    }

    public enum DrawModes
    {
        Pen,
        Marker,
        Highlighter
    }

    public enum ErasingModes
    {
        Ink,
        PageObjects,
        InkAndPageObjects,
        Dividers
    }

    public class PageInteractionService : IPageInteractionService
    {
        private const double PEN_SIZE = 2;
        private const double MARKER_SIZE = 5;
        private const double HIGHLIGHTER_SIZE = 12;

        public PageInteractionService() { InitializeDefaultInteractionMode(); }

        private void InitializeDefaultInteractionMode()
        {
            ActivePageViewModels = new List<ACLPPageBaseViewModel>();
            CurrentPageInteractionMode = PageInteractionModes.Draw;
            CurrentDrawMode = DrawModes.Pen;
            IsInkInteracting = true;
            CurrentErasingMode = ErasingModes.Ink;
            StrokeEraserMode = InkCanvasEditingMode.EraseByStroke;
            PenColor = Colors.Black;
        }

        #region Properties

        public PageInteractionModes CurrentPageInteractionMode { get; private set; }
        public DrawModes CurrentDrawMode { get; private set; }
        public bool IsInkInteracting { get; set; }
        public ErasingModes CurrentErasingMode { get; private set; }
        public InkCanvasEditingMode StrokeEraserMode { get; private set; }
        public Color PenColor { get; private set; }
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
                case PageInteractionModes.Draw:
                    SetDrawMode();
                    break;
                case PageInteractionModes.Erase:
                    SetEraseMode();
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

        public void SetDrawMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Draw;
            switch (CurrentDrawMode)
            {
                case DrawModes.Pen:
                    SetPenMode();
                    break;
                case DrawModes.Marker:
                    SetMarkerMode();
                    break;
                case DrawModes.Highlighter:
                    SetHighlighterMode();
                    break;
            }
        }

        public void SetEraseMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Erase;
            switch (CurrentErasingMode)
            {
                case ErasingModes.Ink:
                    SetInkEraserMode();
                    break;
                case ErasingModes.PageObjects:
                    SetPageObjectEraserMode();
                    break;
                case ErasingModes.Dividers:
                    SetDividerEraserMode();
                    break;
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
                pageViewModel.DefaultDA.Height = PEN_SIZE;
                pageViewModel.DefaultDA.Width = PEN_SIZE;
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
                pageViewModel.DefaultDA.Height = PEN_SIZE;
                pageViewModel.DefaultDA.Width = PEN_SIZE;
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
                pageViewModel.DefaultDA.Height = PEN_SIZE;
                pageViewModel.DefaultDA.Width = PEN_SIZE;
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

        #region Set Draw Properties

        public void SetPenColor(Color color)
        {
            PenColor = color;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.DefaultDA.Color = PenColor;
            }
        }

        public void SetPenMode()
        {
            CurrentDrawMode = DrawModes.Pen;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = PEN_SIZE;
                pageViewModel.DefaultDA.Width = PEN_SIZE;
                pageViewModel.DefaultDA.FitToCurve = true;
                pageViewModel.DefaultDA.Color = PenColor;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetMarkerMode()
        {
            CurrentDrawMode = DrawModes.Marker;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = MARKER_SIZE;
                pageViewModel.DefaultDA.Width = MARKER_SIZE;
                pageViewModel.DefaultDA.FitToCurve = true;
                pageViewModel.DefaultDA.Color = PenColor;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetHighlighterMode()
        {
            CurrentDrawMode = DrawModes.Highlighter;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = true;
                pageViewModel.DefaultDA.Height = HIGHLIGHTER_SIZE;
                pageViewModel.DefaultDA.Width = HIGHLIGHTER_SIZE;
                pageViewModel.DefaultDA.FitToCurve = false;
                pageViewModel.DefaultDA.Color = PenColor;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                pageViewModel.ClearAdorners();
            }
        }

        #endregion //Set Draw Properties

        #region Set Erase Properties

        public void SetInkEraserMode()
        {
            CurrentErasingMode = ErasingModes.Ink;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = true;
                pageViewModel.EditingMode = StrokeEraserMode;
                pageViewModel.IsUsingCustomCursors = false;

                pageViewModel.DefaultDA.IsHighlighter = false;
                pageViewModel.DefaultDA.Height = PEN_SIZE;
                pageViewModel.DefaultDA.Width = PEN_SIZE;
                pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                pageViewModel.ClearAdorners();
            }
        }

        public void SetPageObjectEraserMode()
        {
            CurrentErasingMode = ErasingModes.PageObjects;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = false;
                pageViewModel.IsUsingCustomCursors = true;
                pageViewModel.PageCursor = Cursors.Arrow;

                pageViewModel.ClearAdorners();
            }
        }

        public void SetDividerEraserMode()
        {
            CurrentErasingMode = ErasingModes.Dividers;
            foreach (var pageViewModel in ActivePageViewModels)
            {
                pageViewModel.IsInkCanvasHitTestVisible = false;
                pageViewModel.IsUsingCustomCursors = true;
                pageViewModel.PageCursor = Cursors.UpArrow;

                pageViewModel.ClearAdorners();
            }
        }

        #endregion //Set Erase Properties

        #endregion //Methods
    }
}