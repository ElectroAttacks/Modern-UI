using System;
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
    ///     Gets the options to use for JSON serialization and deserialization.
    /// </summary>
    JsonSerializerOptions JsonOptions { get; }


    /// <summary>
    ///     Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
    /// </summary>
    /// <param name="path">
    ///     The path of the directory to remove.
    /// </param>
    /// <param name="recusive">
    ///     A value indicating whether to remove subdirectories and files in the directory.
    /// </param>
    void DeleteDirectory(string path, bool recusive);


    /// <summary>
    ///     Deletes the specified file.
    /// </summary>
    /// <param name="path">
    ///     The path to the file.
    /// </param>
    /// <param name="fileName">
    ///     The name of the file to delete.
    /// </param>
    void DeleteFile(string path, string fileName);


    /// <summary>
    ///     Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">
    ///     The path to the directory to check.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the directory exists; otherwise, <see langword="false"/>.
    /// </returns>
    bool DirectoryExists(string path);


    /// <summary>
    ///     Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">
    ///     The path to the file.
    /// </param>
    /// <param name="fileName">
    ///     The name of the file to check.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the file exists; otherwise, <see langword="false"/>.
    /// </returns>
    bool FileExists(string path, string fileName);


    /// <summary>
    ///     Gets the last write time of the specified file.
    /// </summary>
    /// <param name="path">
    ///     The path to the file.
    /// </param>
    /// <param name="fileName">
    ///     The name of the file.
    /// </param>
    /// <returns>
    ///     A <see cref="DateTime"/> instance representing the last write time of the file.
    /// </returns>
    DateTime GetLastWriteTime(string path, string fileName);


    /// <summary>
    ///     Reads a JSON file asynchronously and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to deserialize the JSON data to.
    /// </typeparam>
    /// <param name="path">
    ///     The path to the file.
    /// </param>
    /// <param name="fileName">
    ///     The name of the file to read.
    /// </param>
    /// <param name="jsonOptions">
    ///     The options to use for JSON serialization and deserialization.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     An <typeparamref name="T"/> instance representing the deserialized JSON data.
    /// </returns>
    Task<T?> ReadJsonAsync<T>(string path, string fileName, JsonSerializerOptions? jsonOptions = default, CancellationToken cancellationToken = default);


    /// <summary>
    ///     Writes the specified data to a JSON file asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of data to serialize to JSON.
    /// </typeparam>
    /// <param name="path">
    ///     The path to the file.
    /// </param>
    /// <param name="fileName">
    ///     The name of the file to write.
    /// </param>
    /// <param name="data">
    ///     The <typeparamref name="T"/> instance to serialize to JSON.
    /// </param>
    /// <param name="jsonOptions">
    ///     The options to use for JSON serialization and deserialization.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task WriteJsonAsync<T>(string path, string fileName, T data, JsonSerializerOptions? jsonOptions = default, CancellationToken cancellationToken = default);
}
