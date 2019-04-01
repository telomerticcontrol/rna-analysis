using System.Windows.Input;

namespace Bio.Views.Interfaces
{
    /// <summary>
    /// Sidebar interface to load panes into main BioBrowser view
    /// </summary>
    public interface ISidebarViewItem
    {
        /// <summary>
        /// Displayed on the titlebar or header of the expander
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The visual to display
        /// </summary>
        object Visual { get; }

        /// <summary>
        /// Whether this is floating or docked in the main UI
        /// </summary>
        bool IsDocked { get; set; }

        /// <summary>
        /// Command used to close the view
        /// </summary>
        ICommand CloseView { get; }
    }
}
