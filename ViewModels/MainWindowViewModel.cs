using AvaloniaEdit.Editing;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using IniConfigTroubleshooting.Services;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaEdit.Document;

namespace IniConfigTroubleshooting.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Fields
    [ObservableProperty] private TextDocument? _sourceDocument;
    #endregion

    #region Commands
    [RelayCommand]
    private void CopyMouse(TextArea textArea)
        => ApplicationCommands.Copy.Execute(null, textArea);

    [RelayCommand]
    private void CutMouseCommand(TextArea textArea)
        => ApplicationCommands.Cut.Execute(null, textArea);

    [RelayCommand]
    private void PasteMouseCommand(TextArea textArea)
        => ApplicationCommands.Paste.Execute(null, textArea);

    [RelayCommand]
    private void SelectAllMouseCommand(TextArea textArea)
        => ApplicationCommands.SelectAll.Execute(null, textArea);

    // Undo Status is not given back to disable its item in ContextFlyout; therefore it's not being used yet.
    [RelayCommand]
    private void UndoMouseCommand(TextArea textArea)
        => ApplicationCommands.Undo.Execute(null, textArea);

    // Redo Status is not given back to disable its item in ContextFlyout; therefore it's not being used yet.
    [RelayCommand]
    private void RedoMouseCommand(TextArea textArea)
        => ApplicationCommands.Redo.Execute(null, textArea);

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>()
            ?? throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        await using var readStream = await file.OpenReadAsync();
        using var reader = new StreamReader(readStream);
        SourceDocument = new TextDocument(await reader.ReadToEndAsync(token));
    }
    #endregion
}
