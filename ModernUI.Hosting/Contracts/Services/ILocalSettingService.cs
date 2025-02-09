using System.Threading;
using System.Threading.Tasks;

using ModernUI.Hosting.Contracts.Services.Abstractions;
using ModernUI.Hosting.Models;

namespace ModernUI.Hosting.Contracts.Services;

/// <summary>
///     Specifies the contract for a service that provides methods to persist and restore settings.
/// </summary>
public interface ILocalSettingService : IPersistAndRestoreService
{

    /// <summary>
    ///     Gets or adds the setting from the specified <paramref name="category"/> using the specified <paramref name="key"/> asynchronously.
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
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     An instance of the <see cref="LocalSetting{T}" /> class for the specified <paramref name="category"/> and <paramref name="key"/>.
    /// </returns>
    Task<LocalSetting<T>> GetOrAddAsync<T>(string category, string key, T? initialValue = default, CancellationToken cancellationToken = default);


    /// <summary>
    ///     Removes the setting from the specified <paramref name="category"/> using the specified <paramref name="key"/> asynchronously.
    /// </summary>
    /// <param name="category">
    ///     The category to which the setting belongs.
    /// </param>
    /// <param name="key">
    ///     The key of the setting.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the setting was removed; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> RemoveAsync(string category, string key, CancellationToken cancellationToken = default);
}