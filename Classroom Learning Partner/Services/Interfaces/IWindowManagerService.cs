using System;

namespace Classroom_Learning_Partner.Services
{
    public interface IWindowManagerService
    {
        Panels LeftPanel { get; set; }
        Panels RightPanel { get; set; }

        event EventHandler<EventArgs> LeftPanelChanged;
        event EventHandler<EventArgs> RightPanelChanged;
    }
}