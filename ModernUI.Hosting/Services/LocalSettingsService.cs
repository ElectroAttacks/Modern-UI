using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModernUI.Hosting.Contracts.Models;
using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Models;

namespace ModernUI.Hosting.Services;

/// <summary>
///     Provides a service for managing local settings.
/// </summary>
public sealed class LocalSettingsService : ILocalSettingsService
{
    // Category => Settings => Value (JsonElement or Setting<T>)
    private readonly SortedDictionary<string, SortedDictionary<string, object>> _cache;

    private readonly ReaderWriterLockSlim _cacheLock;

    private readonly IFileService _fileService;

    private readonly ILogger _logger;

    private string? _filePath;


    /// <inheritdoc />
    public string FilePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_filePath))
                return _filePath;

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "settings.json");
        }
        private set
        {
            _filePath = value;
        }
    }

    /// <inheritdoc />
    public bool UseCompression { get; private set; }

    /// <inheritdoc />
    public bool OmitDefaultValues { get; private set; }

    /// <inheritdoc />
    public JsonSerializerOptions JsonOptions { get; private set; }

    /// <inheritdoc />
    public long Count { get; private set; }



    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalSettingsService" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to use for logging.
    /// </param>
    /// <param name="configuration">
    ///     The configuration to use for the settings.
    /// </param>
    /// <param name="fileService">
    ///     The file service to use for reading and writing the settings.
    /// </param>
    public LocalSettingsService(ILogger<LocalSettingsService> logger, IConfiguration configuration, IFileService fileService)
    {
        _cache = new SortedDictionary<string, SortedDictionary<string, object>>();
        _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _logger = logger;
        _fileService = fileService;

        JsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper };

        ConfigurationBinder.Bind(configuration.GetSection(nameof(LocalSettingsService)), this);
    }

    /// <summary>
    ///     Finalizes an instance of the <see cref="LocalSettingsService" /> class.
    /// </summary>
    ~LocalSettingsService()
    {
        _cacheLock?.Dispose();
    }



    /// <inheritdoc />
    public void Clear()
    {
        _cacheLock.EnterWriteLock();

        try
        {
            _cache.Clear();
            Count = 0;
        }
        finally
        {
            if (_cacheLock.IsWriteLockHeld)
                _cacheLock.ExitWriteLock();
        }
    }


    /// <inheritdoc />
    public bool Contains(string category, string key)
    {
        try
        {
            // Ensure the category and key are matching the JsonNamingPolicy from the cache.
            ConvertToJsonName(ref category);
            ConvertToJsonName(ref key);

            // Ensure read access for checking the settings.
            _cacheLock.EnterReadLock();

            return _cache.TryGetValue(category, out SortedDictionary<string, object>? settings) && settings.ContainsKey(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} failed to check the setting '{Category}.{Key}'.", nameof(LocalSettingsService), category, key);

            return false;
        }
        finally
        {
            if (_cacheLock.IsReadLockHeld)
                _cacheLock.ExitReadLock();
        }
    }


    /// <inheritdoc />
    public Setting<T> GetOrAdd<T>(string category, string key, T? initialValue = default)
    {
        try
        {
            // Ensure the category and key are matching the JsonNamingPolicy from the cache.
            ConvertToJsonName(ref category);
            ConvertToJsonName(ref key);

            // Ensure read access for retrieving the settings.
            _cacheLock.EnterUpgradeableReadLock();

            // Retrieve the category or create a new one.
            if (!_cache.TryGetValue(category, out SortedDictionary<string, object>? settings))
            {
                settings = new SortedDictionary<string, object>();

                _cacheLock.EnterWriteLock();

                _cache[category] = settings;
            }

            // Retrieve the setting or create a new one.
            if (!settings.TryGetValue(key, out object? setting) || setting is not Setting<T>)
            {
                _cacheLock.EnterWriteLock();

                settings[key] = setting is JsonElement jsonElement
                    ? jsonElement.Deserialize<Setting<T>>(JsonOptions) ?? new Setting<T>(initialValue)
                    : new Setting<T>(initialValue);
            }

            return (Setting<T>)setting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} failed to retrieve or add the setting '{Category}.{Key}'.", nameof(LocalSettingsService), category, key);

            return Setting<T>.Unregistered;
        }
        finally
        {
            if (_cacheLock.IsWriteLockHeld)
                _cacheLock.ExitWriteLock();

            if (_cacheLock.IsUpgradeableReadLockHeld)
                _cacheLock.ExitUpgradeableReadLock();
        }
    }


    /// <inheritdoc />
    public bool Remove(string category, string key)
    {
        try
        {
            // Ensure the category and key are matching the JsonNamingPolicy from the cache.
            ConvertToJsonName(ref category);
            ConvertToJsonName(ref key);

            // Ensure write access for removing the setting.
            _cacheLock.EnterWriteLock();

            // Remove the setting from the cache.
            if (_cache.TryGetValue(category, out SortedDictionary<string, object>? settings) && settings.Remove(key))
            {
                Count--;

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} failed to remove the setting '{Category}.{Key}'.", nameof(LocalSettingsService), category, key);

            return false;
        }
        finally
        {
            if (_cacheLock.IsWriteLockHeld)
                _cacheLock.ExitWriteLock();
        }
    }


    /// <inheritdoc />
    public async Task<bool> RestoreAsync(int millisecondsTimeout = 1000, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure write access for restoring the settings.
            if (!_cacheLock.TryEnterWriteLock(millisecondsTimeout))
                throw new InvalidOperationException("Failed to acquire a write lock for restoring the settings.");

            // Read the settings from the file.


            var restored = await _fileService.ReadJsonAsync<SortedDictionary<string, SortedDictionary<string, object>>>(FilePath, UseCompression, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            // Ensure the restored settings are valid.
            if (restored is not SortedDictionary<string, SortedDictionary<string, object>> { Count: > 0 })
            {
                _logger.LogInformation("The Service {ServiceName} has no settings to restore.", nameof(LocalSettingsService));

                return true;
            }

            // Manually add and count the restored settings.
            foreach (var (category, settings) in restored)
            {
                _cache[category] = settings;

                Count += settings.Count;
            }

            _logger.LogInformation("The Service {ServiceName} has successfully restored {Count} settings.", nameof(LocalSettingsService), Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} failed to restore the settings.", nameof(LocalSettingsService));

            return false;
        }
        finally
        {
            if (_cacheLock.IsWriteLockHeld)
                _cacheLock.ExitWriteLock();
        }
    }


    /// <inheritdoc />
    public async Task<bool> SaveAsync(int millisecondsTimeout = 1000, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure read access for saving the settings.
            if (!_cacheLock.TryEnterUpgradeableReadLock(millisecondsTimeout))
                throw new InvalidOperationException("Failed to acquire a read lock for saving the settings.");

            // Create a copy of the cache to filter out default values.
            var copy = new SortedDictionary<string, SortedDictionary<string, object>>();
            long count = 0;

            foreach (var (category, settings) in _cache)
            {
                var filteredSettings = new SortedDictionary<string, object>();

                foreach (var (key, entry) in settings)
                {
                    if (entry is ISetting { IsDefaultValue: true } setting && OmitDefaultValues)
                        continue;

                    filteredSettings[key] = entry;

                    count++;
                }

                if (filteredSettings.Count > 0)
                    copy[category] = filteredSettings;
            }

            // Save the settings to the file.
            await _fileService.WriteJsonAsync(FilePath, copy, UseCompression, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("The Service {ServiceName} has successfully saved {SavedCount}/{TotalCount} settings.", nameof(LocalSettingsService), count, Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} failed to save the settings.", nameof(LocalSettingsService));

            return false;
        }
        finally
        {
            if (_cacheLock.IsReadLockHeld)
                _cacheLock.ExitReadLock();
        }
    }



    /// <summary>
    ///     Converts the specified name with the JsonNamingPolicy from the <see cref="_fileService"/>.
    /// </summary>
    /// <param name="name">
    ///     The name to convert.
    /// </param>
    private void ConvertToJsonName(ref string name)
    {
        if (JsonOptions.PropertyNamingPolicy?.ConvertName(name) is not string jsonName) return;

        name = jsonName;
    }
}