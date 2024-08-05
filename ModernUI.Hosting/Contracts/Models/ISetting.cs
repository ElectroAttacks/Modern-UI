namespace ModernUI.Hosting.Contracts.Models;

/// <summary>
///     Specifies the contract for a setting that notifies when its value changes and can be serialized.
/// </summary>
internal interface ISetting
{

    /// <summary>
    ///     Gets whether the setting is registered in the cache.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    ///     Gets whether the settings value equals <see langword="default"/>.
    /// </summary>
    bool IsDefaultValue { get; }
}