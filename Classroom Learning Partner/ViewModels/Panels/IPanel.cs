namespace Classroom_Learning_Partner.ViewModels
{
    public enum PanelLocations
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
        bool IsResizable { get; set; }
        double InitialLength { get; }
        double MinLength { get; set; }
        double Length { get; set; }
        PanelLocations Location { get; set; }
    }
}
