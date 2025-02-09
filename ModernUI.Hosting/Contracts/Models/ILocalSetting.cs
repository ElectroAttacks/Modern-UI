namespace ModernUI.Hosting.Contracts.Models;

/// <summary>
///     Specifies the contract for a setting that notifies when its value changes and can be serialized.
/// </summary>
public interface ILocalSetting
{

    /// <summary>
    ///     Gets whether the settings value equals <see langword="default"/>.
    /// </summary>
    bool IsDefaultValue { get; }
}
