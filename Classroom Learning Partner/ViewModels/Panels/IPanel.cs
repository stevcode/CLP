using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum PanelLocation
    {
        Left,
        Right,
        Top,
        Bottom,
        Floating
    }

    public interface IPanel
    {
        string PanelName { get; }
        bool IsPinned { get; set; }
        bool IsVisible { get; set; }
        bool IsResizable { get; set; }
        double InitialWidth { get; }
        PanelLocation Location { get; set; }
        IPanel LinkedPanel { get; set; }
    }
}
