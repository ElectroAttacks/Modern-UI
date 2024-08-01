using System.Threading;
using System.Threading.Tasks;

using ModernUI.Abstractions.Models;

namespace ModernUI.Abstractions.Contracts.Services;

/// <summary>
///     Specifies the contract for a service that provides access to local settings.
/// </summary>
public interface ILocalSettingsService
{

    /// <summary>
    ///     Gets a value indicating whether the local settings have been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     Gets the total number of stored settings.
    /// </summary>
    long Count { get; }

    /// <summary>
    ///     Asynchronously restores the local settings from the backing store.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task RestoreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously saves the local settings to the backing store.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the setting from the specified <paramref name="category"/> using the specified <paramref name="key"/>.
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
    Setting<T> Get<T>(string category, string key, T? initialValue = default);
}
