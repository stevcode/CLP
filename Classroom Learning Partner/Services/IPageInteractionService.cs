using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Services
{
    public interface IPageInteractionService
    {
        PageInteractionModes CurrentPageInteractionMode { get; }
        DrawModes CurrentDrawMode { get; }
        ErasingModes CurrentErasingMode { get; }
        InkCanvasEditingMode StrokeEraserMode { get; }
        Color PenColor { get; }
        List<ACLPPageBaseViewModel> ActivePageViewModels { get; }

        void SetPageInteractionMode(PageInteractionModes pageInteractionMode);
        void SetNoInteractionMode();
        void SetSelectMode();
        void SetDrawMode();
        void SetEraseMode();
        void SetLassoMode();
        void SetCutMode();
        void SetDividerCreationMode();
        void SetPenColor(Color color);
        void SetPenMode();
        void SetMarkerMode();
        void SetHighlighterMode();
        void SetErasingMode(ErasingModes erasingMode);
    }
}