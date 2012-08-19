namespace Classroom_Learning_Partner.ViewModels
{
    public interface IDisplayViewModel
    {
        string DisplayName { get; }
        string DisplayID { get; }
        bool IsOnProjector { get; set; }

        void AddPageToDisplay(CLPPageViewModel page);
    }
}
