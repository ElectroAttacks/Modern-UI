using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ModernUI.Hosting.Contracts.Services;

namespace ModernUI.Hosting.Services;

/// <summary>
///     Provides a service for file operations.
/// </summary>
public sealed class FileService : IFileService
{

    private readonly ILogger _logger;



    /// <summary>
    ///     Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to use for logging.
    /// </param>
    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }



    /// <inheritdoc />
    public async Task<T?> ReadJsonAsync<T>(string combinedPath, bool useCompression, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file path and compression settings
            if (!IsPathValid(ref combinedPath, $".json{(useCompression ? ".gz" : "")}"))
            {
                _logger.LogWarning("The Service {ServiceName} could not validate the path {Path}.", nameof(FileService), combinedPath);

                return default;
            }

            // Validate if the file exists
            if (!File.Exists(combinedPath))
            {
                _logger.LogWarning("The Service {ServiceName} could not find the file {Path}.", nameof(FileService), combinedPath);

                return default;
            }

            // Create a stream to read the file
            Stream utf8Json = useCompression
                ? new GZipStream(File.OpenRead(combinedPath), CompressionMode.Decompress)
                : File.OpenRead(combinedPath);

            // Deserialize the content of the file
            await using (utf8Json.ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<T>(utf8Json, options, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} could not read the file {Path}.", nameof(FileService), combinedPath);

            return default;
        }
    }


    /// <inheritdoc />
    public async Task WriteJsonAsync<T>(string combinedPath, T value, bool useCompression, JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Validate file path and compression settings
            if (!IsPathValid(ref combinedPath, $".json{(useCompression ? ".gz" : "")}"))
            {
                _logger.LogWarning("The Service {ServiceName} could not validate the path {Path}.", nameof(FileService), combinedPath);

                return;
            }

            // Create a stream to write the file
            Stream utf8Json = useCompression
                ? new GZipStream(File.Create(combinedPath), CompressionMode.Compress)
                : File.Create(combinedPath);

            // Serialize the object to a file
            await using (utf8Json.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync(utf8Json, value, options, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Service {ServiceName} could not write the file {Path}.", nameof(FileService), combinedPath);
        }
    }


    /// <summary>
    ///     Validates the specified <paramref name="combinedPath"/> and ensures the directory exists.
    /// </summary>
    /// <param name="combinedPath">
    ///     The combined path to validate.
    /// </param>
    /// <param name="requiredExtension">
    ///     The required extension of the file.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the path is valid; otherwise, <see langword="false"/>.
    /// </returns>
    private bool IsPathValid(ref string combinedPath, string requiredExtension)
    {
        string directory = Path.GetDirectoryName(combinedPath);
        string currentExtension = Path.GetExtension(combinedPath);

        // Ensure the file extension matches the required extension
        if (!string.Equals(currentExtension, requiredExtension, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("The file extension {Extension} does not match the required extension {RequiredExtension}.", currentExtension, requiredExtension);

            combinedPath = Path.ChangeExtension(combinedPath, requiredExtension);
        }

        // Ensure the directory exists
        if (!Directory.Exists(directory))
        {
            _logger.LogDebug("The directory {Directory} does not exist. Creating it now.", directory);

            Directory.CreateDirectory(directory);
        }

        return Directory.Exists(directory);
    }
}
