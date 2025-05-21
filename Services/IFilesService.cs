using Avalonia.Platform.Storage;

namespace IniConfigTroubleshooting.Services;

/// <summary>
/// Interface for file operations such as opening and saving files.
/// </summary>
public interface IFilesService
{
    /// <summary>
    /// Opens a file picker dialog to select a file.
    /// </summary>
    /// <returns>The selected file or null if no file is selected.</returns>
    Task<IStorageFile?> OpenFileAsync();

    /// <summary>
    /// Opens a file picker dialog to save a file.
    /// </summary>
    /// <returns>The file to be saved.</returns>
    Task<IStorageFile?> SaveFileAsync();
}
