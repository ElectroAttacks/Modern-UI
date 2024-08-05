using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ModernUI.Hosting.Contracts.Services;

/// <summary>
///     Specifies the contract for a service that provides file operations.
/// </summary>
public interface IFileService
{

    /// <summary>
    ///     Asynchronously reads the JSON content of the file at the specified <paramref name="combinedPath"/> and deserializes it to the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to which the JSON content should be deserialized.
    /// </typeparam>
    /// <param name="combinedPath">
    ///     The combined path to the file (including it's name) to read.
    /// </param>
    /// <param name="useCompression">
    ///     The value that determines whether the file should be decompressed before reading.
    /// </param>
    /// <param name="serializerOptions">
    ///     The options to use when serializing and deserializing JSON.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <typeparamref name="T"/> instance deserialized from the JSON content of the file.
    /// </returns>
    Task<T?> ReadJsonAsync<T>(string combinedPath, bool useCompression, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously writes the specified <paramref name="value"/> to the file at the specified <paramref name="combinedPath"/> as JSON content.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value to write.
    /// </typeparam>
    /// <param name="combinedPath">
    ///     The combined path to the file (including it's name) to write.
    /// </param>
    /// <param name="value">
    ///     The value to write as JSON content to the file.
    /// </param>
    /// <param name="useCompression">
    ///     The value that determines whether the file should be compressed before writing.
    /// </param>
    /// <param name="serializerOptions">
    ///     The options to use when serializing and deserializing JSON.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task WriteJsonAsync<T>(string combinedPath, T value, bool useCompression, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken);
}
