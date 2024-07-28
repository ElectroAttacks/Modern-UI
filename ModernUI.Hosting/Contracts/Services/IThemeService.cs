using System.ComponentModel;

using ModernUI.Hosting.Models;

namespace ModernUI.Hosting.Contracts.Services;

/// <summary>
///     Defines a contract for a service that manages the theme of the application.
/// </summary>
public interface IThemeService : INotifyPropertyChanging, INotifyPropertyChanged
{

    /// <summary>
    ///     Gets or sets the current theme of the application.
    /// </summary>
    AppTheme SelectedTheme { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the accent color should be synchronized with the system accent color.
    /// </summary>
    bool SynchronizeAccent { get; set; }

    /// <summary>
    ///     Sets the accent color of the application.
    /// </summary>
    /// <param name="accentColor">
    ///     The accent color to set.
    /// </param>
    void SetAccent(object accentColor);
}
