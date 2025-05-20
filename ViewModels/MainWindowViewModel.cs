using System.Text;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IniConfigTroubleshooting.Services;
using IniConfigTroubleshooting.Views;
using AvaloniaEdit.Document;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace IniConfigTroubleshooting.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Ini Config Troubleshooting";

    [ObservableProperty]
    private TextDocument _sourceDocument = new("Please, load file :)");

    private TextEditor? _editor;
    private RedSquiggleRenderer? _squiggleRenderer;
    private RegistryOptions? _registryOptions;
    private string? _iniScopeName;

    public void AttachEditor(TextEditor editor)
    {
        _editor = editor;
        _editor.Document = SourceDocument;
        _editor.TextChanged += OnEditorTextChanged;
        InitTextMate();
        UpdateSquiggle();
    }

    private void InitTextMate()
    {
        _registryOptions = new RegistryOptions(ThemeName.Light);
        _iniScopeName = _registryOptions.GetScopeByExtension(".ini");
        if (_editor is null || _registryOptions is null || _iniScopeName is null)
            return;
        _editor.InstallTextMate(_registryOptions)
              .SetGrammar(_iniScopeName);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (_editor?.Document is not null)
            SourceDocument = _editor.Document;
        UpdateSquiggle();
    }

    private void UpdateSquiggle()
    {
        if (_editor is null) return;
        _squiggleRenderer?.Detach(_editor);
        _squiggleRenderer = new RedSquiggleRenderer(_editor.Document);
        _squiggleRenderer.Attach(_editor);
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        var filesService = App.Current?.Services?.GetService<IFilesService>()
            ?? throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        var tempFilePath = TransformFileEncoding(file.Path.LocalPath);

        try
        {
            var content = await File.ReadAllTextAsync(tempFilePath, Encoding.UTF8);
            SourceDocument = new TextDocument(content);
            if (_editor is null) return;
            _editor.Document = SourceDocument;
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }

    private string TransformFileEncoding(string file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var fileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, fileContent, Encoding.UTF8);
        return tempFilePath;
    }

    [RelayCommand]
    private void CopyMouse(TextArea textArea) => ApplicationCommands.Copy.Execute(null, textArea);
    [RelayCommand]
    private void CutMouse(TextArea textArea) => ApplicationCommands.Cut.Execute(null, textArea);
    [RelayCommand]
    private void PasteMouse(TextArea textArea) => ApplicationCommands.Paste.Execute(null, textArea);
    [RelayCommand]
    private void SelectAllMouse(TextArea textArea) => ApplicationCommands.SelectAll.Execute(null, textArea);
    [RelayCommand]
    private void UndoMouse(TextArea textArea) => ApplicationCommands.Undo.Execute(null, textArea);
    [RelayCommand]
    private void RedoMouse(TextArea textArea) => ApplicationCommands.Redo.Execute(null, textArea);
}
