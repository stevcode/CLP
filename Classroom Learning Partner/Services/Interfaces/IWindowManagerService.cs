using System;

namespace Classroom_Learning_Partner.Services
{
    public interface IWindowManagerService
    {
        bool IsPageInformationPanelVisible { get; set; }

        event EventHandler<EventArgs> PageInformationPanelVisibleChanged;
    }
}