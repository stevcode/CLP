using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Services
{
    public interface IPageInteractionService
    {
        PageInteractionModes CurrentPageInteractionMode { get; }
        ErasingModes CurrentErasingMode { get; }
        InkCanvasEditingMode StrokeEraserMode { get; }
        double PenSize { get; }
        Color PenColor { get; }
        bool IsHighlighting { get; }
        List<ACLPPageBaseViewModel> ActivePageViewModels { get; }

        void SetPageInteractionMode(PageInteractionModes pageInteractionMode);
        void SetNoInteractionMode();
        void SetSelectMode();
        void SetPenMode();
        void SetEraserMode();
        void SetLassoMode();
        void SetCutMode();
        void SetDividerCreationMode();
        void SetPenSize(double penSize);
        void SetPenColor(Color color);
        void ToggleHighlighter();
        void SetErasingMode(ErasingModes erasingMode);
    }
}