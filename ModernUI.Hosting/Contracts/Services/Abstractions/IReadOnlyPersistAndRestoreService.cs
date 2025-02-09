using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ModernUI.Hosting.Contracts.Services.Abstractions;

/// <summary>
///     Specifies the contract for a service that provides read-only persistence and restoration of data.
/// </summary>
public interface IReadOnlyPersistAndRestoreService : INotifyPropertyChanging, INotifyPropertyChanged
{

    /// <summary>
    ///     Gets or sets the interval to update the local database.
    /// </summary>
    TimeSpan UpdateInterval { get; set; }

    /// <summary>
    ///     Gets the last update time of the database.
    /// </summary>
    DateTime LastUpdate { get; }

    /// <summary>
    ///     Gets or sets the path to the database.
    /// </summary>
    string DatabasePath { get; set; }

    /// <summary>
    ///     Gets or sets the file name of the database.
    /// </summary>
    string DatabaseFileName { get; set; }


    /// <summary>
    ///     Restores the data from the persistent storage asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     <see langword="null"/> if the operation was successful; otherwise, an <see cref="Exception"/> instance.
    /// </returns>
    Task<Exception?> RestoreAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Persists the data to the persistent storage asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     <see langword="null"/> if the operation was successful; otherwise, an <see cref="Exception"/> instance.
    /// </returns>
    Task<Exception?> PersistAsync(CancellationToken cancellationToken);
}