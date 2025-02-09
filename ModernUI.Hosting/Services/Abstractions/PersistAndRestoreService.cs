using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Contracts.Services.Abstractions;

namespace ModernUI.Hosting.Services.Abstractions;

/// <summary>
///     Provides a service to persist and restore data.
/// </summary>
/// <typeparam name="TSelf">
///     The type of the service itself.
/// </typeparam>
/// <typeparam name="TKey">
///     The type of the key used to identify the data.
/// </typeparam>
/// <typeparam name="TValue">
///     The type of the data to be persisted and restored.
/// </typeparam>
public abstract class PersistAndRestoreService<TSelf, TKey, TValue> : ReadOnlyPersistAndRestoreService<TSelf, TKey, TValue>, IPersistAndRestoreService
    where TSelf : PersistAndRestoreService<TSelf, TKey, TValue>
    where TKey : notnull
{

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersistAndRestoreService{TSelf, TKey, TValue}"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected PersistAndRestoreService(ILogger<TSelf> logger, IFileService fileService, IConfiguration configuration, Action<ReadOnlyPersistAndRestoreService<TSelf, TKey, TValue>>? setup = null)
        : base(logger, fileService, configuration, setup)
    {
    }



    /// <inheritdoc/>
    public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await WaitForCompletionAsync(_restoreCompletionSource, "restoring")
            .ConfigureAwait(false);

        using (await _readerWriterLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
        {
            _data.Clear();
        }
    }

}