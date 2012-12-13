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
        bool IsPinned { get; set; }
        bool IsVisible { get; set; }
        PanelLocation Location { get; set; }
    }
}
