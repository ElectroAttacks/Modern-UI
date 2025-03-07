using System.ComponentModel;
using System.Windows.Media;

using ModernUI.Hosting.Wpf.Models;

namespace ModernUI.Hosting.Wpf.Contracts.Services;

/// <summary>
///     Specifies the contract for a service that provides methods to manage the application theme and accent color.
/// </summary>
public interface IThemeService : INotifyPropertyChanging, INotifyPropertyChanged
{

    /// <summary>
    ///     Gets or sets the theme of the application.
    /// </summary>
    ApplicationTheme Theme { get; set; }

    /// <summary>
    ///     Gets or sets the accent color of the application.
    /// </summary>
    Color AccentColor { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the system accent color should be used.
    /// </summary>
    bool UseSystemAccent { get; set; }
}
