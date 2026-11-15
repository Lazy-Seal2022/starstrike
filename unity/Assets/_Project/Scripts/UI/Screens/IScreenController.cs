namespace StarStrike.UI
{
    /// <summary>
    /// Contract for decoupled UI screen controllers.
    /// Manages show/hide lifecycle without direct coupling between views.
    /// </summary>
    public interface IScreenController
    {
        /// <summary>
        /// Unique string identifier for this screen (e.g. "MainMenu", "HUD", "Settings").
        /// </summary>
        string ScreenId { get; }

        /// <summary>
        /// Whether the screen is currently active and visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Displays the screen view.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the screen view.
        /// </summary>
        void Hide();
    }
}
