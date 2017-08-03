using System;

namespace Classroom_Learning_Partner.Services
{
    public interface IWindowManagerService
    {
        bool IsPageInformationPanelVisible { get; set; }
        bool IsDisplaysPanelVisible { get; set; }

        event EventHandler<EventArgs> PageInformationPanelVisibleChanged;
        event EventHandler<EventArgs> DisplaysPanelVisibleChanged;
    }
}