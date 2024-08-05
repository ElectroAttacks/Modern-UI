using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ModernUI.Hosting.Models;

namespace ModernUI.Hosting.Contracts.Services;

/// <summary>
///     Specifies the contract for a service that provides access to local settings.
/// </summary>
public interface ILocalSettingsService
{

    /// <summary>
    ///     Gets the path to the file (including it's name) where the settings are stored.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    ///     Gets whether the settings should be compressed when stored.
    /// </summary>
    bool UseCompression { get; }

    /// <summary>
    ///     Gets whether default values should be omitted when stored.
    /// </summary>
    bool OmitDefaultValues { get; }

    /// <summary>
    ///     Gets the options to use when serializing and deserializing JSON.
    /// </summary>
    JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    ///     Gets the total number of stored settings.
    /// </summary>
    long Count { get; }


    /// <summary>
    ///     Clears all settings from the backing store.
    /// </summary>
    void Clear();

    /// <summary>
    ///     Determines whether the specified setting exists in the specified <paramref name="category"/> using the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="category">
    ///     The category to which the setting belongs.
    /// </param>
    /// <param name="key">
    ///     The key of the setting.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the setting exists; otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(string category, string key);

    /// <summary>
    ///     Gets or adds the setting from the specified <paramref name="category"/> using the specified <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value which is stored in the setting.
    /// </typeparam>
    /// <param name="category">
    ///     The category to which the setting belongs.
    /// </param>
    /// <param name="key">
    ///     The key of the setting.
    /// </param>
    /// <param name="initialValue">
    ///     The value to use if the setting does not exist.
    /// </param>
    /// <returns>
    ///     An instance of the <see cref="Setting{T}" /> class for the specified <paramref name="category"/> and <paramref name="key"/>.
    /// </returns>
    Setting<T> GetOrAdd<T>(string category, string key, T? initialValue = default);

    /// <summary>
    ///     Removes the setting from the specified <paramref name="category"/> using the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="category">
    ///     The category to which the setting belongs.
    /// </param>
    /// <param name="key">
    ///     The key of the setting.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the setting was removed; otherwise, <see langword="false"/>.
    /// </returns>
    bool Remove(string category, string key);

    /// <summary>
    ///     Asynchronously restores the local settings from the backing store.
    /// </summary>
    /// <param name="millisecondsTimeout">
    ///     The number of milliseconds to wait before the operation times out.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the settings were restored successfully; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> RestoreAsync(int millisecondsTimeout = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously saves the local settings to the backing store.
    /// </summary>
    /// <param name="millisecondsTimeout">
    ///     The number of milliseconds to wait before the operation times out.
    /// </param>
    /// <param name="cancellationToken">1
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the settings were saved successfully; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> SaveAsync(int millisecondsTimeout = 1000, CancellationToken cancellationToken = default);
}
