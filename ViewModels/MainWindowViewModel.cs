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
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace IniConfigTroubleshooting.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Ini Config Troubleshooting";

    [ObservableProperty]
    private TextDocument _sourceDocument = new("Please, load file :)");

    [ObservableProperty]
    private TextEditor? _editor = new();

    private RedSquiggleRenderer? _squiggleRenderer;
    private RegistryOptions? _registryOptions;
    private string? _iniScopeName;

    private int _startLine = 0;
    private int _endLine = 0;

    public MainWindowViewModel()
    {
        Editor = new TextEditor
        {
            Document = SourceDocument,
            FontFamily = new Avalonia.Media.FontFamily("Cascadia Code,Consolas,Menlo,Monospace"),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            ShowLineNumbers = true,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible
        };
        Editor.TextChanged += OnEditorTextChanged;
        InitTextMate();
        UpdateSquiggle(_startLine, _endLine);
    }

    private void InitTextMate()
    {
        _registryOptions = new RegistryOptions(ThemeName.Light);
        _iniScopeName = _registryOptions.GetScopeByExtension(".ini");
        if (Editor is null || _registryOptions is null || _iniScopeName is null)
            return;
        Editor.InstallTextMate(_registryOptions)
              .SetGrammar(_iniScopeName);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (Editor?.Document is not null)
            SourceDocument = Editor.Document;
        UpdateSquiggle(_startLine, _endLine);
    }

    private void UpdateSquiggle(int startLine = 0, int endLine = 0)
    {
        if (Editor is null) return;
        _squiggleRenderer?.Detach(Editor);
        _squiggleRenderer = new RedSquiggleRenderer(Editor.Document, startLine, endLine);
        _squiggleRenderer.Attach(Editor);
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
            if (Editor is not null)
                Editor.Document = SourceDocument;

            TryBuildCondiguration(tempFilePath);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }

    private void TryBuildCondiguration(string filePath)
    {
        try
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddIniFile(filePath)
                .Build();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading INI file: {ex.InnerException?.Message}");
            Debug.WriteLine($"Document line count: {SourceDocument.LineCount}");


            _startLine = 157;
            _endLine = 157;
            UpdateSquiggle(_startLine, _endLine);
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
