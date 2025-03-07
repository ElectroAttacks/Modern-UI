using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ModernUI.Hosting.Contracts.Models;
using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Models;
using ModernUI.Hosting.Services.Abstractions;

namespace ModernUI.Hosting.Services;

/// <summary>
///     Provides a service to persist and restore settings.
/// </summary>
public sealed class LocalSettingService : ReadOnlyPersistAndRestoreService<LocalSettingService, string, ConcurrentDictionary<string, object>>, ILocalSettingService
{

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalSettingService"/> class.
    /// </summary>
    /// <inheritdoc/>
    public LocalSettingService(IServiceProvider serviceProvider, Action<LocalSettingService>? setup = null)
        : base(serviceProvider, setup)
    {
    }

    #endregion


    /// <inheritdoc/>
    public LocalSetting<T> GetOrAdd<T>(string category, string key, T? initialValue = default)
    {
        WaitForCompletionAsync(_restoreCompletionSource, "restoring").Wait();

        using IDisposable writeLock = _readerWriterLock.WriterLock();

        ConvertToJsonName(ref category, ref key);

        if (!_data.TryGetValue(category, out ConcurrentDictionary<string, object>? settings))
        {
            settings = new ConcurrentDictionary<string, object>();
            _data[category] = settings;
        }

        if (!settings.TryGetValue(key, out object? setting) || setting is not LocalSetting<T>)
        {
            // When the JSON Serializer cannot create a type, it returns a JsonElement.
            if (setting is JsonElement jsonElement)
                setting = jsonElement.Deserialize<LocalSetting<T>>(_jsonOptions);

            setting ??= new LocalSetting<T>(initialValue);

            settings[key] = setting;
        }

        return (LocalSetting<T>)setting;
    }


    /// <inheritdoc/>
    public async Task<LocalSetting<T>> GetOrAddAsync<T>(string category, string key, T? initialValue = default, CancellationToken cancellationToken = default)
    {
        await WaitForCompletionAsync(_restoreCompletionSource, "restoring")
            .ConfigureAwait(false);

        using IDisposable writeLock = await _readerWriterLock.WriterLockAsync(cancellationToken).ConfigureAwait(false);

        ConvertToJsonName(ref category, ref key);

        if (!_data.TryGetValue(category, out ConcurrentDictionary<string, object>? settings))
        {
            settings = new ConcurrentDictionary<string, object>();

            _data[category] = settings;
        }

        if (!settings.TryGetValue(key, out object? setting) || setting is not LocalSetting<T>)
        {
            // When the JSON Serializer cannot create a type, it returns a JsonElement.
            if (setting is JsonElement jsonElement)
                setting = jsonElement.Deserialize<LocalSetting<T>>(_jsonOptions);

            setting ??= new LocalSetting<T>(initialValue);

            settings[key] = setting;
        }

        return (LocalSetting<T>)setting;
    }


    /// <inheritdoc/>
    protected override bool ShouldPersist(KeyValuePair<string, ConcurrentDictionary<string, object>> entry)
    {
        var filteredSettings = new ConcurrentDictionary<string, object>();

        foreach ((string key, object setting) in entry.Value)
        {
            if (setting is not ILocalSetting { IsDefaultValue: true })
                filteredSettings[key] = setting;
        }

        return !filteredSettings.IsEmpty;
    }


    /// <summary>
    ///     Converts the category and key to the JSON name.
    /// </summary>
    /// <param name="category">
    ///     The category to convert.
    /// </param>
    /// <param name="key">
    ///     The key to convert.
    /// </param>
    private void ConvertToJsonName(ref string category, ref string key)
    {
        category = _jsonOptions.DictionaryKeyPolicy?.ConvertName(category) ?? category;
        key = _jsonOptions.DictionaryKeyPolicy?.ConvertName(key) ?? key;
    }

}
