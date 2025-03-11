using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace IniConfigTroubleshooting.Services;

/// <summary>
/// Service class for handling file operations such as opening and saving files.
/// </summary>
public class FilesService(Window target) : IFilesService
{
    private readonly Window _target = target;

    /// <summary>
    /// Opens a file picker dialog to select a file.
    /// </summary>
    /// <returns>The selected file or null if no file is selected.</returns>
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    /// <summary>
    /// Opens a file picker dialog to save a file.
    /// </summary>
    /// <returns>The file to be saved.</returns>
    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }
}