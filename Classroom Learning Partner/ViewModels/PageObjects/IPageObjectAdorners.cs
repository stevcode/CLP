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
        /// Time (in seconds) after MouseEnter event when the adorner begins to fade in. 
        /// </summary>
        double OpenAdornerTimeOut { get; set; }

        /// <summary>
        /// Time (in seconds) after MouseLeave event when the adorner begins to fade out. 
        /// </summary>
        double CloseAdornerTimeOut { get; set; }

        bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown);

        //TODO: Steve - Add CanMousePassThrough boolean to allow the mouse to pass it's mouse enter event to pageObjects with a lower z-index below itself.
    }
}
