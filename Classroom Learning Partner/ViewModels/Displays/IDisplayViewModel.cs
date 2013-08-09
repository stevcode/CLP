using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public interface IDisplayViewModel
    {
        string DisplayName { get; }             //Type of Display.
        string DisplayID { get; }               //Unique ID of Display.
        bool IsOnProjector { get; set; }        //If Display is currently being projected.

        void AddPageToDisplay(ICLPPage page);    //Method to add a new page to this display.
    }
}
