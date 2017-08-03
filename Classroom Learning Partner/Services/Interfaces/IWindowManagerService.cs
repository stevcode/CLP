using System;

namespace Classroom_Learning_Partner.Services
{
    public interface IWindowManagerService
    {
        bool IsNotebookPagesPanelVisible { get; set; }
        bool IsProgressPanelVisible { get; set; }
        bool IsPageInformationPanelVisible { get; set; }
        bool IsDisplaysPanelVisible { get; set; }

        event EventHandler<EventArgs> NotebookPagesPanelVisibleChanged;
        event EventHandler<EventArgs> ProgressPanelVisibleChanged;
        event EventHandler<EventArgs> PageInformationPanelVisibleChanged;
        event EventHandler<EventArgs> DisplaysPanelVisibleChanged;
    }
}