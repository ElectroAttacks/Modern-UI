using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModernUI.Hosting.Contracts.Services;

namespace ModernUI.Hosting.Services;

/// <summary>
///     Provides a service to work with files.
/// </summary>
public sealed class FileService : IFileService
{

    #region Fields & Properties

    private readonly ILogger _logger;


    /// <inheritdoc/>
    public JsonSerializerOptions JsonOptions
    {
        get;

        private set;
    }

    #endregion


    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    /// <param name="configuration">
    ///     The configuration settings to bind to the service.
    /// </param>
    /// <param name="setup">
    ///     The optional setup to be performed.
    /// </param>
    public FileService(ILogger<FileService> logger, IConfiguration configuration, Action<FileService>? setup = default)
    {
        _logger = logger;

        JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        ConfigurationBinder.Bind(configuration.GetSection(nameof(FileService)), this);

        setup?.Invoke(this);
    }

    #endregion


    /// <inheritdoc/>
    public void DeleteDirectory(string path, bool recusive)
    {
        try
        {
            Directory.Delete(path, recusive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to delete the directory.", nameof(FileService));
        }
    }


    /// <inheritdoc/>
    public void DeleteFile(string path, string fileName)
    {
        try
        {
            File.Delete(Path.Combine(path, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to delete the file.", nameof(FileService));
        }
    }


    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Directory.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to check if the directory exists.", nameof(FileService));


            return false;
        }
    }


    /// <inheritdoc/>
    public bool FileExists(string path, string fileName)
    {
        try
        {
            return File.Exists(Path.Combine(path, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to check if the file exists.", nameof(FileService));

            return false;
        }
    }


    /// <inheritdoc/>
    public DateTime GetLastWriteTime(string path, string fileName)
    {
        try
        {
            return File.GetLastWriteTime(Path.Combine(path, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to get the last write time.", nameof(FileService));

            return DateTime.MinValue;
        }
    }


    /// <inheritdoc/>
    public async Task<T?> ReadJsonAsync<T>(string path, string fileName, JsonSerializerOptions? jsonOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using Stream fileStream = File.OpenRead(Path.Combine(path, fileName));
            jsonOptions ??= JsonOptions;

            if (fileName.EndsWith(".gz"))
            {
                await using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);

                return await JsonSerializer.DeserializeAsync<T>(gzipStream, jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                return await JsonSerializer.DeserializeAsync<T>(fileStream, jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to read the file.", nameof(FileService));

            return default;
        }
    }


    /// <inheritdoc/>
    public async Task WriteJsonAsync<T>(string path, string fileName, T data, JsonSerializerOptions? jsonOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using Stream fileStream = File.Create(Path.Combine(path, fileName));
            jsonOptions ??= JsonOptions;

            if (fileName.EndsWith(".gz"))
            {
                await using GZipStream gzipStream = new(fileStream, CompressionMode.Compress);

                await JsonSerializer.SerializeAsync(gzipStream, data, jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await JsonSerializer.SerializeAsync(fileStream, data, jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} failed to write the file.", nameof(FileService));
        }
    }

}
