using System;
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

    public class PageInteractionService
    {
        private const double HIGHLIGHTER_SIZE = 12;

        public PageInteractionService() { }

        #region Properties

        public PageInteractionModes CurrentPageInteractionMode { get; private set; }

        public ErasingModes CurrentErasingMode { get; private set; }

        public InkCanvasEditingMode StrokeEraserMode { get; private set; }

        public double PenSize { get; private set; }

        public Color PenColor { get; private set; }

        public bool IsHighlighting { get; private set; }

        public DrawingAttributes InkDrawingAttributes { get; private set; }

        public ACLPPageBaseViewModel PageViewModel { get; private set; }

        #endregion //Properties

        #region Methods

        #region Set PageInteractionModes

        public void SetNoInteractionMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.None;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = InkCanvasEditingMode.None;
            PageViewModel.IsUsingCustomCursors = true;
            PageViewModel.PageCursor = Cursors.No;
            PageViewModel.ClearAdorners();
        }

        public void SetSelectMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Select;
            PageViewModel.IsInkCanvasHitTestVisible = false;
            PageViewModel.IsUsingCustomCursors = true;
            PageViewModel.PageCursor = Cursors.Hand;
        }

        public void SetPenMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Pen;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = InkCanvasEditingMode.Ink;
            PageViewModel.IsUsingCustomCursors = false;

            PageViewModel.DefaultDA.IsHighlighter = IsHighlighting;
            PageViewModel.DefaultDA.Height = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
            PageViewModel.DefaultDA.Width = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
            PageViewModel.DefaultDA.Color = PenColor;
            PageViewModel.DefaultDA.StylusTip = IsHighlighting ? StylusTip.Rectangle : StylusTip.Ellipse;
            PageViewModel.ClearAdorners();
        }

        public void SetEraserMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Eraser;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = StrokeEraserMode;
            PageViewModel.IsUsingCustomCursors = false;

            PageViewModel.DefaultDA.IsHighlighter = false;
            PageViewModel.DefaultDA.Height = PenSize;
            PageViewModel.DefaultDA.Width = PenSize;
            PageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
            PageViewModel.ClearAdorners();
        }

        public void SetLassoMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Lasso;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = InkCanvasEditingMode.Ink;
            PageViewModel.IsUsingCustomCursors = true;
            var lassoStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/LassoCursor.cur", UriKind.Relative));
            if (lassoStream != null)
            {
                PageViewModel.PageCursor = new Cursor(lassoStream.Stream);
            }

            PageViewModel.DefaultDA.IsHighlighter = false;
            PageViewModel.DefaultDA.Height = 2.0;
            PageViewModel.DefaultDA.Width = 2.0;
            PageViewModel.DefaultDA.Color = Colors.DarkGoldenrod;
            PageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
            PageViewModel.ClearAdorners();
        }

        public void SetCutMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.Cut;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = InkCanvasEditingMode.Ink;
            PageViewModel.IsUsingCustomCursors = true;
            var scissorsStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/ScissorsCursor.cur", UriKind.Relative));
            if (scissorsStream != null)
            {
                PageViewModel.PageCursor = new Cursor(scissorsStream.Stream);
            }

            PageViewModel.DefaultDA.IsHighlighter = false;
            PageViewModel.DefaultDA.Height = 2.0;
            PageViewModel.DefaultDA.Width = 2.0;
            PageViewModel.DefaultDA.Color = Colors.Black;
            PageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
            PageViewModel.ClearAdorners();
            if (!PageViewModel.IsPagePreview)
            {
                foreach (var array in PageViewModel.PageObjects.OfType<CLPArray>().Where(array => array.Rows < 71 && array.Columns < 71))
                {
                    array.IsGridOn = true;
                }
            }
        }

        public void SetDividerCreationMode()
        {
            CurrentPageInteractionMode = PageInteractionModes.DividerCreation;
            PageViewModel.IsInkCanvasHitTestVisible = true;
            PageViewModel.EditingMode = InkCanvasEditingMode.Ink;
            PageViewModel.PageCursor = Cursors.UpArrow;

            PageViewModel.DefaultDA.IsHighlighter = false;
            PageViewModel.DefaultDA.Height = 2.0;
            PageViewModel.DefaultDA.Width = 2.0;
            PageViewModel.DefaultDA.Color = Colors.Black;
            PageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
            PageViewModel.ClearAdorners();
            if (!PageViewModel.IsPagePreview)
            {
                foreach (var array in PageViewModel.PageObjects.OfType<CLPArray>().Where(array => array.Rows < 71 && array.Columns < 71))
                {
                    array.IsGridOn = true;
                }
            }
        }

        #endregion //Set PageInteractionModes

        #region Set Pen Properties

        public void SetPenSize(double penSize)
        {
            PenSize = penSize;
            InkDrawingAttributes.Height = penSize;
            InkDrawingAttributes.Width = penSize;
            PageViewModel.DefaultDA.Height = penSize;
            PageViewModel.DefaultDA.Width = penSize;
        }

        public void SetPenColor(Color color)
        {
            PenColor = color;
            PageViewModel.DefaultDA.Color = PenColor;
        }

        public void ToggleHighlighter()
        {
            IsHighlighting = !IsHighlighting;
            PageViewModel.DefaultDA.IsHighlighter = IsHighlighting;
            PageViewModel.DefaultDA.Height = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
            PageViewModel.DefaultDA.Width = IsHighlighting ? HIGHLIGHTER_SIZE : PenSize;
            PageViewModel.DefaultDA.StylusTip = IsHighlighting ? StylusTip.Rectangle : StylusTip.Ellipse;
        }

        #endregion //Set Pen Properties

        public void SetErasingMode(ErasingModes erasingMode) { CurrentErasingMode = erasingMode; }

        #endregion //Methods
    }
}