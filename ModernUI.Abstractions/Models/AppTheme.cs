namespace ModernUI.Abstractions.Models;

/// <summary>
///     Defines the available application themes.
/// </summary>
public enum AppTheme
{
    /// <summary>
    ///     Indicates that the theme should be determined by the system settings.
    /// </summary>
    SystemDefault,

    /// <summary>
    ///     A light theme with a white background and black foreground.
    /// </summary>
    Light,

    /// <summary>
    ///     A dark theme with a black background and white foreground.
    /// </summary>
    Dark,

    /// <summary>
    ///     Indicates that no theme is specified.
    /// </summary>
    Custom,
}