using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classroom_Learning_Partner.ViewModels
{
    public interface IPageObjectAdorners
    {
        //Instantly Show or Hide Adorners
        bool IsAdornerVisible { get; set; }

        //Allow Adorners to Show or Hide via MouseOver/MouseLeave events
        bool IsMouseOverShowEnabled { get; set; }

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves over the 
        /// adorned control (or the adorner) when the adorner begins to fade in.
        /// </summary>
        double OpenAdornerTimeOut { get; set; }

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves away from the 
        /// adorned control (or the adorner) when the adorner begins to fade out.
        /// </summary>
        double CloseAdornerTimeOut { get; set; }

        bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown);
    }
}
