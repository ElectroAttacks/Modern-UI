using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Contracts.Services.Abstractions;

using Nito.AsyncEx;

namespace ModernUI.Hosting.Services.Abstractions;

/// <summary>
///     Provides a service to a read-only collection of data that can be persisted and restored.
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
public abstract class ReadOnlyPersistAndRestoreService<TSelf, TKey, TValue> : IReadOnlyPersistAndRestoreService
    where TSelf : ReadOnlyPersistAndRestoreService<TSelf, TKey, TValue>
    where TKey : notnull
{

    #region Fields & Properties

    /// <summary>
    ///     The completion source for the persist operation.
    /// </summary>
    protected TaskCompletionSource<Exception?>? _persistCompletionSource;

    /// <summary>
    ///     The completion source for the restore operation.
    /// </summary>
    protected TaskCompletionSource<Exception?>? _restoreCompletionSource;

    /// <summary>
    ///     The data to be persisted and restored.
    /// </summary>
    protected readonly ConcurrentDictionary<TKey, TValue> _data;

    /// <summary>
    ///     The reader-writer lock to synchronize access to the data.
    /// </summary>
    protected readonly AsyncReaderWriterLock _readerWriterLock;

    /// <summary>
    ///     The options to be used for JSON serialization and deserialization.
    /// </summary>
    protected readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    ///     The file service to be used.
    /// </summary>
    protected readonly IFileService _fileService;

    /// <summary>
    ///     The logger to be used.
    /// </summary>
    protected readonly ILogger _logger;


    /// <inheritdoc />
    public TimeSpan UpdateInterval
    {
        get;

        set
        {
            if (value != field)
            {
                OnPropertyChanging();

                field = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public DateTime LastUpdate
    {
        get;

        protected set
        {
            if (value != field)
            {
                OnPropertyChanging();

                field = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public string DatabasePath
    {
        get;


        set
        {
            if (value != field)
            {
                OnPropertyChanging();

                field = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public string DatabaseFileName
    {
        get;


        set
        {
            if (value != field)
            {
                OnPropertyChanging();

                field = value;

                OnPropertyChanged();
            }
        }
    }

    #endregion


    #region Events & Delegates

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    ///     Raises the <see cref="PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that is changing.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
        => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));


    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that has changed.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion


    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyPersistAndRestoreService{TSelf, TKey, TValue}"/> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to be used.
    /// </param>
    /// <param name="fileService">
    ///     The file service to be used.
    /// </param>
    /// <param name="configuration">
    ///     The configuration settings to bind to the service.
    /// </param>
    /// <param name="setup">
    ///     The optional setup to be performed.
    /// </param>
    protected ReadOnlyPersistAndRestoreService(ILogger<TSelf> logger, IFileService fileService, IConfiguration configuration, Action<ReadOnlyPersistAndRestoreService<TSelf, TKey, TValue>>? setup = default)
    {
        _data = new ConcurrentDictionary<TKey, TValue>();
        _readerWriterLock = new AsyncReaderWriterLock();
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.KebabCaseLower,
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };
        _fileService = fileService;
        _logger = logger;

        DatabasePath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        DatabaseFileName = $"{typeof(TSelf).Name}.json";

        ConfigurationBinder.Bind(configuration.GetSection(typeof(TSelf).Name), this);

        setup?.Invoke(this);
    }

    #endregion


    /// <inheritdoc />
    public virtual async Task<Exception?> RestoreAsync(CancellationToken cancellationToken)
    {
        await WaitForCompletionAsync(_restoreCompletionSource, "restoring")
            .ConfigureAwait(false);

        _restoreCompletionSource = new TaskCompletionSource<Exception?>();

        _logger.LogDebug("{ServiceName} is restoring it's data.", typeof(TSelf).Name);

        try
        {
            if (_fileService.DirectoryExists(DatabasePath) && _fileService.FileExists(DatabasePath, DatabaseFileName))
            {
                using IDisposable writerLock = await _readerWriterLock.WriterLockAsync(cancellationToken).ConfigureAwait(false);

                _data.Clear();

                ConcurrentDictionary<TKey, TValue>? data = await _fileService
                    .ReadJsonAsync<ConcurrentDictionary<TKey, TValue>>(DatabasePath, DatabaseFileName, _jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (data is ConcurrentDictionary<TKey, TValue> { Count: > 0 })
                {
                    foreach (KeyValuePair<TKey, TValue> item in data)
                    {
                        _data.TryAdd(item.Key, item.Value);
                    }

                    await ProcessLocalFileLoaded(cancellationToken).ConfigureAwait(false);

                    _logger.LogDebug("{ServiceName} restored it's data successfully.", typeof(TSelf).Name);
                }
                else
                {
                    _logger.LogDebug("{ServiceName} has no data to restore.", typeof(TSelf).Name);
                }
            }
            else
            {
                _logger.LogDebug("{ServiceName} has no local file to restore.", typeof(TSelf).Name);

                await ProcessLocalFileMissing(cancellationToken).ConfigureAwait(false);
            }

            _restoreCompletionSource.TrySetResult(null);
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                _logger.LogDebug("{ServiceName} restore operation was canceled.", typeof(TSelf).Name);

                _restoreCompletionSource.TrySetCanceled(cancellationToken);
            }
            else
            {
                _logger.LogError(ex, "{ServiceName} restore operation failed.", typeof(TSelf).Name);

                _restoreCompletionSource.TrySetResult(ex);
            }
        }

        return await _restoreCompletionSource.Task.ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<Exception?> PersistAsync(CancellationToken cancellationToken)
    {
        await WaitForCompletionAsync(_restoreCompletionSource, "restoring")
            .ConfigureAwait(false);

        await WaitForCompletionAsync(_persistCompletionSource, "persisting")
            .ConfigureAwait(false);

        _persistCompletionSource = new TaskCompletionSource<Exception?>();

        _logger.LogDebug("{ServiceName} is persisting it's data.", typeof(TSelf).Name);

        try
        {
            using IDisposable readerLock = await _readerWriterLock.ReaderLockAsync(cancellationToken).ConfigureAwait(false);

            if (_fileService.GetLastWriteTime(DatabasePath, DatabaseFileName).Add(UpdateInterval) > DateTime.Now)
            {
                _logger.LogDebug("{ServiceName} does not need to persist it's data.", typeof(TSelf).Name);
            }
            else
            {
                if (!_data.IsEmpty)
                {
                    await _fileService
                        .WriteJsonAsync(DatabasePath, DatabaseFileName, _data.Where(ShouldPersist).ToDictionary(), _jsonOptions, cancellationToken)
                        .ConfigureAwait(false);

                    _logger.LogDebug("{ServiceName} persisted it's data successfully.", typeof(TSelf).Name);
                }
                else
                {
                    _logger.LogDebug("{ServiceName} has no data to persist.", typeof(TSelf).Name);
                }
            }

            _persistCompletionSource.TrySetResult(null);
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                _logger.LogDebug("{ServiceName} persist operation was canceled.", typeof(TSelf).Name);

                _persistCompletionSource.TrySetCanceled(cancellationToken);
            }
            else
            {
                _logger.LogError(ex, "{ServiceName} persist operation failed.", typeof(TSelf).Name);

                _persistCompletionSource.TrySetResult(ex);
            }
        }

        return await _persistCompletionSource.Task.ConfigureAwait(false);

    }


    /// <summary>
    ///     Invoked when the local file is missing.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    protected virtual async Task ProcessLocalFileMissing(CancellationToken cancellationToken)
        => await Task.CompletedTask.ConfigureAwait(false);


    /// <summary>
    ///     Invoked when the local file is loaded.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    protected virtual async Task ProcessLocalFileLoaded(CancellationToken cancellationToken)
    {
        LastUpdate = _fileService.GetLastWriteTime(DatabasePath, DatabaseFileName);

        await Task.CompletedTask.ConfigureAwait(false);
    }


    /// <summary>
    ///     Invoked to determine if the key-value pair should be persisted.
    /// </summary>
    /// <param name="pair">
    ///     The key-value pair to be persisted.
    /// </param>
    /// <returns>
    ///     A <see cref="bool"/> indicating whether the key-value pair should be persisted.
    /// </returns>
    protected virtual bool ShouldPersist(KeyValuePair<TKey, TValue> pair)
        => true;


    /// <summary>
    ///     Waits for the completion of the specified operation asynchronously.
    /// </summary>
    /// <param name="completionSource">
    ///     The completion source to be used.
    /// </param>
    /// <param name="operation">
    ///     The name of the operation.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    protected async Task WaitForCompletionAsync(TaskCompletionSource<Exception?>? completionSource, string operation)
    {
        if (completionSource is not null && !completionSource.Task.IsCompleted)
        {
            _logger.LogDebug("{ServiceName} is already {Operation} it's data.", typeof(TSelf).Name, operation);

            await completionSource.Task.ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

}