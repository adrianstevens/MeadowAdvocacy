namespace TravelClock.Core.Views
{
    /// <summary>
    /// Extended view interface for views that handle directional input directly.
    /// HandleNext/HandlePrevious return true if the press was consumed (no view switch),
    /// false to let ClockController perform the default navigation.
    /// </summary>
    public interface IInteractiveView : IClockView
    {
        bool HandleNext();
        bool HandlePrevious();
        void HandleUp();
        void HandleDown();
    }
}
