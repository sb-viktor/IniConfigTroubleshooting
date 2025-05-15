using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IniConfigTroubleshooting.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace IniConfigTroubleshooting.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Fields
    [ObservableProperty] private TextDocument _sourceDocument = new("Please, load file :)");

    [ObservableProperty] private string _title = "Ini Config Troubleshooting";
    #endregion

    #region Commands
    [RelayCommand]
    private void CopyMouse(TextArea textArea)
        => ApplicationCommands.Copy.Execute(null, textArea);

    [RelayCommand]
    private void CutMouse(TextArea textArea)
        => ApplicationCommands.Cut.Execute(null, textArea);

    [RelayCommand]
    private void PasteMouse(TextArea textArea)
        => ApplicationCommands.Paste.Execute(null, textArea);

    [RelayCommand]
    private void SelectAllMouse(TextArea textArea)
        => ApplicationCommands.SelectAll.Execute(null, textArea);

    // Undo Status is not given back to disable its item in ContextFlyout; therefore it's not being used yet.
    [RelayCommand]
    private void UndoMouse(TextArea textArea)
        => ApplicationCommands.Undo.Execute(null, textArea);

    // Redo Status is not given back to disable its item in ContextFlyout; therefore it's not being used yet.
    [RelayCommand]
    private void RedoMouse(TextArea textArea)
        => ApplicationCommands.Redo.Execute(null, textArea);

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>()
            ?? throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        await ProcessFile(file, token);
    }

    private async Task ProcessFile(Avalonia.Platform.Storage.IStorageFile file, CancellationToken token)
    {
        Title = file.Name;

        // Transform file encoding from ANSI to UTF-8  
        string tempFilePath = TransformFileEncoding(file.Path.LocalPath);

        try
        {
            // Use FileStream to open the file for reading  
            await using FileStream readStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new(readStream);
            string content = await reader.ReadToEndAsync(token);
            SourceDocument = new TextDocument(content);

            // Build a configuration object from INI file  
            IConfiguration config = new ConfigurationBuilder()
                .AddIniFile(tempFilePath)
                .Build();
        }
        finally
        {
            // Delete the temp file after loading
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                    // Handle the exception if needed
                    // For example, log the error or show a message to the user
                    throw new IOException($"Failed to delete temporary file: {tempFilePath}");
                }
            }
        }
    }
    #endregion

    private string TransformFileEncoding(string file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string fileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, fileContent, Encoding.UTF8);
        return tempFilePath;
    }

}
