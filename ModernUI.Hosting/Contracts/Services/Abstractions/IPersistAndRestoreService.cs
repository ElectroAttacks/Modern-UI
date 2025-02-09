using System.Threading;
using System.Threading.Tasks;

namespace ModernUI.Hosting.Contracts.Services.Abstractions;

/// <summary>
///     Specifies the contract for a service that provides persistence and restoration of data.
/// </summary>
public interface IPersistAndRestoreService : IReadOnlyPersistAndRestoreService
{

    /// <summary>
    ///     Clears the data from the persistent storage asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task ClearAsync(CancellationToken cancellationToken = default);
}