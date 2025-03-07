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
    public void CopyMouseCommand(TextArea textArea) => ApplicationCommands.Copy.Execute(null, textArea);

    public void CutMouseCommand(TextArea textArea) => ApplicationCommands.Cut.Execute(null, textArea);

    public void PasteMouseCommand(TextArea textArea) => ApplicationCommands.Paste.Execute(null, textArea);

    public void SelectAllMouseCommand(TextArea textArea) => ApplicationCommands.SelectAll.Execute(null, textArea);

    // Undo Status is not given back to disable it's item in ContextFlyout; therefore it's not being used yet.
    public void UndoMouseCommand(TextArea textArea) => ApplicationCommands.Undo.Execute(null, textArea);

    [ObservableProperty] private TextDocument? _sourceDocument;

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>();
        if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        await using var readStream = await file.OpenReadAsync();
        using var reader = new StreamReader(readStream);
        SourceDocument = new TextDocument(await reader.ReadToEndAsync(token));
    }
}